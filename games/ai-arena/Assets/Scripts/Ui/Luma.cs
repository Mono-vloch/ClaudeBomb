using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// LUMA, the coach: the same character as in Claude Bomb and Robot Racing. She says one short
// thing only when something happens in the world (an agent sent home or out of tokens) and the
// takeaway on the debrief; her other lines live in the file (OnTheWay). Her fixed lines are recorded, one
// file per line id in Resources/LumaVoice (AI Arena > Generate LUMA Voice); lines built on the
// fly, with an agent's name in them, are text only.
public class Luma : MonoBehaviour
{
    public const int CueWords = 15; // a cue on screen is never longer than this

    // Every recorded line, by id. The level lines come from the ladder, so the lesson text
    // and the voice can never drift apart. The cues of the game-feel pass are sentences of her
    // own longer lines, said at the moment they fit.
    public static Dictionary<string, string> Lines
    {
        get
        {
            var lines = new Dictionary<string, string>
            {
                ["recap"] = "The agents are home. Here is what each one practiced, and the one idea to take from this errand.",
                ["free"] = Coach.FreeLive,
                // The story, in full: in the file. The four shots say one sentence of it each.
                ["story-1"] = "This is your household agent. It can walk to the market, look at the stalls, and buy things. It cannot think of dinner on its own.",
                ["story-2"] = "Every evening, the family sends a message about dinner. You read it. The agent never does.",
                ["story-3"] = "You turn the message into a shopping list. That list is the agent's job description. It is the only thing the agent will know.",
                ["story-4"] = "Once it leaves, there is no phone. At every stall it looks, decides, moves, and texts you one line. Then it looks again. That is the loop.",
                ["story-5"] = "A vague list does not come home with a bad basket. It comes home with a tomato in the fruit salad. Say exactly what you mean.",
                ["story-6"] = "Six errands, one idea each. Each one opens one more part of the agent. I will coach you along the way. Let's go.",
                ["shot-1"] = "Every evening, the family sends a message about dinner.",
                ["shot-2"] = "That list is the agent's job description.",
                ["shot-3"] = "Once it leaves, there is no phone.",
                ["shot-4"] = "Say exactly what you mean.",
                // The garage, the first time, one thing at a time: her tour lines, whole or cut to a sentence.
                ["tour-1"] = "Here is the family's message. Read it closely. The agent will never see it.",
                ["tour-2"] = "Here you write the agent's list. Name the things. Say what to leave at the market.",
                ["tour-3"] = "Every word costs tokens, the agent's pocket money. This line shows what one turn will cost.",
                ["tour-4"] = "When every list is written, send the agents. From then on, nobody can help them.",
                ["garage-note"] = "Here you write the agent's list.",
                ["garage-phone"] = "Read it closely. The agent will never see it.",
                ["garage-coin"] = "Every word costs tokens, the agent's pocket money.",
                ["title"] = "Seven errands, one idea each.",
                ["cue-nobody"] = "Nobody is steering.",
                ["cue-off"] = "The one control you have left is Call them home, the off switch.",
                ["decide"] = "Mark each task: an agent, a simple automation, or neither.",
                ["round-2"] = "The catch tonight is the tomato.",
                ["round-3"] = "Everything outside that square does not exist for it.",
                ["round-4"] = "Memory is not free: every remembered turn rides along in the next prompt.",
                ["round-5"] = "Watch the cost, not just the basket.",
                ["round-6"] = "Watch the basket and the cost.",
                ["round-7"] = "Everything is in play.",
                ["arrive-3"] = "Now you choose how far the agent sees.",
                ["arrive-4"] = "Now you choose what the agent remembers.",
                ["arrive-5"] = "Now you choose the agent's brain and how hard it thinks.",
                ["chips-5"] = "Three brains: Haiku is small, Sonnet medium, Opus large.",
                ["arrive-7"] = "Every dial costs tokens on every turn.",
            };
            foreach (var l in Ladder.All)
            {
                lines["intro-" + l.number] = l.luma;
                lines["live-" + l.number] = l.live;
                var take = FirstSentence(l.takeaway);
                if (take != null) lines["take-" + l.number] = take;
                if (l.broken != null)
                {
                    lines["fix-" + l.number] = l.broken.name.TrimEnd('.') + ".";
                    var again = LastSentence(l.broken.why);
                    if (again != null) lines["again-" + l.number] = again;
                }
            }
            return lines;
        }
    }

    // Her lines the screens no longer say (she speaks only when something happens in the world:
    // an agent sent home or out of tokens, the agents home, the medal): kept for the errand's file.
    public static List<string> OnTheWay(Lesson l)
    {
        var ids = new List<string> { "title", "tour-1", "tour-2", "tour-3", "tour-4" };
        if (l == null || l.number == 1) ids.AddRange(new[] { "cue-nobody", "cue-off" });
        if (l != null)
        {
            if (l.decide) ids.Add("decide");
            foreach (var p in new[] { "arrive-", "chips-", "round-" }) ids.Add(p + l.number);
        }
        var lines = Lines;
        var said = ids.Where(lines.ContainsKey).Select(id => lines[id]).ToList();
        if (l == null || l.number == 1) said.Add(Coach.ScoredInFile);
        return said;
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

    // The last sentence of a line, when short enough to say as a cue: what to do about it.
    public static string LastSentence(string text)
    {
        if (string.IsNullOrEmpty(text)) return null;
        var parts = Regex.Split(text.Trim(), @"(?<=[.!?])\s+");
        for (int i = parts.Length - 1; i >= 0; i--) if (Words(parts[i]) <= CueWords && Words(parts[i]) > 0) return parts[i].Trim();
        return null;
    }

    // What the voice says for a line: the text without any formatting.
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
    // True while a line is still being typed or spoken: a moment that must not cover her
    // mid-sentence waits on it.
    public bool Busy => typing || Speaking;
    bool typing;
    Coroutine speaking;
    RectTransform current; // the cue on screen, so the next replaces it

    void Awake()
    {
        voice = gameObject.AddComponent<AudioSource>();
        voice.playOnAwake = false;
        voice.spatialBlend = 0f; // she speaks to the room, not from somewhere on the market
    }

    // Says a recorded line aloud (voice on, recording present) and returns its text.
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

    // In the browser a clip is decoded in the background after it is loaded, so wait for it
    // rather than play silence.
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
    // mid-sentence), gone once she is done unless it is to stay. The id must be in Lines.
    public RectTransform Cue(Transform parent, string id, float x = 40, float y = 944, float w = 820, bool stay = false)
        => Line(parent, x, y, w, Say(id), true, stay, 18, type: false);

    // A line built on the fly (an agent's name in it): shown whole, text only.
    public RectTransform Text(Transform parent, string text, float x = 40, float y = 944, float w = 820, bool stay = false)
    {
        Hush();
        return Line(parent, x, y, w, text, true, stay, 18, type: false);
    }

    // One line of coaching over a screen: her small portrait and the line, typed out as she
    // says it or shown whole. The strip fades once she is done unless it is to stay. Returns
    // the strip, so a screen can replace the line.
    public RectTransform Line(Transform parent, float x, float y, float w, string text, bool dark, bool stay = false, float size = 17, bool type = true)
    {
        if (current != null) { Destroy(current.gameObject); current = null; }
        // Opaque, both ways: the scene never shows through her strip.
        var fill = dark ? Theme.Ink : Theme.Panel; fill.a = 1f;
        var card = Ui.Box(parent, "Luma line", fill);
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

    // The cue on screen goes now (a screen that moves on).
    public void Clear() { if (current != null) { Destroy(current.gameObject); current = null; } Hush(); }

    public void Hush()
    {
        if (speaking != null) { StopCoroutine(speaking); speaking = null; }
        if (voice != null) voice.Stop();
        typing = false;
    }
}
