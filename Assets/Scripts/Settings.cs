using UnityEngine;

// The player's choices from the game menu, kept between visits (PlayerPrefs, which in
// the browser is the site's own storage).
public static class Settings
{
    const string SoundKey = "sound_on", VoiceKey = "luma_voice_on", ClockKey = "clock_minutes";

    // Every sound in the game, LUMA's voice included.
    public static bool Sound
    {
        get => PlayerPrefs.GetInt(SoundKey, 1) == 1;
        set { PlayerPrefs.SetInt(SoundKey, value ? 1 : 0); PlayerPrefs.Save(); Apply(); }
    }

    // LUMA reads her lines aloud; off, they only appear in the chat.
    public static bool LumaVoice
    {
        get => PlayerPrefs.GetInt(VoiceKey, 1) == 1;
        set { PlayerPrefs.SetInt(VoiceKey, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    // The clock for a new device: 10 or 15 minutes, or 0 for practice with no clock.
    // Read when a device is made (GameManager.Awake), so a change applies on restart.
    public static readonly int[] ClockChoices = { 10, 15, 0 };
    public static int ClockMinutes
    {
        get => PlayerPrefs.GetInt(ClockKey, 10);
        set { PlayerPrefs.SetInt(ClockKey, value); PlayerPrefs.Save(); }
    }

    public static void Apply() => AudioListener.volume = Sound ? 1f : 0f;

    // "10 minutes", "15 minutes" or "no clock".
    public static string Describe(int minutes) => minutes > 0 ? $"{minutes} minutes" : "no clock";
}
