using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json.Linq;

// Reaches Claude through the Claude Code command line (`claude -p`), which uses
// the subscription it is signed in with. Takes one prompt per call, so the
// remembered turns go in as a transcript. Runs on a worker thread.
public static class ClaudeCode
{
    public static Task<ClaudeReply> Ask(string model, string effort, string system, List<(string user, string assistant)> history, string userTurn)
        => Task.Run(() => AskBlocking(model, effort, system, Transcript(history, userTurn)));

    static string Transcript(List<(string user, string assistant)> history, string latest)
    {
        if (history.Count == 0) return latest;
        var sb = new StringBuilder("Earlier turns:\n\n");
        foreach (var (u, a) in history) sb.Append("Observation: ").Append(u).Append("\nYou answered: ").Append(a).Append("\n\n");
        return sb.Append("---\n\nNow:\n\n").Append(latest).ToString();
    }

    static ClaudeReply AskBlocking(string model, string effort, string system, string prompt)
    {
        var reply = new ClaudeReply();
#if UNITY_EDITOR || UNITY_STANDALONE
        string systemFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(systemFile, system);
            var psi = new System.Diagnostics.ProcessStartInfo(ClaudeAccess.ClaudeCodePath,
                $"-p --system-prompt-file \"{systemFile}\" --tools \"\" --output-format json " +
                $"--model {model} --effort {effort} --no-session-persistence --setting-sources \"\"")
            {
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                WorkingDirectory = Path.GetTempPath(),
            };
            psi.EnvironmentVariables.Remove("ANTHROPIC_API_KEY");
            using (var p = System.Diagnostics.Process.Start(psi))
            {
                byte[] input = Encoding.UTF8.GetBytes(prompt);
                p.StandardInput.BaseStream.Write(input, 0, input.Length);
                p.StandardInput.Close();
                var output = p.StandardOutput.ReadToEndAsync();
                var errors = p.StandardError.ReadToEndAsync();
                if (!p.WaitForExit(120_000)) { p.Kill(); reply.error = "Claude Code took too long"; return reply; }

                var root = JObject.Parse(output.Result);
                string text = ((string)root["result"] ?? "").Trim();
                bool failed = (bool?)root["is_error"] ?? false;
                reply.inputTokens = (long?)root["usage"]?["input_tokens"] ?? 0;
                reply.inputTokens += (long?)root["usage"]?["cache_read_input_tokens"] ?? 0;
                reply.inputTokens += (long?)root["usage"]?["cache_creation_input_tokens"] ?? 0;
                reply.outputTokens = (long?)root["usage"]?["output_tokens"] ?? 0;
                if (reply.inputTokens == 0) reply.inputTokens = Observation.EstimateTokens(system + prompt);
                if (reply.outputTokens == 0) reply.outputTokens = Observation.EstimateTokens(text);
                if (!failed && text != "") { reply.ok = true; reply.text = text; return reply; }
                Debug.LogWarning($"Claude Code: {output.Result} {errors.Result}");
                reply.error = text.Contains("limit") ? "The Claude subscription has hit its usage limit"
                            : text.Contains("login") ? "Claude Code is not signed in (run claude, then /login)"
                            : "Claude Code returned an error";
            }
        }
        catch (Exception ex) { reply.error = "Claude Code: " + ex.Message; }
        finally { File.Delete(systemFile); }
#else
        reply.error = "Claude Code is not available in this build";
#endif
        return reply;
    }
}
