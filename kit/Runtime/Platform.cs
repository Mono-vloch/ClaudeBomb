using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Maaloomatiia.Games
{
    // Where a game stands in the Maaloomatiia Games platform.
    //
    // On the web, every game is served from a folder of the platform's site
    // (/games/<id>/), so the platform's menu is the site root and its key keeper is
    // /api/... on the same address. The browser carries the sign-in cookie by itself,
    // which is why a game never handles codes: if the keeper answers 401, the game
    // sends the player back to the menu to sign in again (SignInAgain).
    //
    // In the Editor and in desktop builds there is no platform: GoToMenu quits, and
    // ApiUrl points at the local classroom server (node server/local.mjs) in case a
    // game wants to reach Claude through it.
    public static class Platform
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        public static bool IsWeb => true;
        [DllImport("__Internal")] static extern void MaalPlatform_SetLocation(string url);
#else
        public static bool IsWeb => false;
        static void MaalPlatform_SetLocation(string url) => Debug.Log($"[Platform] would open {url}");
#endif

        // Set by the game at start, so progress reports and logs say which game.
        public static string GameId = "";

        public const string LocalServer = "http://localhost:8080";

        // The page the game runs on; empty outside a browser.
        public static string PageUrl => Application.absoluteURL ?? "";

        // A query value from the page's address (?level=3), or null.
        public static string Query(string name)
        {
            if (!Uri.TryCreate(PageUrl, UriKind.Absolute, out var page) || string.IsNullOrEmpty(page.Query)) return null;
            foreach (var pair in page.Query.TrimStart('?').Split('&'))
            {
                int eq = pair.IndexOf('=');
                string key = Uri.UnescapeDataString(eq < 0 ? pair : pair.Substring(0, eq)).Replace('+', ' ');
                if (key != name) continue;
                return eq < 0 ? "" : Uri.UnescapeDataString(pair.Substring(eq + 1).Replace('+', ' '));
            }
            return null;
        }

        // A platform route (/api/claude, /api/progress) as a full address: on the game's
        // own site in a browser, on the local classroom server otherwise.
        public static string ApiUrl(string route)
        {
            if (Uri.TryCreate(PageUrl, UriKind.Absolute, out var page) && page.Scheme.StartsWith("http"))
                return new Uri(page, route).ToString();
            return LocalServer + route;
        }

        // The path of this game's page on the site (/games/claude-bomb/), for the
        // return trip after signing in again.
        static string PagePath =>
            Uri.TryCreate(PageUrl, UriKind.Absolute, out var page) ? page.AbsolutePath : "/";

        // Back to the platform's menu, in the same tab. Outside a browser: quit.
        public static void GoToMenu()
        {
            if (IsWeb) MaalPlatform_SetLocation("/");
            else Application.Quit();
        }

        // The keeper refused the session: back to the menu, which asks for the code and
        // then returns the player to this game.
        public static void SignInAgain()
        {
            if (IsWeb) MaalPlatform_SetLocation("/?signin=1&return=" + Uri.EscapeDataString(PagePath));
            else Debug.LogWarning("[Platform] the platform asked for a sign-in; outside a browser there is none.");
        }

        // Tells the platform how a run ended. Fire and forget: a failure changes nothing
        // for the player. Does nothing outside a browser.
        public static IEnumerator ReportProgress(string result, int stars = 0, string detail = null)
        {
            if (!IsWeb) yield break;
            var body = new Dictionary<string, object> { ["game"] = GameId, ["result"] = result, ["stars"] = stars };
            if (detail != null) body["detail"] = detail;
            using (var req = new UnityWebRequest(ApiUrl("/api/progress"), "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(body)));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("content-type", "application/json");
                req.timeout = 10;
                yield return req.SendWebRequest();
            }
        }
    }
}
