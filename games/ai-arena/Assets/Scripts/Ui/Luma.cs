using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// LUMA, the coach: the same character as in Claude Bomb. She opens each level,
// speaks up while the round runs when something worth naming happens, and reads
// the recap on the debrief. Her fixed lines are recorded, one file per line id
// in Resources/LumaVoice (AI Arena > Generate LUMA Voice); lines built on the
// fly, with an agent's name in them, are shown as text only.
public class Luma : MonoBehaviour
{
    // Every recorded line, by id. The level lines come from the ladder, so the
    // lesson text and the voice can never drift apart.
    public static Dictionary<string, string> Lines
    {
        get
        {
            var lines = new Dictionary<string, string>
            {
                ["recap"] = "The agents are home. Here is what each one practiced, and the one idea to take from this errand.",
                ["free"] = Coach.FreeLive,
                // The story, before the first errand.
                ["story-1"] = "This is your household agent. It can walk to the market, look at the stalls, and buy things. It cannot think of dinner on its own.",
                ["story-2"] = "Every evening, the family sends a message about dinner. You read it. The agent never does.",
                ["story-3"] = "You turn the message into a shopping list. That list is the agent's job description. It is the only thing the agent will know.",
                ["story-4"] = "Once it leaves, there is no phone. At every stall it looks, decides, moves, and texts you one line. Then it looks again. That is the loop.",
                ["story-5"] = "A vague list does not come home with a bad basket. It comes home with a tomato in the fruit salad. Say exactly what you mean.",
                ["story-6"] = "Six errands, one idea each. Each one opens one more part of the agent. I will coach you along the way. Let's go.",
                // The tour of the setup screen, on the first errand.
                ["tour-1"] = "Here is the family's message. Read it closely. The agent will never see it.",
                ["tour-2"] = "Here you write the agent's list. Name the things. Say what to leave at the market.",
                ["tour-3"] = "Every word costs tokens, the agent's pocket money. This line shows what one turn will cost.",
                ["tour-4"] = "When every list is written, send the agents. From then on, nobody can help them.",
            };
            foreach (var l in Ladder.All)
            {
                lines["intro-" + l.number] = l.luma;
                lines["live-" + l.number] = l.live;
            }
            return lines;
        }
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

    void Awake()
    {
        voice = gameObject.AddComponent<AudioSource>();
        voice.playOnAwake = false;
        voice.spatialBlend = 0f; // she speaks to the room, not from somewhere on the board
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

    Coroutine speaking;

    // In the browser a clip is decoded in the background after it is loaded, so
    // wait for it rather than play silence.
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

    public void Hush()
    {
        if (speaking != null) { StopCoroutine(speaking); speaking = null; }
        if (voice != null) voice.Stop();
    }

    // Whether a recorded line is still being said.
    public bool Speaking => speaking != null || (voice != null && voice.isPlaying);

    // A card with her portrait on the left, her name, what she says, and an
    // optional second line in a quieter colour. On the dark cover screens the
    // card is dark too. The returned text can be changed as the round goes on.
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
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
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
