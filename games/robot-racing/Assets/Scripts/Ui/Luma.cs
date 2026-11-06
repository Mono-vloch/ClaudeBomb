using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// LUMA, the coach: the same character as in Claude Bomb and AI Arena. She opens
// each level, speaks while the robot trains, and reads the recap on the debrief.
// Her fixed lines are recorded, one file per line id in Resources/LumaVoice
// (Robot Racing > Generate LUMA Voice); lines built on the fly are text only.
public class Luma : MonoBehaviour
{
    public static Dictionary<string, string> Lines
    {
        get
        {
            var lines = new Dictionary<string, string>
            {
                ["recap"] = "The race is over. Here is what the robot learned, what you did against the checks, and the one idea to take from this level.",
                ["welcome"] = "I'm LUMA, your coach. Nobody drives this robot. You decide what it senses, what it may do and what earns points; it learns the rest.",
            };
            foreach (var l in Ladder.All)
            {
                lines["intro-" + l.number] = l.luma;
                lines["live-" + l.number] = l.live;
            }
            return lines;
        }
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
    public bool Speaking => voice != null && voice.isPlaying;
    Coroutine speaking;

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
        speaking = null;
    }

    // One line of coaching over a screen: her small portrait and the line typed out as she
    // says it (or at reading speed when there is no recording). The strip fades once she is
    // done unless it is to stay. Returns the strip, so a screen can replace the line.
    public RectTransform Line(Transform parent, float x, float y, float w, string text, bool dark, bool stay = false, float size = 17)
    {
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
        StartCoroutine(RunLine(card, group, body, text ?? "", stay));
        return card.rectTransform;
    }

    System.Collections.IEnumerator RunLine(Image card, CanvasGroup group, TMP_Text body, string text, bool stay)
    {
        yield return Ui.Type(body, text, 45f);
        float waited = 0f;
        while (card != null && Speaking && waited < 30f) { waited += Time.unscaledDeltaTime; yield return null; }
        if (stay || card == null) yield break;
        yield return new WaitForSecondsRealtime(2.5f);
        if (card == null) yield break;
        yield return Ui.Fade(group, 0f, 0.6f);
        if (card != null) card.gameObject.SetActive(false);
    }

    public void Hush()
    {
        if (speaking != null) { StopCoroutine(speaking); speaking = null; }
        if (voice != null) voice.Stop();
    }

    public static TextMeshProUGUI Card(Transform parent, float x, float y, float w, float h, string text, float fontSize = 18,
                                       string second = null, Luma voiceToggle = null, bool dark = false, float portrait = 120)
        => Make(parent, x, y, w, h, text, fontSize, second, voiceToggle, dark, portrait).text;

    public static (Image card, TextMeshProUGUI text) Make(Transform parent, float x, float y, float w, float h, string text, float fontSize = 18,
                                                           string second = null, Luma voiceToggle = null, bool dark = false, float portrait = 120)
    {
        var card = Ui.Card(parent, "Luma", dark ? Theme.Ink2 : Theme.Panel, dark ? Theme.TealDeep : Theme.Border1);
        Ui.Place(card.rectTransform, x, y, w, h);
        var t = card.transform;
        float p = Mathf.Min(h - 24, portrait);
        var frame = Ui.Box(t, "Portrait", dark ? Theme.Ink : Theme.Surface2);
        Ui.Place(frame.rectTransform, 12, 12, p, h - 24);
        var sprite = Portrait;
        if (sprite != null)
        {
            var img = Ui.Node("Picture", frame.transform).gameObject.AddComponent<Image>();
            img.sprite = sprite; img.preserveAspect = true; img.raycastTarget = false;
            Ui.Stretch(img.rectTransform, 6, 6, 6, 6);
        }
        var name = Ui.Eyebrow(t, "LUMA", dark ? Theme.Teal : Theme.TealDeep);
        name.font = Theme.Bold;
        Ui.Place(name.rectTransform, p + 28, 14, 200, 22);
        float textH = second == null ? h - 50 : (h - 50) * 0.5f;
        var body = Ui.Text(t, text, fontSize, dark ? Color.white : Theme.Ink);
        Ui.Place(body.rectTransform, p + 28, 38, w - p - 44, textH);
        if (second != null)
        {
            var more = Ui.Text(t, second, fontSize - 2, dark ? Theme.Faint : Theme.Text2);
            Ui.Place(more.rectTransform, p + 28, 38 + textH + 4, w - p - 44, h - 50 - textH - 4);
        }
        if (voiceToggle != null)
        {
            Button toggle = null;
            toggle = Ui.Button(t, Voice ? "Voice on" : "Voice off", () =>
            {
                Voice = !Voice;
                if (!Voice) voiceToggle.Hush();
                Ui.SetLabel(toggle, Voice ? "Voice on" : "Voice off");
            }, dark ? Ui.ButtonStyle.GhostOnDark : Ui.ButtonStyle.Quiet, 14);
            Ui.Place(toggle.GetComponent<RectTransform>(), w - 122, 10, 110, 32);
        }
        return (card, body);
    }
}
