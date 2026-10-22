using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

// The real advisor. Claude is stateless - every call resends the full trimmed
// history plus the new turn. systemPrompt (how to work, no module rules) lives in
// the Inspector; the module rules only arrive as manual pages the player shares
// from the PaperDesk, which ride along in the system prompt of every call.
//
// Two ways to reach Claude:
// - Editor / desktop: straight to the API, with a key from a local file that is
//   never shipped (UserSettings/claude_key.txt, gitignored) or typed into the panel.
// - Web build: through the key keeper on Netlify (PROXY_URL, see Web/netlify), which
//   holds the key on the server. The browser only ever sends an access code.
public class ClaudeClient : MonoBehaviour, IAdvisor
{
    const string API_URL = "https://api.anthropic.com/v1/messages";
    // The key keeper lives on Netlify; the game page itself is served from GitHub
    // Pages, so this must be a full address, not a path on the page's own site.
    const string PROXY_URL = "https://claude-bomb.netlify.app/api/claude";
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

    public static bool Ready => UsesProxy ? !string.IsNullOrEmpty(AccessCode) : !string.IsNullOrEmpty(ApiKey);

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

        using (var req = new UnityWebRequest(UsesProxy ? PROXY_URL : API_URL, "POST"))
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
        using (var req = new UnityWebRequest(PROXY_URL, "POST"))
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
