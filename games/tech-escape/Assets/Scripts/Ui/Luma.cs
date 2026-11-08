using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// LUMA, the coach: the same character as in Claude Bomb and the other games. She says one short
// thing when something happens (the room as handed over, the attack's first door, the debrief)
// and nothing to explain a screen; her full lines and the curriculum are in the file. Her fixed
// lines are recorded, one file per line id in Resources/LumaVoice (Tech Escape > Generate LUMA
// Voice); lines built on the fly (the attack's summary) are text only.
public class Luma : MonoBehaviour
{
    public const int CueWords = 15; // a cue on screen is never longer than this

    public static Dictionary<string, string> Lines
    {
        get
        {
            var lines = new Dictionary<string, string>
            {
                ["title"] = "Press Start: the first door is the Risk Room.",
                ["back"] = "Welcome back.",
            };
            foreach (var l in Rooms.All)
            {
                // Entering the room: the planned mistake's name, or her first sentence on a level without one.
                string enter = !string.IsNullOrEmpty(l.mistake) ? l.mistake.TrimEnd('.') + "." : ShortestSentence(l.luma);
                if (enter != null && Words(enter) <= CueWords) lines["enter-" + l.number] = enter;
                var live = FirstSentence(l.live);
                if (live != null) lines["live-" + l.number] = live;
                var watch = LastSentence(l.live);
                if (watch != null && watch != live) lines["watch-" + l.number] = watch;
                var take = FirstSentence(l.takeaway);
                if (take != null) lines["take-" + l.number] = take;
            }
            return lines;
        }
    }

    public static int Words(string text) => string.IsNullOrWhiteSpace(text) ? 0 : text.Split(' ').Count(w => w.Any(char.IsLetterOrDigit));

    // The first sentence of a line that is short enough to say as a cue, or the next when the
    // first is not; null when none is.
    public static string FirstSentence(string text)
    {
        if (string.IsNullOrEmpty(text)) return null;
        var parts = Regex.Split(text.Trim(), @"(?<=[.!?])\s+");
        foreach (var p in parts) if (Words(p) <= CueWords && Words(p) > 0) return p.Trim();
        return null;
    }

    // The shortest whole sentence of a line: the room's cue on a level with no planned mistake.
    public static string ShortestSentence(string text)
    {
        if (string.IsNullOrEmpty(text)) return null;
        return Regex.Split(text.Trim(), @"(?<=[.!?])\s+").Where(p => Words(p) > 0).OrderBy(Words).FirstOrDefault()?.Trim();
    }

    public static string LastSentence(string text)
    {
        if (string.IsNullOrEmpty(text)) return null;
        var parts = Regex.Split(text.Trim(), @"(?<=[.!?])\s+");
        for (int i = parts.Length - 1; i >= 0; i--) if (Words(parts[i]) <= CueWords && Words(parts[i]) > 0) return parts[i].Trim();
        return null;
    }

    public static string Spoken(string id) => Regex.Replace(Lines[id], "<[^>]+>", "");

    public static bool Silent; // the screenshot walks only: never saved, unlike Voice
    const string PrefVoice = "luma_voice";
    public static bool Voice
    {
        get => PlayerPrefs.GetInt(PrefVoice, 1) == 1;
        set { PlayerPrefs.SetInt(PrefVoice, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static Sprite Portrait => Resources.Load<Sprite>("Luma/luma");

    AudioSource voice;
    public bool Speaking => speaking != null || (voice != null && voice.isPlaying);
    // True while a line is still being shown or spoken: a moment that must not cover her
    // mid-sentence (the attack's first banner) waits on it.
    public bool Busy => typing || Speaking;
    bool typing;
    Coroutine speaking;
    RectTransform current; // the cue on screen, so the next replaces it

    void Awake()
    {
        voice = gameObject.AddComponent<AudioSource>();
        voice.playOnAwake = false;
        voice.spatialBlend = 0f;
    }

    public string Say(string id)
    {
        Hush();
        if (Voice && !Silent)
        {
            var clip = Resources.Load<AudioClip>("LumaVoice/" + id);
            if (clip != null) speaking = StartCoroutine(PlayWhenLoaded(clip));
        }
        return Lines[id];
    }

    System.Collections.IEnumerator PlayWhenLoaded(AudioClip clip)
    {
        if (clip.loadState != AudioDataLoadState.Loaded)
        {
            if (clip.loadState == AudioDataLoadState.Unloaded) clip.LoadAudioData();
            float waited = 0f;
            while (clip.loadState == AudioDataLoadState.Loading && waited < 5f) { waited += Time.unscaledDeltaTime; yield return null; }
            if (clip.loadState != AudioDataLoadState.Loaded) yield break;
        }
        voice.clip = clip;
        voice.Play();
        Debug.Log($"[Luma] speaking {clip.name} ({clip.length:0.0} s)");
        speaking = null;
    }

    // A cue: one of her short lines, said and shown whole (it fades in, never typed out
    // mid-sentence), gone once she is done unless it is to stay. The id must be in Lines; a
    // missing id says nothing and returns null.
    public RectTransform Cue(Transform parent, string id, float x = 40, float y = 944, float w = 820, bool stay = false)
    {
        if (!Lines.ContainsKey(id)) return null;
        return Line(parent, x, y, w, Say(id), true, stay, 18, type: false);
    }

    // A line made on the fly (the attack's summary), shown whole, text only.
    public RectTransform Text(Transform parent, string text, float x = 40, float y = 944, float w = 820, bool stay = false)
        => Line(parent, x, y, w, text, true, stay, 18, type: false);

    // One line of coaching over a screen: her small portrait and the line, typed out as she
    // says it or shown whole. The strip fades once she is done unless it is to stay. Returns
    // the strip, so a screen can replace the line.
    public RectTransform Line(Transform parent, float x, float y, float w, string text, bool dark, bool stay = false, float size = 17, bool type = true)
    {
        if (current != null) { Destroy(current.gameObject); current = null; }
        var card = Ui.Box(parent, "Luma line", dark ? new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.84f) : Theme.Panel);
        card.raycastTarget = false;
        var t = card.transform;
        var body = Ui.Text(t, "", size, dark ? Color.white : Theme.Ink);
        float textH = body.GetPreferredValues(text ?? "", w - 110, 0).y;
        float h = Mathf.Max(64f, textH + 38f);
        Ui.Place(card.rectTransform, x, y, w, h);
        var frame = Ui.Box(t, "Portrait", dark ? Theme.Ink : Theme.Surface2);
        Ui.Place(frame.rectTransform, 10, 10, h - 20, h - 20);
        var sprite = Portrait;
        if (sprite != null)
        {
            var img = Ui.Node("Picture", frame.transform).gameObject.AddComponent<Image>();
            img.sprite = sprite; img.preserveAspect = true; img.raycastTarget = false;
            Ui.Stretch(img.rectTransform, 4, 4, 4, 4);
        }
        var name = Ui.Eyebrow(t, "LUMA", dark ? Theme.Teal : Theme.TealDeep);
        name.font = Theme.Bold;
        Ui.Place(name.rectTransform, h + 2, 9, 100, 18);
        Ui.Place(body.rectTransform, h + 2, 28, w - h - 16, h - 32);
        var group = Ui.Group(card);
        current = card.rectTransform;
        StartCoroutine(RunLine(card, group, body, text ?? "", stay, type));
        return card.rectTransform;
    }

    System.Collections.IEnumerator RunLine(Image card, CanvasGroup group, TMP_Text body, string text, bool stay, bool type)
    {
        typing = true;
        if (type) yield return Ui.Type(body, text, 45f);
        else { body.text = text; group.alpha = 0f; yield return Ui.Fade(group, 1f, 0.25f); }
        typing = false;
        float waited = 0f;
        while (card != null && Speaking && waited < 30f) { waited += Time.unscaledDeltaTime; yield return null; }
        if (stay || card == null) yield break;
        yield return new WaitForSecondsRealtime(type ? 2.5f : 1.5f + 0.3f * Words(text));
        if (card == null) yield break;
        yield return Ui.Fade(group, 0f, 0.6f);
        if (card != null) card.gameObject.SetActive(false);
    }

    public void Hush()
    {
        if (speaking != null) { StopCoroutine(speaking); speaking = null; }
        if (voice != null) voice.Stop();
        typing = false;
    }
}
