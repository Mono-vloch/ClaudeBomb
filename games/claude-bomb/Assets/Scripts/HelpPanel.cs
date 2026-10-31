using TMPro;
using UnityEngine;
using UnityEngine.UI;

// "How to play", opened from the gear button: the situation, the controls and the
// rules, and a way to take LUMA's tour again. It deliberately says nothing about
// sharing manual pages with Claude - finding that out is part of the game, and
// Claude tells the player when asked. The clock is frozen while the panel is open.
// Builds its own UI at runtime.
public class HelpPanel : MonoBehaviour
{
    static readonly (string title, string body)[] Left =
    {
        ("The situation",
         "Night shift, Bay 3. A live device with five modules is in front of you. You can see it, but you don't know how to defuse it. " +
         "Claude, the expert on the radio, knows how - but it can't see the device. Talk to Claude in the chat on the right.\n" +
         "{clock} Three mistakes and the device explodes. Defuse all five modules to win."),
        ("Look at the device",
         "Rotate: the arrow buttons, the arrow keys, or hold the right mouse button and drag\n" +
         "Zoom: mouse wheel, or W and S\n" +
         "Move left and right: A and D\n" +
         "Look at every side: some information is on the back and underneath."),
    };

    static readonly (string title, string body)[] Right =
    {
        ("Use the modules",
         "Cut a wire: hold the left mouse button and drag across the wire\n" +
         "Buttons, glyphs, coloured cells and switches: left click\n" +
         "Sequence strip: press PLAY to watch the lamps flash\n" +
         "A module's light turns from red to green when it is solved."),
        ("The manuals",
         "Manuals (bottom left) opens the table with the manual pages.\n" +
         "Click a page to look at it up close. Click or drag over any text to black it out."),
        ("LUMA",
         "Your coach. LUMA speaks up with a tip when it can help, and the clock stops while it talks."),
        ("The menu",
         "Esc, the pause button or the gear: pause, start again, take a new device, and set the sound, LUMA's voice and the clock."),
    };

    GameObject panel;

    public bool IsOpen => panel != null && panel.activeSelf;

    void Start()
    {
        Build();
        panel.SetActive(false);
    }

    public void Show()
    {
        panel.SetActive(true);
        GamePause.Hold(this);
    }

    public void Hide()
    {
        panel.SetActive(false);
        GamePause.Release(this);
    }

    void OnDestroy() => GamePause.Release(this);

    void Build()
    {
        var canvas = Ui.NewCanvas("HelpCanvas", transform, 20); // over the chat, the manual table and the HUD

        // Full-screen dim: also blocks clicks from reaching the bomb behind it.
        panel = Ui.NewImage("HelpPanel", canvas.transform, Ui.Ink.WithAlpha(0.94f)).gameObject;
        Ui.Stretch((RectTransform)panel.transform);

        var card = Ui.NewCard("Card", panel.transform, Color.white, out var background, 24);
        Ui.Place(card, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1320f, 900f));

        var kicker = Ui.NewLabel("Kicker", card, "Field guide", 12f, Ui.Text2);
        Ui.Place(kicker.rectTransform, new Vector2(0f, 1f), new Vector2(60f, -46f), new Vector2(600f, 18f));
        var title = Ui.NewHeading("Title", card, "How to play", 40f, Ui.Ink);
        Ui.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(58f, -70f), new Vector2(900f, 52f));
        Ui.NewUnderline(card, new Vector2(60f, -130f));

        Column(card, Left, 60f);
        Column(card, Right, 690f);

        var back = Ui.NewButton("Close", card, "Back to the game", Ui.ButtonStyle.Primary, Hide, 20f);
        Ui.Place((RectTransform)back.transform, new Vector2(1f, 0f), new Vector2(-60f, 44f), new Vector2(340f, 66f));
        var tour = Ui.NewButton("Tour", card, "Take the tour again", Ui.ButtonStyle.Secondary, () =>
        {
            Hide();
            if (Tour.Instance != null) Tour.Instance.Replay();
        }, 18f);
        Ui.Place((RectTransform)tour.transform, new Vector2(1f, 0f), new Vector2(-420f, 44f), new Vector2(330f, 66f));
    }

    // The clock as this device has it (the menu can change it).
    static string ClockText()
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.NoClock) return "This device has no clock: it's practice.";
        return $"You have {Mathf.RoundToInt(gm.StartingSeconds / 60f)} minutes, and the clock starts after LUMA's tour.";
    }

    // One column of sections: a small coloured heading over each block of text.
    static void Column(RectTransform card, (string title, string body)[] sections, float x)
    {
        var column = Ui.NewRect("Column", card);
        Ui.Place(column, new Vector2(0f, 1f), new Vector2(x, -158f), new Vector2(570f, 600f));
        var layout = column.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        foreach (var (heading, body) in sections)
        {
            Ui.NewHeading("Heading", column, heading, 20f, Ui.Ink);
            var text = Ui.NewText("Body", column, body.Replace("{clock}", ClockText()), 19f, Ui.Text2);
            text.lineSpacing = 8f;
            var gap = Ui.NewRect("Gap", column).gameObject.AddComponent<LayoutElement>();
            gap.preferredHeight = 16f;
        }
    }
}
