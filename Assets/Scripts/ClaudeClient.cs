using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

// The real advisor. Claude is stateless - every call resends the full trimmed
// history plus the new turn. systemPrompt (how to work, no module rules) lives in
// the Inspector; the module rules only arrive as manual pages the player shares
// from the PaperDesk, which ride along in the system prompt of every call.
//
// Ways to reach Claude:
// - Editor / desktop with a key file (UserSettings/claude_key.txt, gitignored, never
//   shipped): straight to the API.
// - Editor / desktop without one, when Claude Code is installed: through `claude -p`,
//   which uses the Claude subscription it is signed in with. No key needed.
// - Otherwise straight to the API with a key typed into the panel.
// - Web build: through the key keeper on Netlify (PROXY_URL, see Web/netlify), which
//   holds the key on the server. The browser only ever sends an access code.
public class ClaudeClient : MonoBehaviour, IAdvisor
{
    const string API_URL = "https://api.anthropic.com/v1/messages";
    // The key keeper lives on Netlify; the game page itself is served from GitHub
    // Pages, so this must be a full address, not a path on the page's own site.
    const string PROXY_URL = "https://claude-bomb.netlify.app/api/claude";
    // A build played on this computer (Web/serve-local.mjs) has its own key keeper on
    // the page's address; the Netlify one only accepts the published sites.
    static string ProxyUrl =>
        Uri.TryCreate(Application.absoluteURL, UriKind.Absolute, out var page) && page.IsLoopback
            ? new Uri(page, "/api/claude").ToString()
            : PROXY_URL;
    const string MODEL = "claude-sonnet-5";
    const string PREF_KEY = "anthropic_key";
    const string PREF_CODE = "access_code";
    const string LOCAL_KEY_FILE = "../UserSettings/claude_key.txt"; // relative to Assets, never in a build
    const int MaxHistory = 24;

#if UNITY_WEBGL && !UNITY_EDITOR
    public static bool UsesProxy => true;
#else
    public static bool UsesProxy => false;
#endif

    // Raised when the server refuses the key or access code, so the panel can ask again.
    public static event Action OnCredentialsRejected;

    // A key typed into the panel wins; otherwise the local key file, so the Editor
    // just works for whoever owns the project.
    public static string ApiKey
    {
        get
        {
            string typed = PlayerPrefs.GetString(PREF_KEY, "");
            return !string.IsNullOrEmpty(typed) ? typed : LocalKey;
        }
        set { PlayerPrefs.SetString(PREF_KEY, value.Trim()); PlayerPrefs.Save(); }
    }

    public static string AccessCode
    {
        get => PlayerPrefs.GetString(PREF_CODE, "");
        set { PlayerPrefs.SetString(PREF_CODE, value.Trim()); PlayerPrefs.Save(); }
    }

    // A key file wins, so the setup in the README keeps working; a typed key is the
    // last resort.
    public static bool UsesClaudeCode => !UsesProxy && string.IsNullOrEmpty(LocalKey) && ClaudeCodePath != null;

    public static bool Ready => UsesProxy ? !string.IsNullOrEmpty(AccessCode)
                              : UsesClaudeCode || !string.IsNullOrEmpty(ApiKey);

    // Claude Code's program: on the PATH, in a usual install folder, or the copy inside
    // the VS Code extension (newest first). An Editor started from the Dock or the Hub
    // doesn't get the shell's PATH, hence the fixed folders.
    static string claudeCodePath;
    static bool claudeCodeSearched;
    static string ClaudeCodePath
    {
        get
        {
            if (claudeCodeSearched) return claudeCodePath;
            claudeCodeSearched = true;

            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var dirs = (Environment.GetEnvironmentVariable("PATH") ?? "")
                .Split(System.IO.Path.PathSeparator).Where(d => d != "").ToList();
            dirs.AddRange(new[] { System.IO.Path.Combine(home, ".local/bin"), System.IO.Path.Combine(home, ".claude/local"),
                                  "/opt/homebrew/bin", "/usr/local/bin" });
            string extensions = System.IO.Path.Combine(home, ".vscode/extensions");
            if (System.IO.Directory.Exists(extensions))
                dirs.AddRange(System.IO.Directory.GetDirectories(extensions, "anthropic.claude-code-*")
                    .OrderByDescending(System.IO.Directory.GetLastWriteTimeUtc)
                    .Select(d => System.IO.Path.Combine(d, "resources/native-binary")));

            claudeCodePath = dirs.SelectMany(d => new[] { "claude", "claude.exe" }, System.IO.Path.Combine)
                                 .FirstOrDefault(System.IO.File.Exists);
            return claudeCodePath;
        }
    }

    // Re-read on every access (it's tiny) so a key pasted into the file while the
    // Editor is open is picked up on the next message.
    static string LocalKey
    {
        get
        {
            string path = System.IO.Path.Combine(Application.dataPath, LOCAL_KEY_FILE);
            return System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path).Trim() : "";
        }
    }

    [TextArea(10, 40)] public string systemPrompt = "";

    class Msg { public string role; public string content; }
    readonly List<Msg> history = new List<Msg>();

    // Pages shared so far, in share order. Kept out of the trimmed history so a
    // page shared early is never forgotten later in the game.
    readonly List<(string id, string text)> sharedPages = new List<(string id, string text)>();

    // Sharing a page again (e.g. after blacking out more of it) replaces the old copy.
    public void SharePage(string id, string englishText)
    {
        sharedPages.RemoveAll(p => p.id == id);
        sharedPages.Add((id, englishText));
    }

    string SystemWithPages() => sharedPages.Count == 0
        ? systemPrompt + "\n\nSHARED PAGES: none yet."
        : systemPrompt + "\n\nSHARED PAGES:\n\n" + string.Join("\n\n", sharedPages.ConvertAll(p => p.text));

    public IEnumerator Send(string playerText, Action<string> onReply)
    {
        if (!Ready) { onReply?.Invoke(UsesProxy ? "No access code yet." : "No API key set."); yield break; }

        history.Add(new Msg { role = "user", content = playerText });
        // The API rejects a conversation whose first message is Claude's, so trim
        // from the front until it fits AND starts with the player's turn. Trimming
        // a fixed count could leave an assistant turn first, and every later call
        // would then fail too.
        while (history.Count > MaxHistory || history[0].role != "user")
            history.RemoveAt(0);

        string system = SystemWithPages();

        if (UsesClaudeCode)
        {
            string prompt = Transcript(history); // built here: history is only touched on this thread
            var run = System.Threading.Tasks.Task.Run(() => AskClaudeCode(system, prompt));
            yield return new WaitUntil(() => run.IsCompleted);
            var (ok, text) = run.Result;
            if (ok) history.Add(new Msg { role = "assistant", content = text });
            else history.RemoveAt(history.Count - 1); // never keep a turn that did not land
            onReply?.Invoke(text);
            yield break;
        }

        // The key keeper sets the model and limits itself; only the conversation goes up.
        string body = UsesProxy
            ? JsonConvert.SerializeObject(new { system, messages = history })
            : JsonConvert.SerializeObject(new
            {
                model = MODEL,
                // Thinking tokens count toward this cap too; too low a cap cuts replies
                // off mid-sentence. Only tokens actually generated are billed.
                max_tokens = 2000,
                system,
                messages = history,
                // Low effort keeps this fast and terse - this is a live, timed game and
                // the task (matching manual rules to what the player describes) doesn't
                // need deep reasoning.
                output_config = new { effort = "low" },
                // Caches the repeated prefix (instructions, pages, history so far):
                // cheaper, and cached input doesn't count against the per-minute limit.
                cache_control = new { type = "ephemeral" }
            });

        using (var req = new UnityWebRequest(UsesProxy ? ProxyUrl : API_URL, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("content-type", "application/json");
            if (UsesProxy)
            {
                req.SetRequestHeader("x-access-code", AccessCode);
            }
            else
            {
                req.SetRequestHeader("x-api-key", ApiKey);
                req.SetRequestHeader("anthropic-version", "2023-06-01");
            }
            req.timeout = 60;

            yield return req.SendWebRequest();

            string reply;
            if (req.result != UnityWebRequest.Result.Success)
            {
                history.RemoveAt(history.Count - 1); // never keep a turn that did not land
                string error = req.downloadHandler.text ?? "";
                reply = req.responseCode == 401 ? (UsesProxy ? "Access code rejected." : "Key rejected.")
                      : req.responseCode == 429 ? "Radio's busy - too many calls at once. Wait a few seconds and say again."
                      : error.Contains("credit balance") ? "Radio's out of power - the API account has run out of credit."
                      : "Radio's cutting out. Say again.";
                Debug.LogWarning($"{req.responseCode}: {req.downloadHandler.text}");
                if (req.responseCode == 401) OnCredentialsRejected?.Invoke();
            }
            else
            {
                reply = Extract(req.downloadHandler.text);
                history.Add(new Msg { role = "assistant", content = reply });
            }
            onReply?.Invoke(reply);
        }
    }

    // Asks the key keeper whether an access code is valid, without calling Claude.
    public static IEnumerator CheckAccessCode(string code, Action<bool> done)
    {
        using (var req = new UnityWebRequest(ProxyUrl, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("{\"check\":true}"));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("content-type", "application/json");
            req.SetRequestHeader("x-access-code", code);
            req.timeout = 20;
            yield return req.SendWebRequest();
            done?.Invoke(req.result == UnityWebRequest.Result.Success);
        }
    }

    // `claude -p` takes one prompt, so the earlier turns go in as a transcript before
    // the new one. Web/serve-local.mjs builds the same text for the browser version.
    static string Transcript(List<Msg> turns)
    {
        string latest = turns[turns.Count - 1].content;
        if (turns.Count == 1) return latest;
        var sb = new StringBuilder("Earlier in this conversation:\n\n");
        for (int i = 0; i < turns.Count - 1; i++)
            sb.Append(turns[i].role == "user" ? "Player: " : "You: ").Append(turns[i].content).Append("\n\n");
        return sb.Append("---\n\nThe player now says:\n\n").Append(latest).ToString();
    }

    // Runs on a worker thread. Same model and effort as the API call; no tools, no saved
    // session, none of the user's Claude Code settings, and the system prompt from a file
    // (the conversation goes in on stdin) so nothing needs quoting.
    static (bool ok, string reply) AskClaudeCode(string system, string prompt)
    {
        string systemFile = System.IO.Path.GetTempFileName();
        try
        {
            System.IO.File.WriteAllText(systemFile, system);
            var psi = new System.Diagnostics.ProcessStartInfo(ClaudeCodePath,
                $"-p --system-prompt-file \"{systemFile}\" --tools \"\" --output-format json " +
                $"--model {MODEL} --effort low --no-session-persistence --setting-sources \"\"")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                WorkingDirectory = System.IO.Path.GetTempPath(), // away from any project CLAUDE.md
            };
            psi.EnvironmentVariables.Remove("ANTHROPIC_API_KEY"); // the subscription, never a key

            using (var p = System.Diagnostics.Process.Start(psi))
            {
                byte[] input = Encoding.UTF8.GetBytes(prompt);
                p.StandardInput.BaseStream.Write(input, 0, input.Length);
                p.StandardInput.Close();
                var output = p.StandardOutput.ReadToEndAsync();
                var errors = p.StandardError.ReadToEndAsync();
                if (!p.WaitForExit(90_000))
                {
                    p.Kill();
                    Debug.LogWarning("Claude Code took longer than 90 seconds.");
                    return (false, "Radio's cutting out. Say again.");
                }

                var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(output.Result);
                string text = result != null && result.TryGetValue("result", out var r) ? (r as string ?? "").Trim() : "";
                bool failed = result == null || (result.TryGetValue("is_error", out var e) && e is bool b && b);
                if (!failed && text != "") return (true, text);

                Debug.LogWarning($"Claude Code: {output.Result} {errors.Result}");
                return (false, text.Contains("limit") ? "Radio's out of power - the Claude subscription has hit its usage limit."
                             : text.Contains("login") ? "Radio's off - Claude Code isn't signed in. Run claude, then /login."
                             : "Radio's cutting out. Say again.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Claude Code: {ex.Message}");
            return (false, "Radio's cutting out. Say again.");
        }
        finally
        {
            System.IO.File.Delete(systemFile);
        }
    }

    string Extract(string json)
    {
        try
        {
            var root = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            if (!root.ContainsKey("content")) return "Radio's cutting out. Say again.";

            var blocks = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(
                             root["content"].ToString());
            var sb = new StringBuilder();
            foreach (var b in blocks)
                if (b.TryGetValue("type", out var t) && (string)t == "text") sb.Append(b["text"]);

            string s = sb.ToString().Trim();
            return string.IsNullOrEmpty(s) ? "Radio's cutting out. Say again." : s;
        }
        catch { return "Radio's cutting out. Say again."; }
    }
}
