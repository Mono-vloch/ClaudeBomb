// Plays the web build on this computer: serves Web/public and answers the game's
// Claude calls at /api/claude, on the same address as the game. From the project
// folder:  node Web/serve-local.mjs
//
// Claude is reached through Claude Code (`claude -p`), with the Claude subscription it
// is signed in with, so no API key is needed. Otherwise it behaves like the Netlify key
// keeper (netlify/edge-functions/claude.js): the same access-code check, model and
// effort, and an answer in the API's shape, which is what the game reads. The access
// code is ACCESS_CODE, or "local" if unset.
import { spawn } from "node:child_process";
import { createServer } from "node:http";
import { existsSync, readdirSync, statSync } from "node:fs";
import { readFile } from "node:fs/promises";
import { homedir, tmpdir } from "node:os";
import { delimiter, extname, join, normalize, sep } from "node:path";
import { fileURLToPath } from "node:url";

const PROJECT = fileURLToPath(new URL("..", import.meta.url));
const PUBLIC = join(PROJECT, "Web", "public");
const PORT = 8080;
const ACCESS_CODE = process.env.ACCESS_CODE || "local";
const MODEL = "claude-sonnet-5"; // as in the key keeper and ClaudeClient.cs
const MAX_BODY_CHARS = 120000;
const TIMEOUT_MS = 90000;

const TYPES = {
  ".html": "text/html; charset=utf-8",
  ".js": "text/javascript",
  ".wasm": "application/wasm",
  ".data": "application/octet-stream",
  ".json": "application/json",
  ".css": "text/css",
  ".png": "image/png",
  ".jpg": "image/jpeg",
  ".ico": "image/x-icon",
  ".svg": "image/svg+xml",
};

// Claude Code's program: on the PATH, in a usual install folder, or the copy inside the
// VS Code extension (newest first). ClaudeClient.cs looks in the same places.
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
  return dirs.filter(Boolean).map((dir) => join(dir, "claude")).find((path) => existsSync(path));
}
const CLAUDE = findClaude();

// `claude -p` takes one prompt, so the earlier turns go in as a transcript before the
// new one - the same text ClaudeClient.Transcript builds in the Editor.
function transcript(messages) {
  const latest = messages.at(-1).content;
  if (messages.length === 1) return latest;
  const earlier = messages.slice(0, -1).map((m) => `${m.role === "user" ? "Player" : "You"}: ${m.content}\n\n`);
  return `Earlier in this conversation:\n\n${earlier.join("")}---\n\nThe player now says:\n\n${latest}`;
}

// No tools, no saved session, none of the user's Claude Code settings, and the
// subscription rather than any ANTHROPIC_API_KEY in this shell.
function askClaude(system, messages) {
  const { ANTHROPIC_API_KEY, ...env } = process.env;
  const args = ["-p", "--system-prompt", system, "--tools", "", "--output-format", "json", "--model", MODEL,
                "--effort", "low", "--no-session-persistence", "--setting-sources", ""];
  return new Promise((resolve) => {
    const child = spawn(CLAUDE, args, { cwd: tmpdir(), env });
    let out = "";
    let err = "";
    child.stdout.on("data", (chunk) => (out += chunk));
    child.stderr.on("data", (chunk) => (err += chunk));
    const timer = setTimeout(() => child.kill(), TIMEOUT_MS);
    child.on("error", (e) => (err += e.message));
    child.on("close", () => {
      clearTimeout(timer);
      let result;
      try {
        result = JSON.parse(out);
      } catch {}
      const text = (result?.result ?? "").trim();
      if (result && !result.is_error && text) return resolve({ text });
      console.error("Claude Code:", out || err || "no answer");
      resolve({ error: text || err.trim() || "no answer", limit: /limit/i.test(text) });
    });
    child.stdin.end(transcript(messages));
  });
}

async function api(req, res) {
  if (req.method !== "POST") return json(res, 405, { error: "POST only" });
  if ((req.headers["x-access-code"] ?? "").trim().toLowerCase() !== ACCESS_CODE.toLowerCase())
    return json(res, 401, { error: "access code rejected" });

  let raw = "";
  for await (const chunk of req) raw += chunk;
  if (raw.length > MAX_BODY_CHARS) return json(res, 413, { error: "request too large" });

  let body;
  try {
    body = JSON.parse(raw);
  } catch {
    return json(res, 400, { error: "bad json" });
  }
  if (body.check) return json(res, 200, { ok: true }); // the start panel validating a code

  const { system, messages } = body;
  if (typeof system !== "string" || !Array.isArray(messages) || messages.length === 0 ||
      messages.some((m) => typeof m.content !== "string"))
    return json(res, 400, { error: "expected system and messages" });
  if (!CLAUDE) return json(res, 502, { error: "Claude Code not found" });

  const answer = await askClaude(system, messages);
  if (answer.error) return json(res, answer.limit ? 429 : 502, { error: answer.error });
  json(res, 200, { type: "message", role: "assistant", content: [{ type: "text", text: answer.text }] });
}

function json(res, status, data) {
  res.writeHead(status, { "content-type": "application/json" });
  res.end(JSON.stringify(data));
}

async function file(req, res) {
  const path = decodeURIComponent(new URL(req.url, "http://localhost").pathname);
  const full = normalize(join(PUBLIC, path.endsWith("/") ? path + "index.html" : path));
  if (full !== PUBLIC && !full.startsWith(PUBLIC + sep)) return send(res, 403, "Forbidden");

  let body;
  try {
    body = await readFile(full);
  } catch {
    if (path === "/") return send(res, 404, "No web build yet. In Unity: Claude Bomb > Build Web Version");
    return send(res, 404, "Not found");
  }

  // Unity's Brotli files (*.wasm.br, ...): let the browser unpack them when it can;
  // otherwise the build's own decompression fallback does.
  const brotli = full.endsWith(".br");
  const headers = { "content-type": TYPES[extname(brotli ? full.slice(0, -3) : full)] ?? "application/octet-stream" };
  if (brotli && /\bbr\b/.test(req.headers["accept-encoding"] ?? "")) headers["content-encoding"] = "br";
  res.writeHead(200, headers);
  res.end(body);
}

function send(res, status, text) {
  res.writeHead(status, { "content-type": "text/plain; charset=utf-8" });
  res.end(text + "\n");
}

createServer((req, res) => {
  const handler = req.url.startsWith("/api/claude") ? api : file;
  handler(req, res).catch((err) => {
    console.error(err);
    if (!res.headersSent) send(res, 500, "Server error");
  });
}).listen(PORT, "127.0.0.1", () => {
  console.log(`Claude Bomb: http://localhost:${PORT} (access code: ${ACCESS_CODE})`);
  console.log(CLAUDE ? `Claude via Claude Code: ${CLAUDE}` : "Claude Code not found: install it and sign in with `claude`");
});
