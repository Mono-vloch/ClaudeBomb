// A game reporting how a run ended: { game, result, stars }. Today it is only
// acknowledged, so the games can already send it; storing it per student comes with
// the student codes (Netlify Blobs keyed by the session's subject).
import { verify, readCookie } from "../../lib/session.js";

export default async (request) => {
  const json = (status, data) =>
    new Response(JSON.stringify(data), { status, headers: { "content-type": "application/json" } });
  if (request.method !== "POST") return json(405, { error: "POST only" });
  const session = await verify(Netlify.env.get("SESSION_SECRET") ?? "", readCookie(request.headers.get("cookie")));
  if (!session) return json(401, { error: "not signed in" });
  let body;
  try {
    body = await request.json();
  } catch {
    return json(400, { error: "bad json" });
  }
  if (typeof body?.game !== "string" || typeof body?.result !== "string") return json(400, { error: "expected game and result" });
  return json(200, { ok: true, stored: false });
};

export const config = { path: "/api/progress" };
