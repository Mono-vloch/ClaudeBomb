using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

// The real advisor. Claude is stateless - every call resends the full trimmed
// history plus the new turn. systemPrompt (the manual) lives ONLY here, in the
// Inspector - never anywhere the player can read it.
public class ClaudeClient : MonoBehaviour, IAdvisor
{
    const string URL = "https://api.anthropic.com/v1/messages";
    const string MODEL = "claude-sonnet-5";
    const string PREF_KEY = "anthropic_key";

    public static string ApiKey
    {
        get => PlayerPrefs.GetString(PREF_KEY, "");
        set { PlayerPrefs.SetString(PREF_KEY, value.Trim()); PlayerPrefs.Save(); }
    }
    public static bool HasKey => !string.IsNullOrEmpty(ApiKey);

    [TextArea(10, 40)] public string systemPrompt = "";

    class Msg { public string role; public string content; }
    readonly List<Msg> history = new List<Msg>();

    public IEnumerator Send(string playerText, Action<string> onReply)
    {
        if (!HasKey) { onReply?.Invoke("No API key set. Open Settings."); yield break; }

        history.Add(new Msg { role = "user", content = playerText });
        if (history.Count > 24) history.RemoveRange(0, history.Count - 24);

        string body = JsonConvert.SerializeObject(new
        {
            model = MODEL,
            max_tokens = 500,
            system = systemPrompt,
            messages = history,
            // Low effort keeps this fast and terse - this is a live, timed game and
            // the task (matching manual rules to what the player describes) doesn't
            // need deep reasoning.
            output_config = new { effort = "low" }
        });

        using (var req = new UnityWebRequest(URL, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("content-type", "application/json");
            req.SetRequestHeader("x-api-key", ApiKey);
            req.SetRequestHeader("anthropic-version", "2023-06-01");
            req.timeout = 30;

            yield return req.SendWebRequest();

            string reply;
            if (req.result != UnityWebRequest.Result.Success)
            {
                history.RemoveAt(history.Count - 1); // never keep a turn that did not land
                reply = req.responseCode == 401
                    ? "Key rejected. Check it in Settings."
                    : "Radio's cutting out. Say again.";
                Debug.LogWarning($"{req.responseCode}: {req.downloadHandler.text}");
            }
            else
            {
                reply = Extract(req.downloadHandler.text);
                history.Add(new Msg { role = "assistant", content = reply });
            }
            onReply?.Invoke(reply);
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
