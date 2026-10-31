// The classroom server: the Maaloomatiia Games menu and every built game, served from
// this computer, with Claude answered through Claude Code (`claude -p`) and the Claude
// subscription it is signed in with. No key, no hosting. From the repository root:
//
//   node server/local.mjs        then open http://localhost:8080 and type the code "local"
//
// It is the local twin of the Netlify edge functions next door: the same sign-in
// (lib/session.js), the same request checks (lib/validate.js) and the same answer shape
// (the Messages API's), so a game cannot tell the two apart.
//
//   ACCESS_CODE=...   the code to type on the menu (default: local)
//   PORT=...          default 8080
//   HOST=0.0.0.0      lets other computers on the network open the games (every Claude
//                     call then runs on this computer's subscription)
import { spawn } from "node:child_process";
import { randomBytes } from "node:crypto";
import { existsSync, readdirSync, statSync } from "node:fs";
import { readFile } from "node:fs/promises";
import { createServer } from "node:http";
import { homedir, tmpdir } from "node:os";
import { delimiter, extname, join, normalize, sep } from "node:path";
import { fileURLToPath } from "node:url";
import { issue, verify, readCookie, setCookieHeader, clearCookieHeader, codesMatch } from "./lib/session.js";
import { checkClaudeBody, MAX_BODY_CHARS } from "./lib/validate.js";

const ROOT = fileURLToPath(new URL("..", import.meta.url));
const SITE = join(ROOT, "site");
const PORT = Number(process.env.PORT) || 8080;
const HOST = process.env.HOST || "127.0.0.1";
const ACCESS_CODE = process.env.ACCESS_CODE || "local";
// A fresh secret on every start, so sessions end with the server; set SESSION_SECRET to keep them.
const SECRET = process.env.SESSION_SECRET || randomBytes(32).toString("hex");
const TIMEOUT_MS = 120000;
const cookieOptions = { secure: false }; // plain http on localhost

const TYPES = {
  ".html": "text/html; charset=utf-8",
  ".js": "text/javascript",
  ".mjs": "text/javascript",
  ".css": "text/css",
  ".json": "application/json",
  ".wasm": "application/wasm",
  ".data": "application/octet-stream",
  ".unityweb": "application/octet-stream",
  ".png": "image/png",
  ".jpg": "image/jpeg",
  ".webp": "image/webp",
  ".svg": "image/svg+xml",
  ".ico": "image/x-icon",
  ".woff2": "font/woff2",
  ".mp3": "audio/mpeg",
  ".wav": "audio/wav",
};

// Claude Code's program: on the PATH, in a usual install folder, or the copy inside the
// VS Code extension (newest first). The games look in the same places on the desktop.
function findClaude() {
  const home = homedir();
  const dirs = [...(process.env.PATH ?? "").split(delimiter), join(home, ".local/bin"),
                join(home, ".claude/local"), "/opt/homebrew/bin", "/usr/local/bin"];
  const extensions = join(home, ".vscode/extensions");
  if (existsSync(extensions))
    dirs.push(...readdirSync(extensions)
      .filter((name) => name.startsWith("anthropic.claude-code-"))
      .map((name) => join(extensions, name))
      .sort((a, b) => statSync(b).mtimeMs - statSync(a).mtimeMs)
      .map((dir) => join(dir, "resources", "native-binary")));
  return dirs.filter(Boolean).flatMap((dir) => [join(dir, "claude"), join(dir, "claude.exe")]).find((path) => existsSync(path));
}
const CLAUDE = findClaude();

// `claude -p` takes one prompt, so the earlier turns go in as a transcript before the
// new one.
function transcript(messages) {
  const latest = messages.at(-1).content;
  if (messages.length === 1) return latest;
  let text = "Earlier in this conversation:\n\n";
  for (const m of messages.slice(0, -1)) text += `${m.role === "user" ? "User" : "You"}: ${m.content}\n\n`;
  return text + "---\n\nThe user now says:\n\n" + latest;
}

// No tools, no saved session, none of the user's Claude Code settings, and the
// subscription rather than any ANTHROPIC_API_KEY in this shell.
function askClaude(model, effort, system, messages) {
  const { ANTHROPIC_API_KEY, ...env } = process.env;
  const args = ["-p", "--system-prompt", system, "--tools", "", "--output-format", "json", "--model", model,
                "--effort", effort, "--no-session-persistence", "--setting-sources", ""];
  return new Promise((resolve) => {
    const child = spawn(CLAUDE, args, { cwd: tmpdir(), env });
    let out = "";
    let err = "";
    let timedOut = false;
    child.stdout.on("data", (chunk) => (out += chunk));
    child.stderr.on("data", (chunk) => (err += chunk));
    const timer = setTimeout(() => { timedOut = true; child.kill(); }, TIMEOUT_MS);
    child.on("error", (e) => (err += e.message));
    child.on("close", () => {
      clearTimeout(timer);
      if (timedOut) return resolve({ error: "Claude Code took too long", status: 504 });
      let result;
      try {
        result = JSON.parse(out);
      } catch {}
      const text = (result?.result ?? "").trim();
      const u = result?.usage ?? {};
      const usage = {
        input_tokens: u.input_tokens ?? 0,
        cache_read_input_tokens: u.cache_read_input_tokens ?? 0,
        cache_creation_input_tokens: u.cache_creation_input_tokens ?? 0,
        output_tokens: u.output_tokens ?? 0,
      };
      if (result && !result.is_error && text) return resolve({ text, usage });
      console.error("Claude Code:", (out || err || "no answer").slice(0, 2000));
      const limit = /limit/i.test(text);
      const error = limit ? "The Claude subscription has hit its usage limit"
                  : /login/i.test(text + err) ? "Claude Code is not signed in (run claude, then /login)"
                  : "Claude Code returned an error";
      resolve({ error, status: limit ? 429 : 502 });
    });
    child.stdin.end(transcript(messages));
  });
}

// ---- The API: the same routes as the Netlify edge functions ----

async function readJson(req, res) {
  let raw = "";
  for await (const chunk of req) {
    raw += chunk;
    if (raw.length > MAX_BODY_CHARS) { json(res, 413, { error: "request too large" }); return undefined; }
  }
  try {
    return JSON.parse(raw || "{}");
  } catch {
    json(res, 400, { error: "bad json" });
    return undefined;
  }
}

async function sessionOf(req) {
  return verify(SECRET, readCookie(req.headers.cookie));
}

async function signIn(req, res) {
  const body = await readJson(req, res);
  if (body === undefined) return;
  if (!codesMatch(ACCESS_CODE, body.code)) return json(res, 401, { error: "That code is not valid." });
  const token = await issue(SECRET, "class");
  json(res, 200, { ok: true, subject: "class" }, { "set-cookie": setCookieHeader(token, cookieOptions) });
}

function signOut(req, res) {
  json(res, 200, { ok: true }, { "set-cookie": clearCookieHeader(cookieOptions) });
}

async function me(req, res) {
  const session = await sessionOf(req);
  json(res, 200, session ? { signedIn: true, subject: session.subject, expires: session.exp } : { signedIn: false });
}

async function claude(req, res) {
  if (!(await sessionOf(req))) return json(res, 401, { error: "not signed in" });
  const body = await readJson(req, res);
  if (body === undefined) return;
  if (body.check) return json(res, 200, { ok: true, claude: Boolean(CLAUDE) });

  const checked = checkClaudeBody(body);
  if (checked.error) return json(res, 400, { error: checked.error });
  if (!CLAUDE) return json(res, 502, { error: "Claude Code not found on the computer serving this page" });
  const { model, effort, system, messages } = checked;

  const started = Date.now();
  const answer = await askClaude(model, effort, system, messages);
  console.log(`claude ${model} ${effort} ${messages.length} message(s): ${answer.error ?? "ok"} in ${((Date.now() - started) / 1000).toFixed(1)} s`);
  if (answer.error) return json(res, answer.status, { error: answer.error });
  json(res, 200, { type: "message", role: "assistant", model, stop_reason: "end_turn",
                   content: [{ type: "text", text: answer.text }], usage: answer.usage });
}

const progressLog = [];
async function progress(req, res) {
  const session = await sessionOf(req);
  if (!session) return json(res, 401, { error: "not signed in" });
  const body = await readJson(req, res);
  if (body === undefined) return;
  if (typeof body.game !== "string" || typeof body.result !== "string") return json(res, 400, { error: "expected game and result" });
  progressLog.push({ at: new Date().toISOString(), subject: session.subject, ...body });
  console.log(`progress ${body.game}: ${body.result}${body.stars ? ` (${body.stars} stars)` : ""}`);
  json(res, 200, { ok: true, stored: false });
}

const routes = {
  "POST /api/session": signIn,
  "DELETE /api/session": signOut,
  "GET /api/me": me,
  "POST /api/claude": claude,
  "POST /api/progress": progress,
};

function json(res, status, data, headers = {}) {
  res.writeHead(status, { "content-type": "application/json", "cache-control": "no-store", ...headers });
  res.end(JSON.stringify(data));
}

// ---- The site and the games ----

async function file(req, res, pathname) {
  let full = normalize(join(SITE, pathname));
  if (full !== SITE && !full.startsWith(SITE + sep)) return send(res, 403, "Forbidden");
  if (pathname.endsWith("/")) full = join(full, "index.html");

  let body;
  try {
    body = await readFile(full);
  } catch {
    if (!pathname.endsWith("/") && !extname(pathname)) {
      res.writeHead(302, { location: pathname + "/" }); // /games/claude-bomb -> /games/claude-bomb/
      return res.end();
    }
    if (/^\/games\/[^/]+\/$/.test(pathname))
      return send(res, 404, `No build of this game yet. Build it into site${pathname} first (see README.md).`);
    return send(res, 404, "Not found");
  }

  // Unity's compressed files (*.br, *.gz): let the browser unpack them when it can;
  // otherwise the build's own decompression fallback does (*.unityweb).
  const ext = extname(full);
  const encoded = ext === ".br" ? "br" : ext === ".gz" ? "gzip" : null;
  const type = TYPES[encoded ? extname(full.slice(0, -ext.length)) : ext] ?? "application/octet-stream";
  const headers = { "content-type": type, "cache-control": pathname.includes("/Build/") ? "public, max-age=31536000, immutable" : "no-cache" };
  if (encoded && (req.headers["accept-encoding"] ?? "").includes(encoded)) headers["content-encoding"] = encoded;
  res.writeHead(200, headers);
  res.end(body);
}

function send(res, status, text) {
  res.writeHead(status, { "content-type": "text/plain; charset=utf-8" });
  res.end(text + "\n");
}

createServer((req, res) => {
  const url = new URL(req.url, "http://localhost");
  const pathname = decodeURIComponent(url.pathname);
  const route = routes[`${req.method} ${pathname}`];
  const handler = route ? route(req, res)
                : pathname.startsWith("/api/") ? Promise.resolve(json(res, 404, { error: "no such route" }))
                : file(req, res, pathname);
  Promise.resolve(handler).catch((err) => {
    console.error(err);
    if (!res.headersSent) send(res, 500, "Server error");
  });
}).listen(PORT, HOST, () => {
  console.log(`Maaloomatiia Games: http://${HOST === "0.0.0.0" ? "localhost" : HOST}:${PORT}  (code: ${ACCESS_CODE})`);
  console.log(CLAUDE ? `Claude through Claude Code: ${CLAUDE}` : "Claude Code not found: install it and sign in with `claude`, then /login");
});
