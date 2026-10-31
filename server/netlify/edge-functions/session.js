// Sign in: the menu page posts the code typed by the student; a valid code gets a
// session cookie that every game's Claude calls then carry. DELETE signs out.
//
// Settings, in the Netlify dashboard only (Site configuration > Environment variables):
//   ACCESS_CODE      the class code students type on the menu
//   SESSION_SECRET   any long random string; signs the session cookies
import { issue, setCookieHeader, clearCookieHeader, codesMatch } from "../../lib/session.js";

export default async (request) => {
  const json = (status, data, headers = {}) =>
    new Response(JSON.stringify(data), { status, headers: { "content-type": "application/json", ...headers } });

  if (request.method === "DELETE") return json(200, { ok: true }, { "set-cookie": clearCookieHeader() });
  if (request.method !== "POST") return json(405, { error: "POST only" });

  const secret = Netlify.env.get("SESSION_SECRET") ?? "";
  const expected = Netlify.env.get("ACCESS_CODE") ?? "";
  if (!secret || !expected) return json(503, { error: "The platform is not set up yet (ACCESS_CODE and SESSION_SECRET)." });

  let body;
  try {
    body = await request.json();
  } catch {
    return json(400, { error: "bad json" });
  }
  if (!codesMatch(expected, body?.code)) return json(401, { error: "That code is not valid." });

  const token = await issue(secret, "class");
  return json(200, { ok: true, subject: "class" }, { "set-cookie": setCookieHeader(token) });
};

export const config = { path: "/api/session" };
