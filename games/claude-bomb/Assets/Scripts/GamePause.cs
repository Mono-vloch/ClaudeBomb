using System.Collections.Generic;
using UnityEngine;

// Freezes the game clock while any overlay (Looma, How to play, the access-code
// panel) is on screen. Each overlay holds the pause under its own name, so one
// closing never unfreezes the game while another is still open.
public static class GamePause
{
    static readonly HashSet<object> holders = new HashSet<object>();

    public static bool IsPaused => holders.Count > 0;

    public static void Hold(object owner)
    {
        holders.Add(owner);
        Time.timeScale = 0f; // GameManager counts down with Time.deltaTime
    }

    public static void Release(object owner)
    {
        holders.Remove(owner);
        if (holders.Count == 0) Time.timeScale = 1f;
    }

    // A new run (scene load or Restart) starts unpaused: Time.timeScale and this
    // static set both survive scene loads.
    public static void ResetAll()
    {
        holders.Clear();
        Time.timeScale = 1f;
    }
}
