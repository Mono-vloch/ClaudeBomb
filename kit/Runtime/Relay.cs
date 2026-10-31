using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Maaloomatiia.Games.Claude
{
    public class RelayReply
    {
        public bool ok;
        public string text = "";
        public string error;       // the keeper's or Claude's error line, if any
        public long status;        // HTTP status, 0 when nothing answered
        public bool unauthorized;  // the keeper refused the session: Platform.SignInAgain()
        public bool busy;          // 429: too many calls at once
        public long inputTokens, outputTokens;
    }

    // Claude through the platform's key keeper (/api/claude on the game's own site, or
    // the local classroom server outside a browser). The keeper holds the key and checks
    // the session cookie, which the browser sends by itself. Up go the model, the effort,
    // the instructions and the conversation; back comes a reply in the Messages API's
    // shape, which this reads into a RelayReply.
    public static class Relay
    {
        public const string Route = "/api/claude";

        // messages: any list that serialises to [{ role, content }, ...], alternating
        // user and assistant and ending with the user. model and effort may be null for
        // the keeper's defaults.
        public static IEnumerator Send(string model, string effort, string system, object messages, Action<RelayReply> done, int timeoutSeconds = 150)
        {
            var payload = new JObject { ["system"] = system, ["messages"] = JToken.FromObject(messages) };
            if (!string.IsNullOrEmpty(model)) payload["model"] = model;
            if (!string.IsNullOrEmpty(effort)) payload["effort"] = effort;

            using (var req = Post(payload.ToString(Formatting.None), timeoutSeconds))
            {
                yield return req.SendWebRequest();
                done?.Invoke(Read(req));
            }
        }

        // Asks the keeper whether it is there and signed in, without calling Claude.
        public static IEnumerator Check(Action<RelayReply> done)
        {
            using (var req = Post("{\"check\":true}", 15))
            {
                yield return req.SendWebRequest();
                done?.Invoke(Read(req));
            }
        }

        static UnityWebRequest Post(string body, int timeout)
        {
            var req = new UnityWebRequest(Platform.ApiUrl(Route), "POST");
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("content-type", "application/json");
            req.timeout = timeout;
            return req;
        }

        static RelayReply Read(UnityWebRequest req)
        {
            var reply = new RelayReply { status = req.responseCode };
            string text = req.downloadHandler.text ?? "";
            if (req.result != UnityWebRequest.Result.Success)
            {
                reply.unauthorized = req.responseCode == 401;
                reply.busy = req.responseCode == 429;
                reply.error = ErrorLine(text) ?? (req.responseCode == 0 ? "No answer from the key keeper." : $"Key keeper error {req.responseCode}.");
                Debug.LogWarning($"[Relay] {req.responseCode}: {text}");
                return reply;
            }
            try
            {
                var root = JObject.Parse(text);
                if (root["check"] != null || root["ok"] != null && root["content"] == null) { reply.ok = true; return reply; }
                var sb = new StringBuilder();
                foreach (var block in root["content"] as JArray ?? new JArray())
                    if ((string)block["type"] == "text") sb.Append((string)block["text"]);
                reply.text = sb.ToString().Trim();
                var usage = root["usage"];
                reply.inputTokens = ((long?)usage?["input_tokens"] ?? 0) + ((long?)usage?["cache_read_input_tokens"] ?? 0)
                                  + ((long?)usage?["cache_creation_input_tokens"] ?? 0);
                reply.outputTokens = (long?)usage?["output_tokens"] ?? 0;
                if (reply.text == "") reply.error = "Empty reply."; else reply.ok = true;
            }
            catch (Exception ex) { reply.error = "Unreadable reply: " + ex.Message; }
            return reply;
        }

        static string ErrorLine(string json)
        {
            try
            {
                var root = JObject.Parse(json);
                return (string)root["error"] ?? (string)root["error"]?["message"];
            }
            catch (Exception) { return null; }
        }
    }
}
