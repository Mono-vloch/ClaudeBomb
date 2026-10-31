// The key keeper. Every game on the platform sends its Claude calls here, on its own
// site, so the browser carries the session cookie by itself. This function checks that
// session, adds the Anthropic key (an environment variable, never in a game) and forwards
// the conversation to Claude. The games are the same origin as this function, so there
// are no CORS headers: a page on another site cannot use it.
//
// Runs as a Netlify Edge Function: those can wait up to 40 seconds for Claude's reply,
// where regular functions stop after 10.
//
// Settings, in the Netlify dashboard only:
//   ANTHROPIC_API_KEY   the key the keeper uses
//   SESSION_SECRET      signs the session cookies (see session.js)
import { verify, readCookie } from "../../lib/session.js";
import { checkClaudeBody, MAX_BODY_CHARS, MAX_TOKENS } from "../../lib/validate.js";

export default async (request) => {
  const json = (status, data) =>
    new Response(JSON.stringify(data), { status, headers: { "content-type": "application/json" } });

  if (request.method !== "POST") return json(405, { error: "POST only" });

  const session = await verify(Netlify.env.get("SESSION_SECRET") ?? "", readCookie(request.headers.get("cookie")));
  if (!session) return json(401, { error: "not signed in" });

  const raw = await request.text();
  if (raw.length > MAX_BODY_CHARS) return json(413, { error: "request too large" });
  let body;
  try {
    body = JSON.parse(raw);
  } catch {
    return json(400, { error: "bad json" });
  }
  if (body.check) return json(200, { ok: true, claude: true }); // a game asking whether Claude is reachable

  const checked = checkClaudeBody(body);
  if (checked.error) return json(400, { error: checked.error });
  const { model, effort, system, messages } = checked;

  const upstream = await fetch("https://api.anthropic.com/v1/messages", {
    method: "POST",
    headers: {
      "content-type": "application/json",
      "x-api-key": Netlify.env.get("ANTHROPIC_API_KEY") ?? "",
      "anthropic-version": "2023-06-01",
    },
    body: JSON.stringify({
      model,
      max_tokens: MAX_TOKENS,
      system,
      messages,
      output_config: { effort },
      // Caches the repeated prefix: cheaper, and cached input doesn't count against
      // the account's per-minute limit when a whole class plays at once.
      cache_control: { type: "ephemeral" },
    }),
  });

  // A rejected server key is our problem, not the student's: not a 401 for the game.
  if (upstream.status === 401) return json(502, { error: "server key rejected" });

  // Claude's answer (or error) goes straight back; the games read the API's shape.
  return new Response(await upstream.text(), {
    status: upstream.status,
    headers: { "content-type": "application/json" },
  });
};

export const config = { path: "/api/claude" };
