// The platform's sign-in, shared by the Netlify edge functions and the local classroom
// server. A student enters a code once on the menu page; the server answers with a
// signed session in a cookie, and the browser sends that cookie with every call a game
// makes to /api/claude on the same site. The games never see the code.
//
// Today the subject is "class" (one code for everyone). When every student gets their
// own code, the subject becomes the learner's id and nothing else changes.
//
// Only Web APIs are used (crypto.subtle, TextEncoder, btoa) so this file runs unchanged
// in Node 22 and in Netlify's edge runtime (Deno).

export const COOKIE = "mg_session";
const VERSION = "v1";
export const MAX_AGE_SECONDS = 12 * 60 * 60;

const encoder = new TextEncoder();
const decoder = new TextDecoder();

function base64url(bytes) {
  let binary = "";
  for (const b of bytes) binary += String.fromCharCode(b);
  return btoa(binary).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
}

function fromBase64url(text) {
  const padded = text.replace(/-/g, "+").replace(/_/g, "/") + "=".repeat((4 - (text.length % 4)) % 4);
  const binary = atob(padded);
  return Uint8Array.from(binary, (c) => c.charCodeAt(0));
}

async function sign(secret, text) {
  const key = await crypto.subtle.importKey("raw", encoder.encode(secret), { name: "HMAC", hash: "SHA-256" }, false, ["sign"]);
  return base64url(new Uint8Array(await crypto.subtle.sign("HMAC", key, encoder.encode(text))));
}

// Compares two strings without leaking where they differ.
function same(a, b) {
  if (typeof a !== "string" || typeof b !== "string" || a.length !== b.length) return false;
  let diff = 0;
  for (let i = 0; i < a.length; i++) diff |= a.charCodeAt(i) ^ b.charCodeAt(i);
  return diff === 0;
}

// A new session for a subject ("class", or later a learner id).
export async function issue(secret, subject, now = Date.now()) {
  const exp = Math.floor(now / 1000) + MAX_AGE_SECONDS;
  const payload = `${VERSION}.${base64url(encoder.encode(subject))}.${exp}`;
  return `${payload}.${await sign(secret, payload)}`;
}

// The session behind a token, or null if it is missing, forged or expired.
export async function verify(secret, token, now = Date.now()) {
  if (!secret || typeof token !== "string") return null;
  const parts = token.split(".");
  if (parts.length !== 4 || parts[0] !== VERSION) return null;
  const payload = parts.slice(0, 3).join(".");
  if (!same(await sign(secret, payload), parts[3])) return null;
  const exp = Number(parts[2]);
  if (!Number.isFinite(exp) || exp * 1000 < now) return null;
  try {
    return { subject: decoder.decode(fromBase64url(parts[1])), exp };
  } catch {
    return null;
  }
}

// The session cookie out of a Cookie header.
export function readCookie(header, name = COOKIE) {
  for (const part of String(header ?? "").split(";")) {
    const [k, ...rest] = part.trim().split("=");
    if (k === name) return rest.join("=");
  }
  return null;
}

// HttpOnly: no script on the page can read it. SameSite=Lax: sent on the games' own
// calls, never on a cross-site request. Secure on the published site, not on localhost.
export function setCookieHeader(token, { secure = true } = {}) {
  return `${COOKIE}=${token}; Path=/; Max-Age=${MAX_AGE_SECONDS}; HttpOnly; SameSite=Lax${secure ? "; Secure" : ""}`;
}

export function clearCookieHeader({ secure = true } = {}) {
  return `${COOKIE}=; Path=/; Max-Age=0; HttpOnly; SameSite=Lax${secure ? "; Secure" : ""}`;
}

// A code is typed from a card or read out in a room, so the usual misreads are mapped
// before anything is compared: upper case, O to 0, I to 1, and spaces dropped. The
// expected code goes through the same mapping, so a class code may contain any letter.
export function normalizeCode(code) {
  return String(code ?? "").toUpperCase().replace(/O/g, "0").replace(/I/g, "1").replace(/[\s_]+/g, "");
}

export function codesMatch(expected, given) {
  const a = normalizeCode(expected);
  return a.length > 0 && same(a, normalizeCode(given));
}
