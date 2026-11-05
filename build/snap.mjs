// Walks through the menu site in a headless Chrome and saves a picture of each step,
// so a change can be checked without opening a browser by hand. Needs Google Chrome.
//
//   node build/snap.mjs http://localhost:8080 /tmp/shots [code] [game-id]
//
// Pictures: door, door with a wrong code, the stage (first game, then the next, then a
// game's sheet), the whole menu, the stage and a sheet on a phone, and, with a game id,
// that game's page after it has had a minute to load.
import { spawn } from "node:child_process";
import { mkdirSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";

const [base = "http://localhost:8080", out = "/tmp/shots", code = "local", gameId = ""] = process.argv.slice(2);
const CHROME = process.env.CHROME || "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome";
const PORT = 9333;
mkdirSync(out, { recursive: true });

const chrome = spawn(CHROME, [
  "--headless=new", `--remote-debugging-port=${PORT}`, `--user-data-dir=${join(tmpdir(), "mg-snap-profile")}`,
  "--no-first-run", "--no-default-browser-check", "--hide-scrollbars", "--window-size=1440,900",
  "--use-angle=swiftshader", "--enable-unsafe-swiftshader", "--autoplay-policy=no-user-gesture-required",
  "about:blank",
], { stdio: "ignore" });
process.on("exit", () => chrome.kill());

const wait = (ms) => new Promise((r) => setTimeout(r, ms));

async function target() {
  for (let i = 0; i < 50; i++) {
    try {
      const list = await (await fetch(`http://127.0.0.1:${PORT}/json`)).json();
      const page = list.find((t) => t.type === "page");
      if (page) return page.webSocketDebuggerUrl;
    } catch {}
    await wait(200);
  }
  throw new Error("Chrome did not start");
}

const ws = new WebSocket(await target());
await new Promise((r) => (ws.onopen = r));
let nextId = 1;
const pending = new Map();
ws.onmessage = (e) => {
  const msg = JSON.parse(e.data);
  if (msg.id && pending.has(msg.id)) { pending.get(msg.id)(msg); pending.delete(msg.id); }
};
function send(method, params = {}) {
  const id = nextId++;
  ws.send(JSON.stringify({ id, method, params }));
  return new Promise((resolve, reject) => pending.set(id, (m) => (m.error ? reject(new Error(m.error.message)) : resolve(m.result))));
}
async function evaluate(expression) {
  const r = await send("Runtime.evaluate", { expression, awaitPromise: true, returnByValue: true });
  return r.result?.value;
}
async function shot(name) {
  const r = await send("Page.captureScreenshot", { format: "png" });
  writeFileSync(join(out, name + ".png"), Buffer.from(r.data, "base64"));
  console.log("saved", name);
}
async function viewport(width, height, mobile = false) {
  await send("Emulation.setDeviceMetricsOverride", { width, height, deviceScaleFactor: 1, mobile });
}
const click = (selector) => evaluate(`(() => { const n = document.querySelector(${JSON.stringify(selector)}); if (n) n.click(); return Boolean(n); })()`);
const closeSheet = () => evaluate(`(() => { const s = document.querySelector('#sheet'); if (s && s.open) s.close(); })()`);

await send("Page.enable");
await send("Runtime.enable");
await viewport(1440, 900);

// 1. The door
await send("Page.navigate", { url: base + "/" });
await wait(1800);
await shot("1-door");

// 2. A wrong code
await evaluate(`(() => { const i = document.querySelector('#code'); i.value = 'wrong'; document.querySelector('#signin').requestSubmit(); })()`);
await wait(900);
await shot("2-door-wrong-code");

// 3. The right code, then the stage: the first game, the next one, and a game's sheet
await evaluate(`(() => { const i = document.querySelector('#code'); i.value = ${JSON.stringify(code)}; document.querySelector('#signin').requestSubmit(); })()`);
await wait(2400);
await shot("3-menu");
await click("#next");
await wait(1200);
await shot("3a-menu-next-game");
await click(".slide--active .btn--ghost");
await wait(700);
await shot("3b-menu-game-sheet");
await closeSheet();
await wait(300);
await viewport(1440, 2000);
await shot("3c-menu-full");
await viewport(390, 844, true);
await send("Page.navigate", { url: base + "/" });
await wait(2400);
await shot("4-menu-phone");
await click(".slide--active .btn--ghost");
await wait(700);
await shot("4a-menu-phone-game-sheet");
await closeSheet();
await viewport(1440, 900);

// 4. A game, if asked: first the way back in after a sign-in has run out, then the game itself
if (gameId) {
  await send("Page.navigate", { url: `${base}/?signin=1&return=/games/${gameId}/` });
  await wait(1500);
  await shot("5-door-sign-in-again");
  await evaluate(`(() => { const i = document.querySelector('#code'); i.value = ${JSON.stringify(code)}; document.querySelector('#signin').requestSubmit(); })()`);
  await wait(2500);
  console.log("after sign-in again, the page is", await evaluate("location.pathname"));
  await send("Page.navigate", { url: `${base}/games/${gameId}/` });
  for (let i = 0; i < 12; i++) {
    await wait(10000);
    const done = await evaluate(`(() => { const l = document.querySelector('#loading'); return !l || l.style.display === 'none' || getComputedStyle(l).opacity === '0'; })()`);
    if (done) break;
  }
  await wait(4000);
  await shot("6-game-" + gameId);
}

ws.close();
chrome.kill();
console.log("done:", out);
