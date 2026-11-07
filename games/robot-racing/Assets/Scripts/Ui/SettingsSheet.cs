using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The settings, behind one button: the things a player never needs on the screen.
// The speed training runs at (the fastest unless said otherwise), LUMA's voice and
// the sound. Click anywhere off the sheet to close it.
public static class SettingsSheet
{
    static RectTransform open;

    public static void Toggle(App app, Transform screen, System.Action changed)
    {
        if (open != null) { Close(); return; }
        if (screen == null) return;
        open = Ui.Node("Settings", screen);
        Ui.Stretch(open);
        var dim = Ui.Box(open, "Dim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.55f), false);
        Ui.Stretch(dim.rectTransform);
        dim.gameObject.AddComponent<Button>().onClick.AddListener(() => { Close(); changed?.Invoke(); });
        var card = Ui.Box(open, "Sheet", Ui.DarkFill);
        Ui.Place(card.rectTransform, 560, 300, 800, 360);
        var t = card.transform;
        var tag = Ui.Eyebrow(t, "SETTINGS", Theme.Teal);
        tag.font = Theme.Bold;
        Ui.Place(tag.rectTransform, 40, 28, 400, 20);
        var close = Ui.Button(t, "Done", () => { Close(); changed?.Invoke(); }, Ui.ButtonStyle.Primary, 16);
        Ui.Place(close.GetComponent<RectTransform>(), 800 - 40 - 100, 24, 100, 40);

        var sl = Ui.Text(t, "Training speed", 18, Color.white, true);
        Ui.Place(sl.rectTransform, 40, 80, 300, 26);
        var speed = new Ui.Segmented(t, new[] { "1x", "10x", "100x", "Max" }, Mathf.Clamp(app.speedIndex, 0, 3), 340, 40, 16, dark: true);
        Ui.Place(speed.root, 400, 74, 340, 40);
        speed.OnSelect = i => { app.speedIndex = i; RunScreen.Current?.SetSpeed(i); };

        var vl = Ui.Text(t, "LUMA's voice", 18, Color.white, true);
        Ui.Place(vl.rectTransform, 40, 136, 300, 26);
        Button voice = null;
        voice = Ui.Button(t, Luma.Voice ? "On" : "Off", () => { Luma.Voice = !Luma.Voice; if (!Luma.Voice) app.luma.Hush(); Ui.SetLabel(voice, Luma.Voice ? "On" : "Off"); }, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(voice.GetComponent<RectTransform>(), 400, 130, 140, 40);
        var sdl = Ui.Text(t, "Sound", 18, Color.white, true);
        Ui.Place(sdl.rectTransform, 40, 192, 300, 26);
        Button sound = null;
        sound = Ui.Button(t, Sfx.Enabled ? "On" : "Off", () => { Sfx.Enabled = !Sfx.Enabled; Ui.SetLabel(sound, Sfx.Enabled ? "On" : "Off"); }, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(sound.GetComponent<RectTransform>(), 400, 186, 140, 40);
        Sfx.Tick();
    }

    public static void Close()
    {
        if (open != null) Object.Destroy(open.gameObject);
        open = null;
    }
}
