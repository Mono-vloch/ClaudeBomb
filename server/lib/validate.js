// What a game may send to /api/claude. Shared by the key keeper on Netlify and the
// local classroom server, so both accept exactly the same requests.
//
// Claude Bomb sends { system, messages } and gets the default model and effort.
// AI Arena sends { model, effort, system, messages }, one call per agent per turn.

// Model ids a game may ask for. The default is what Claude Bomb has always used.
export const MODELS = ["claude-sonnet-5", "claude-haiku-5-5", "claude-sonnet-5-5", "claude-opus-5-5"];
export const EFFORTS = ["low", "medium", "high"];
export const DEFAULT_MODEL = "claude-sonnet-5";
export const DEFAULT_EFFORT = "low";
export const MAX_TOKENS = 2000;
export const MAX_BODY_CHARS = 200000; // a whole game's conversation stays far below this
export const MAX_MESSAGES = 60;

// Returns { error } or the checked { model, effort, system, messages }.
export function checkClaudeBody(body) {
  if (!body || typeof body !== "object") return { error: "expected a JSON object" };
  const model = body.model ?? DEFAULT_MODEL;
  const effort = body.effort ?? DEFAULT_EFFORT;
  const { system, messages } = body;
  if (!MODELS.includes(model)) return { error: "unknown model" };
  if (!EFFORTS.includes(effort)) return { error: "unknown effort" };
  if (typeof system !== "string") return { error: "expected system" };
  if (!Array.isArray(messages) || messages.length === 0 || messages.length > MAX_MESSAGES)
    return { error: "expected 1 to " + MAX_MESSAGES + " messages" };
  for (let i = 0; i < messages.length; i++) {
    const m = messages[i];
    const role = i % 2 === 0 ? "user" : "assistant";
    if (!m || m.role !== role || typeof m.content !== "string")
      return { error: "messages must alternate user and assistant, starting with user" };
  }
  if (messages.length % 2 === 0) return { error: "the last message must be the user's" };
  return { model, effort, system, messages: messages.map((m) => ({ role: m.role, content: m.content })) };
}
