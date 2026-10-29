using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The game menu: Esc, the pause button on the HUD or the gear in the chat. The game
// pauses while it's open. From here the player resumes, tries the same device again,
// takes a new one, reads the help, replays LUMA's tour or goes back to the title, and
// changes the settings: sound, LUMA's voice and the clock. Anything that throws the
// current device away asks first. In a build it also opens by itself when the player
// switches away from the game, so the clock never runs unwatched. Added to every run
// by GameUi; builds its own UI.
public class GameMenu : MonoBehaviour
{
    public static GameMenu Instance { get; private set; }
    public bool IsOpen => root != null && root.gameObject.activeSelf;

    const float Width = 600f, Inner = 520f, Left = 40f;

    RectTransform root, card, main, confirm;
    TMP_Text status, clockNote, confirmTitle, confirmText;
    Button confirmButton;
    Action confirmed;
    readonly List<(Button button, int minutes)> clockButtons = new List<(Button, int)>();

    // Restarts. The same device again keeps its seed; a new one gets a new seed.
    public static void Restart(bool sameDevice)
    {
        if (sameDevice && GameManager.Instance != null) GameManager.ReplaySeed = GameManager.Instance.Seed;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void BackToTitle()
    {
        Opening.ShowTitleNextRun();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Awake() => Instance = this;

    void Start()
    {
        Build();
        root.gameObject.SetActive(false);

        // The gear in the chat header opened the help; now it opens this menu, which has the help.
        var chat = FindFirstObjectByType<ChatPanel>();
        var gear = chat != null ? chat.Area.Find("SettingsBtn") : null;
        if (gear != null && gear.TryGetComponent(out Button gearButton))
        {
            for (int i = 0; i < gearButton.onClick.GetPersistentEventCount(); i++)
                gearButton.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);
            gearButton.onClick.AddListener(() => Open());
        }
    }

    void OnDestroy() => GamePause.Release(this);

    // Once the clock has started, while the run is on, and when the tour or the access
    // code doesn't have the screen.
    static bool CanOpen
    {
        get
        {
            var gm = GameManager.Instance;
            return gm != null && gm.ClockStarted && gm.State == GameManager.RunState.Playing
                && (Tour.Instance == null || !Tour.Instance.Running)
                && (KeyPanel.Instance == null || !KeyPanel.Instance.IsOpen);
        }
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || !kb.escapeKey.wasPressedThisFrame) return;
        var help = FindFirstObjectByType<HelpPanel>();
        if (help != null && help.IsOpen) help.Hide();
        else if (IsOpen && confirm.gameObject.activeSelf) ShowMain();
        else if (IsOpen) Close();
        else Open();
    }

    void OnApplicationFocus(bool focus)
    {
#if !UNITY_EDITOR
        if (!focus && !IsOpen) Open(away: true); // switching tabs or windows pauses the game
#endif
    }

    public void Open(bool away = false)
    {
        if (!CanOpen || IsOpen) return;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null); // keys stop going to the chat
        ShowMain();
        status.text = Status(away);
        RefreshClock();
        root.gameObject.SetActive(true);
        GamePause.Hold(this);
        StartCoroutine(Ui.Animate(0.25f, k =>
        {
            Ui.Group(card).alpha = k;
            card.anchoredPosition = new Vector2(0f, -16f * (1f - k));
        }));
    }

    public void Close()
    {
        if (!IsOpen) return;
        root.gameObject.SetActive(false);
        GamePause.Release(this);
    }

    static string Status(bool away)
    {
        var gm = GameManager.Instance;
        string clock = gm.NoClock ? "Practice: there's no clock on this device." : $"The clock is stopped at {GameManager.FormatTime(gm.TimeRemaining)}.";
        string run = $" {gm.Strikes} of {gm.MaxStrikes} strikes, {gm.SolvedCount} of {Plates.All.Length} modules defused.";
        return (away ? "You switched away, so the game paused. " : "") + clock + run;
    }

    void ShowMain()
    {
        main.gameObject.SetActive(true);
        confirm.gameObject.SetActive(false);
        card.sizeDelta = new Vector2(Width, 800f);
    }

    // Asks before throwing the current device away.
    void Ask(string title, string text, string action, Action then)
    {
        confirmTitle.text = title;
        confirmText.text = text;
        confirmButton.GetComponentInChildren<TMP_Text>().text = action;
        confirmed = then;
        main.gameObject.SetActive(false);
        confirm.gameObject.SetActive(true);
        card.sizeDelta = new Vector2(Width, 412f);
    }

    void ChooseClock(int minutes)
    {
        Settings.ClockMinutes = minutes;
        RefreshClock();
    }

    void RefreshClock()
    {
        var gm = GameManager.Instance;
        int current = gm.NoClock ? 0 : Mathf.RoundToInt(gm.StartingSeconds / 60f);
        foreach (var (button, minutes) in clockButtons)
            Ui.Style(button, minutes == Settings.ClockMinutes ? Ui.ButtonStyle.Primary : Ui.ButtonStyle.Secondary);
        clockNote.text = Settings.ClockMinutes == current
            ? "Changes to the clock apply to the next device."
            : $"The next device gets {Settings.Describe(Settings.ClockMinutes)}. Restart to use it now.";
    }

    // ---- UI construction ----

    void Build()
    {
        var canvas = Ui.NewCanvas("MenuCanvas", transform, 30); // over the chat, the HUD and the help; under the tour (35)
        root = (RectTransform)canvas.transform;
        var dim = Ui.NewImage("Dim", root, Ui.Ink.WithAlpha(0.9f));
        Ui.Stretch(dim.rectTransform);

        card = Ui.NewCard("Card", root, Color.white, out var background, 20);
        Ui.Place(card, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(Width, 800f));

        main = Ui.NewRect("Main", card);
        Ui.Stretch(main);
        Text(Ui.NewLabel("Kicker", main, "Paused", 12f, Ui.Text2), -40f, 18f);
        Text(Ui.NewHeading("Title", main, "Night shift, Bay 3", 32f, Ui.Ink), -62f, 42f);
        Ui.NewUnderline(main, new Vector2(Left, -112f));
        status = Ui.NewText("Status", main, "", 18f, Ui.Text2);
        status.lineSpacing = 4f;
        Text(status, -132f, 54f);

        Row(Ui.NewButton("Resume", main, "Resume", Ui.ButtonStyle.Primary, Close, 20f), -206f, Left, Inner, 60f);
        Row(Ui.NewButton("Again", main, "Try this device again", Ui.ButtonStyle.Secondary, () =>
            Ask("Try this device again?", "The same device, with the clock and the strikes reset. What you've done on it so far is lost.",
                "Start again", () => Restart(sameDevice: true)), 18f), -276f, Left, Inner, 52f);
        Row(Ui.NewButton("New", main, "New device", Ui.ButtonStyle.Secondary, () =>
            Ask("Take a new device?", "A different device, with the clock and the strikes reset. This one is lost.",
                "New device", () => Restart(sameDevice: false)), 18f), -338f, Left, Inner, 52f);
        float half = (Inner - 12f) / 2f;
        Row(Ui.NewButton("Help", main, "How to play", Ui.ButtonStyle.Secondary, () =>
        {
            Close();
            var help = FindFirstObjectByType<HelpPanel>();
            if (help != null) help.Show();
        }, 18f), -400f, Left, half, 52f);
        Row(Ui.NewButton("Tour", main, "Take the tour again", Ui.ButtonStyle.Secondary, () =>
        {
            Close();
            if (Tour.Instance != null) Tour.Instance.Replay();
        }, 18f), -400f, Left + half + 12f, half, 52f);

        var line = Ui.NewImage("Line", main, Ui.Border2);
        line.raycastTarget = false;
        Row(line.rectTransform, -478f, Left, Inner, 1f);
        Text(Ui.NewLabel("Settings", main, "Settings", 12f, Ui.Text2), -500f, 18f);
        Setting("Sound", -530f, Ui.NewSwitch("Sound", main, Settings.Sound, on => Settings.Sound = on));
        Setting("LUMA's voice", -578f, Ui.NewSwitch("Voice", main, Settings.LumaVoice, on => Settings.LumaVoice = on));
        var clockLabel = Ui.NewText("Clock", main, "Clock", 19f, Ui.Ink, TextAlignmentOptions.MidlineLeft);
        Row(clockLabel.rectTransform, -626f, Left, 140f, 40f);
        for (int i = 0; i < Settings.ClockChoices.Length; i++)
        {
            int minutes = Settings.ClockChoices[i];
            var choice = Ui.NewButton("Clock " + minutes, main, minutes > 0 ? $"{minutes} min" : "No clock", Ui.ButtonStyle.Secondary, () => ChooseClock(minutes), 16f);
            Row((RectTransform)choice.transform, -626f, Left + Inner - (3 - i) * 112f + 8f, 104f, 40f);
            clockButtons.Add((choice, minutes));
        }
        clockNote = Ui.NewText("ClockNote", main, "", 15f, Ui.Text2);
        Text(clockNote, -676f, 22f);

        Row(Ui.NewButton("Title", main, "Back to the title", Ui.ButtonStyle.Ghost, () =>
            Ask("Back to the title?", "You'll see the title, the briefing and the tour again. This device is lost.",
                "Back to the title", BackToTitle), 16f), -728f, Left, 220f, 46f);
        var hint = Ui.NewText("Hint", main, "Esc to resume", 15f, Ui.Text2, TextAlignmentOptions.MidlineRight);
        Row(hint.rectTransform, -728f, Left + Inner - 200f, 200f, 46f);

        confirm = Ui.NewRect("Confirm", card);
        Ui.Stretch(confirm);
        Text(Ui.NewLabel("Kicker", confirm, "Are you sure?", 12f, Ui.Text2), -40f, 18f);
        confirmTitle = Ui.NewHeading("Title", confirm, "", 32f, Ui.Ink);
        Text(confirmTitle, -62f, 42f);
        Ui.NewUnderline(confirm, new Vector2(Left, -112f));
        confirmText = Ui.NewText("Text", confirm, "", 19f, Ui.Text2);
        confirmText.lineSpacing = 4f;
        Text(confirmText, -134f, 90f);
        confirmButton = Ui.NewButton("Yes", confirm, "", Ui.ButtonStyle.Primary, () => confirmed?.Invoke(), 18f);
        Row((RectTransform)confirmButton.transform, -246f, Left, Inner, 56f);
        Row(Ui.NewButton("No", confirm, "Keep playing", Ui.ButtonStyle.Secondary, ShowMain, 18f), -314f, Left, Inner, 52f);
        confirm.gameObject.SetActive(false);
    }

    // A setting: its name on the left, its switch on the right.
    void Setting(string label, float y, Button control)
    {
        var name = Ui.NewText(label, main, label, 19f, Ui.Ink, TextAlignmentOptions.MidlineLeft);
        Row(name.rectTransform, y, Left, 300f, 40f);
        var rt = (RectTransform)control.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.anchoredPosition = new Vector2(Left + Inner, y - 20f);
    }

    static void Text(TMP_Text text, float y, float height) => Row(text.rectTransform, y, Left, Inner, height);

    static void Row(Button button, float y, float x, float width, float height) => Row((RectTransform)button.transform, y, x, width, height);

    // From the card's top-left: x, y (negative, down), width and height. The pivot is the
    // middle, so buttons grow from their centre under the pointer.
    static void Row(RectTransform rt, float y, float x, float width, float height)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x + width / 2f, y - height / 2f);
        rt.sizeDelta = new Vector2(width, height);
    }
}
