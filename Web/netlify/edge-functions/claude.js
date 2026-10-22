// The key keeper. The web game sends its conversation here; this function checks
// the access code, adds the Anthropic key (kept in Netlify's environment
// variables, never in the game) and forwards the request to Claude. The model and
// limits are fixed here, not by the browser, so the endpoint can't be used for
// anything bigger than the game.
//
// Runs as a Netlify Edge Function: those can wait up to 40 seconds for Claude's
// reply, where regular functions stop after 10. The game page lives on GitHub
// Pages and calls this function across sites, hence the CORS headers below.

const MODEL = "claude-sonnet-5";
const MAX_BODY_CHARS = 120000; // a whole game's conversation stays far below this

// The game itself is served from GitHub Pages (Netlify was too slow to deliver
// the large game files), so browsers call this function cross-origin.
const ALLOWED_ORIGINS = ["https://mono-vloch.github.io", "https://claude-bomb.netlify.app"];

function corsHeaders(request) {
  const origin = request.headers.get("origin");
  if (!origin || !ALLOWED_ORIGINS.includes(origin)) return {};
  return {
    "access-control-allow-origin": origin,
    "access-control-allow-methods": "POST, OPTIONS",
    "access-control-allow-headers": "content-type, x-access-code",
    "access-control-max-age": "86400",
    vary: "origin",
  };
}

export default async (request) => {
  const cors = corsHeaders(request);
  const json = (status, data) =>
    new Response(JSON.stringify(data), { status, headers: { "content-type": "application/json", ...cors } });

  // The browser's pre-flight check before the real request (custom header).
  if (request.method === "OPTIONS") return new Response(null, { status: 204, headers: cors });
  if (request.method !== "POST") return json(405, { error: "POST only" });

  // Case-insensitive, so players don't fail on capital letters.
  const expected = (Netlify.env.get("ACCESS_CODE") ?? "").trim().toLowerCase();
  const given = (request.headers.get("x-access-code") ?? "").trim().toLowerCase();
  if (!expected || given !== expected) return json(401, { error: "access code rejected" });

  const raw = await request.text();
  if (raw.length > MAX_BODY_CHARS) return json(413, { error: "request too large" });

  let body;
  try {
    body = JSON.parse(raw);
  } catch {
    return json(400, { error: "bad json" });
  }
  if (body.check) return json(200, { ok: true }); // the start panel validating a code

  const { system, messages } = body;
  if (typeof system !== "string" || !Array.isArray(messages) || messages.length === 0)
    return json(400, { error: "expected system and messages" });

  const upstream = await fetch("https://api.anthropic.com/v1/messages", {
    method: "POST",
    headers: {
      "content-type": "application/json",
      "x-api-key": Netlify.env.get("ANTHROPIC_API_KEY") ?? "",
      "anthropic-version": "2023-06-01",
    },
    body: JSON.stringify({
      model: MODEL,
      max_tokens: 2000,
      system,
      messages,
      output_config: { effort: "low" },
      // Caches the repeated prefix: cheaper, and cached input doesn't count
      // against the account's per-minute limit when many people play at once.
      cache_control: { type: "ephemeral" },
    }),
  });

  // A rejected server key is our problem, not the player's code: don't send the
  // game back to the access-code screen for it.
  if (upstream.status === 401) return json(502, { error: "server key rejected" });

  // Pass Claude's answer (or error) straight back; the game understands both.
  return new Response(await upstream.text(), {
    status: upstream.status,
    headers: { "content-type": "application/json", ...cors },
  });
};

export const config = { path: "/api/claude" };
