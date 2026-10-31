// Who is signed in, for the menu page: it skips the code screen when a session is
// still valid and shows the games straight away.
import { verify, readCookie } from "../../lib/session.js";

export default async (request) => {
  const session = await verify(Netlify.env.get("SESSION_SECRET") ?? "", readCookie(request.headers.get("cookie")));
  const data = session ? { signedIn: true, subject: session.subject, expires: session.exp } : { signedIn: false };
  return new Response(JSON.stringify(data), { status: 200, headers: { "content-type": "application/json", "cache-control": "no-store" } });
};

export const config = { path: "/api/me" };
