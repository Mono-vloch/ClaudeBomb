using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

// How the game reaches Claude: always through Claude Code, signed in with a Claude
// subscription. No API key anywhere.
// - Editor and desktop app: the game runs `claude -p` itself (ClaudeCode.cs).
// - Browser: the page asks the Maaloomatiia Games platform it runs on, at /api/claude
//   on its own site (ClaudeProxy.cs, through the kit's Relay). Published, the platform's
//   key keeper holds the key; in a classroom, `node server/local.mjs` at the repository
//   root answers with the Claude Code sign-in of the computer running it.
// With neither, the agents run on the built-in stand-in brain.
public enum ClaudeMode { ClaudeCode, Proxy, Offline }

public static class ClaudeAccess
{
    // Set by the screenshot run so no real call is made.
    public static bool ForceOffline;

    public static string Describe() => Mode switch
    {
        ClaudeMode.ClaudeCode => "Claude connected through Claude Code (your subscription)",
        ClaudeMode.Proxy => "Claude connected through the Maaloomatiia Games platform",
        _ => "Claude not connected: agents will use the built-in stand-in brain",
    };

#if UNITY_WEBGL && !UNITY_EDITOR
    static bool proxyReady; // what the server said the last time Probe ran

    public static ClaudeMode Mode =>
        ForceOffline ? ClaudeMode.Offline : proxyReady ? ClaudeMode.Proxy : ClaudeMode.Offline;

    public const string Hint = "The platform could not reach Claude. In a classroom, the computer running the platform's server " +
                               "(node server/local.mjs) needs Claude Code installed and signed in (claude, then /login). " +
                               "Without Claude, a simple stand-in brain moves the agents.";

    // Asks the platform's keeper whether it can reach Claude, then calls done.
    public static IEnumerator Probe(Action done)
    {
        yield return ClaudeProxy.Check(ok => proxyReady = ok);
        done?.Invoke();
    }

    public static string ClaudeCodePath => null;
#else
    public static ClaudeMode Mode =>
        ForceOffline ? ClaudeMode.Offline : ClaudeCodePath != null ? ClaudeMode.ClaudeCode : ClaudeMode.Offline;

    public const string Hint = "Install Claude Code and sign in (claude, then /login); the game finds it on its own. " +
                               "Without Claude, a simple stand-in brain moves the agents.";

    // Looks for Claude Code again, then calls done.
    public static IEnumerator Probe(Action done)
    {
        searched = false;
        yield return null;
        done?.Invoke();
    }

    static string claudeCodePath;
    static bool searched;

    // Claude Code's program: on the PATH, in the usual install folders, or inside
    // the VS Code extension. An app started from the Dock does not see the shell's PATH.
    public static string ClaudeCodePath
    {
        get
        {
            if (searched) return claudeCodePath;
            searched = true;
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var dirs = (Environment.GetEnvironmentVariable("PATH") ?? "")
                .Split(Path.PathSeparator).Where(d => d != "").ToList();
            dirs.AddRange(new[] { Path.Combine(home, ".local/bin"), Path.Combine(home, ".claude/local"), "/opt/homebrew/bin", "/usr/local/bin" });
            string ext = Path.Combine(home, ".vscode/extensions");
            if (Directory.Exists(ext))
                dirs.AddRange(Directory.GetDirectories(ext, "anthropic.claude-code-*")
                    .OrderByDescending(Directory.GetLastWriteTimeUtc)
                    .Select(d => Path.Combine(d, "resources/native-binary")));
            claudeCodePath = dirs.SelectMany(d => new[] { "claude", "claude.exe" }, Path.Combine).FirstOrDefault(File.Exists);
            return claudeCodePath;
        }
    }
#endif
}
