using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// How a run begins. The first time in a session: the title screen over a slowly
// turning device, seen as if on the lab's security camera - a Maaloomatiia cover, on
// Deep Knowledge Blue with the full wordmark; the access code, if this
// copy of the game needs one (KeyPanel); the radio briefing that sets the scene; then
// LUMA's guided tour (Tour). After a Restart it goes straight to a new device. The
// game clock starts when the opening is over. Added to every run by GameUi.
public class Opening : MonoBehaviour
{
    static bool seen; // kept across Restart, like LUMA's tips

    // The menu's "Back to the title": the next run starts with the whole opening again.
    public static void ShowTitleNextRun() => seen = false;

    const string Where = "23:47  /  Meridian Labs, Bay 3";
    static readonly string[] Briefing =
    {
        "A cleaner found a device under the sample bench in Bay 3. Five modules. Armed.",
        "The bomb squad is forty minutes out. Their expert is already on the radio: Claude.",
        "Claude can read the device's manuals, but it can't see the device. You can see the device, but the manuals are in Japanese.",
        "<b><color=#24E0C0>You are its eyes. {clock}</color></b>",
    };

    static string ClockLine(GameManager gm) =>
        gm.NoClock ? "Tonight is practice: there's no clock." : $"You have {Mathf.RoundToInt(gm.StartingSeconds / 60f)} minutes.";

    RectTransform root, title, briefing;
    CanvasGroup group;
    TMP_Text cameraClock;
    Image recDot, radioDot;
    GameObject readyButton;
    readonly List<TMP_Text> lines = new List<TMP_Text>();
    readonly List<RectTransform> wave = new List<RectTransform>();
    bool begun, ready, hurry, talking;
    float since;

    IEnumerator Start()
    {
        yield return null; // every screen has built its UI by now
        var gm = GameManager.Instance;
        if (seen)
        {
            var chatPanel = FindFirstObjectByType<ChatPanel>();
            var view = FindFirstObjectByType<BombCameraController>();
            if (chatPanel != null && view != null) view.FrameBeside = chatPanel.Area;
            gm.StartClock();
            if (chatPanel != null) chatPanel.AddDivider("New device  -  channel open");
            if (Hud.Instance != null) Hud.Instance.Announce("New device. The clock is running", Ui.TealDeep);
            yield break;
        }
        seen = true;

        var cam = FindFirstObjectByType<BombCameraController>();
        var chat = FindFirstObjectByType<ChatPanel>();
        if (chat != null) cam.FrameBeside = chat.Area;
        cam.FrameWidth = 0.6f; // the device on the left, the title on the right
        cam.Scripted = true;
        cam.Glide(-40f, 14f, 1.55f, null, 0.01f);
        Build();
        // The game's own interface waits until the tour shows it.
        var interfaceParts = GameInterface();
        foreach (var part in interfaceParts) part.alpha = 0f;
        yield return new WaitForSecondsRealtime(0.05f);
        cam.Orbit(8f);

        // The title.
        yield return Reveal(title);
        while (!begun)
        {
            if (Pressed()) begun = true;
            yield return null;
        }

        // The access code, if this copy of the game needs one.
        if (!ClaudeClient.Ready)
        {
            if (KeyPanel.Instance != null) KeyPanel.Instance.Show();
            while (!ClaudeClient.Ready) yield return null;
        }

        // The briefing, typed out as it comes over the radio.
        SoundManager.Instance.PlayWhoosh();
        yield return Ui.Fade(Ui.Group(title), 0f, 0.35f);
        title.gameObject.SetActive(false);
        briefing.gameObject.SetActive(true);
        yield return Ui.Fade(Ui.Group(briefing), 1f, 0.4f);
        talking = true;
        foreach (var line in lines)
        {
            yield return Ui.Type(line, line.text, 40f, () => hurry, SoundManager.Instance.PlayBlip);
            if (!hurry) yield return new WaitForSecondsRealtime(0.6f);
        }
        talking = false;
        readyButton.SetActive(true);
        StartCoroutine(Ui.Animate(0.4f, k => readyButton.transform.localScale = Vector3.one * k, Ui.EaseOutBack));
        yield return null; // the key that hurried the last line doesn't also count as ready
        while (!ready)
        {
            if (Pressed()) ready = true;
            yield return null;
        }

        // Then LUMA shows them around, and the clock starts.
        cam.Orbit(0f);
        cam.FrameWidth = null;
        foreach (var part in interfaceParts) StartCoroutine(Ui.Fade(part, 1f, 0.8f));
        yield return Ui.Fade(group, 0f, 0.6f);
        root.gameObject.SetActive(false);
        yield return Tour.Instance.Run(firstTime: true);
        cam.Scripted = false;
        gm.StartClock();
        if (chat != null) chat.AddDivider("Channel open  -  Claude is listening");
        Destroy(root.gameObject);
    }

    void Update()
    {
        if (root == null) return;
        if (talking && Pressed()) hurry = true;
        since += Time.unscaledDeltaTime;
        // The security camera's clock runs from 23:47:00, and its REC light blinks.
        int s = 47 * 60 + Mathf.FloorToInt(since);
        cameraClock.text = $"<mspace=0.62em>23:{s / 60 % 60:00}:{s % 60:00}</mspace>";
        recDot.color = Ui.Danger.WithAlpha(Mathf.Repeat(since, 1.2f) < 0.7f ? 1f : 0.15f);
        radioDot.color = Ui.Teal.WithAlpha(talking ? 0.4f + 0.6f * Mathf.Abs(Mathf.Sin(since * 5f)) : 1f);
        for (int i = 0; i < wave.Count; i++)
        {
            float h = talking ? 6f + 22f * Mathf.Abs(Mathf.Sin(since * (7f + i * 1.7f) + i)) * Mathf.PerlinNoise(since * 3f, i) : 4f;
            wave[i].sizeDelta = new Vector2(4f, Mathf.Lerp(wave[i].sizeDelta.y, h, 0.4f));
        }
    }

    // The chat, the HUD, the arrows and the MANUALS button.
    static List<CanvasGroup> GameInterface()
    {
        var parts = new List<CanvasGroup>();
        var chat = FindFirstObjectByType<ChatPanel>();
        if (chat != null) parts.Add(Ui.Group(chat.Area));
        if (Hud.Instance != null && Hud.Instance.Bar != null) parts.Add(Ui.Group(Hud.Instance.Bar));
        var arrows = FindFirstObjectByType<ViewButtons>();
        if (arrows != null && arrows.Cluster != null) parts.Add(Ui.Group(arrows.Cluster));
        if (PaperDesk.Instance != null && PaperDesk.Instance.ManualsButton != null) parts.Add(Ui.Group(PaperDesk.Instance.ManualsButton));
        return parts;
    }

    static bool Pressed()
    {
        var kb = Keyboard.current;
        return kb != null && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame);
    }

    // Brings a block's pieces in one after another, each rising into place.
    IEnumerator Reveal(RectTransform block)
    {
        var pieces = new List<(RectTransform rt, Vector2 rest, CanvasGroup g)>();
        foreach (RectTransform piece in block)
        {
            var g = Ui.Group(piece);
            g.alpha = 0f;
            pieces.Add((piece, piece.anchoredPosition, g));
        }
        foreach (var (rt, rest, g) in pieces)
        {
            StartCoroutine(Ui.Animate(0.6f, k => { g.alpha = k; rt.anchoredPosition = rest + new Vector2(0f, -24f * (1f - k)); }));
            yield return new WaitForSecondsRealtime(0.12f);
        }
    }

    // ---- UI construction ----

    void Build()
    {
        var canvas = Ui.NewCanvas("OpeningCanvas", transform, 40); // over everything but the access code
        root = (RectTransform)canvas.transform;
        group = Ui.Group(canvas);

        // A click anywhere hurries the briefing along.
        var back = Ui.NewImage("Back", root, Color.clear);
        Ui.Stretch(back.rectTransform);
        var backButton = back.gameObject.AddComponent<Button>();
        backButton.transition = Selectable.Transition.None;
        backButton.onClick.AddListener(() => { if (talking) hurry = true; });

        var vignette = Ui.NewImage("Vignette", root, Ui.Ink.WithAlpha(0.95f), Ui.Vignette);
        vignette.raycastTarget = false;
        Ui.Stretch(vignette.rectTransform);
        // Night on the right, where the title stands; the device is framed on the left.
        var shade = Ui.NewImage("Shade", root, Ui.Ink, Ui.Gradient);
        shade.raycastTarget = false;
        shade.rectTransform.anchorMin = new Vector2(0.4f, 0f);
        shade.rectTransform.anchorMax = new Vector2(0.6f, 1f);
        shade.rectTransform.offsetMin = shade.rectTransform.offsetMax = Vector2.zero;
        var night = Ui.NewImage("Night", root, Ui.Ink);
        night.raycastTarget = false;
        night.rectTransform.anchorMin = new Vector2(0.6f, 0f);
        night.rectTransform.anchorMax = Vector2.one;
        night.rectTransform.offsetMin = night.rectTransform.offsetMax = Vector2.zero;

        BuildCamera();
        BuildTitle();
        BuildBriefing();
    }

    // The security camera's frame around the device: corners, REC, the time.
    void BuildCamera()
    {
        var frame = Ui.NewRect("Camera", root);
        frame.anchorMin = Vector2.zero;
        frame.anchorMax = new Vector2(0.6f, 1f);
        frame.offsetMin = new Vector2(48f, 48f);
        frame.offsetMax = new Vector2(-48f, -48f);
        Color line = Ui.Faint.WithAlpha(0.5f);
        foreach (var corner in new[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one })
        {
            var sign = new Vector2(corner.x == 0f ? 1f : -1f, corner.y == 0f ? 1f : -1f);
            foreach (var size in new[] { new Vector2(44f, 2f), new Vector2(2f, 44f) })
            {
                var stroke = Ui.NewImage("Corner", frame, line);
                stroke.raycastTarget = false;
                stroke.rectTransform.anchorMin = stroke.rectTransform.anchorMax = stroke.rectTransform.pivot = corner;
                stroke.rectTransform.sizeDelta = size;
            }
        }

        recDot = Ui.NewIcon("Rec", frame, Ui.Circle, Ui.Danger, 14f);
        Ui.Place(recDot.rectTransform, new Vector2(0f, 1f), new Vector2(22f, -24f), new Vector2(14f, 14f));
        var rec = Ui.NewLabel("RecLabel", frame, "Rec   Cam 02  /  Bay 3", 13f, Color.white);
        Ui.Place(rec.rectTransform, new Vector2(0f, 1f), new Vector2(46f, -22f), new Vector2(500f, 20f));
        cameraClock = Ui.NewText("Time", frame, "", 18f, Color.white, TextAlignmentOptions.TopRight, Ui.Display);
        Ui.Place(cameraClock.rectTransform, new Vector2(1f, 1f), new Vector2(-22f, -20f), new Vector2(220f, 24f));
        var site = Ui.NewLabel("Site", frame, "Meridian Labs  /  Night shift", 12f, Ui.Faint);
        Ui.Place(site.rectTransform, new Vector2(0f, 0f), new Vector2(22f, 22f), new Vector2(500f, 18f));
    }

    void BuildTitle()
    {
        title = Block("Title");
        // The cover's wordmark, top-left of the title, with its clear space around it.
        var logo = Ui.NewLogo(title, dark: true, 300f);
        Ui.Place(logo.rectTransform, new Vector2(0f, 1f), Vector2.zero, logo.rectTransform.sizeDelta);
        Piece(Ui.NewLabel("Kicker", title, "Night shift  /  Bay 3", 13f, Ui.Faint), -92f, 20f);
        var claude = Ui.NewHeading("Claude", title, "Claude", 120f, Color.white, TextAlignmentOptions.TopLeft);
        Piece(claude, -116f, 136f);
        var bomb = Ui.NewHeading("Bomb", title, "Bomb", 120f, Ui.Teal, TextAlignmentOptions.TopLeft); // teal on navy: the brand's highlight
        Piece(bomb, -232f, 136f);
        var tagline = Ui.NewText("Tagline", title, "Claude knows how to defuse it.\nOnly you can see it.", 28f, Ui.Faint);
        tagline.lineSpacing = 8f;
        Piece(tagline, -388f, 90f);
        var begin = Ui.NewButton("Begin", title, "Begin", Ui.ButtonStyle.Accent, () => begun = true, 24f);
        var brt = (RectTransform)begin.transform;
        Ui.Place(brt, new Vector2(0f, 1f), new Vector2(0f, -496f), new Vector2(280f, 68f));
        Piece(Ui.NewLabel("Hint", title, "Press Enter  /  Best with sound on", 11f, Ui.Faint), -586f, 16f);
        Piece(Ui.NewText("Maker", title, "A Maaloomatiia Academy simulation for practising work with Claude.", 16f, Ui.Faint), -616f, 24f);
    }

    void BuildBriefing()
    {
        briefing = Block("Briefing");
        Ui.Group(briefing).alpha = 0f;
        briefing.gameObject.SetActive(false);

        radioDot = Ui.NewIcon("Live", briefing, Ui.Circle, Ui.Teal, 14f);
        Ui.Place(radioDot.rectTransform, new Vector2(0f, 1f), new Vector2(0f, -8f), new Vector2(14f, 14f));
        var header = Ui.NewHeading("Header", briefing, "Incoming transmission", 26f, Color.white);
        Ui.Place(header.rectTransform, new Vector2(0f, 1f), new Vector2(26f, 0f), new Vector2(480f, 32f));
        var waveform = Ui.NewRect("Wave", briefing);
        Ui.Place(waveform, new Vector2(1f, 1f), new Vector2(0f, -10f), new Vector2(84f, 30f));
        for (int i = 0; i < 9; i++)
        {
            var bar = Ui.NewImage("Bar", waveform, Ui.Teal.WithAlpha(0.85f));
            bar.raycastTarget = false;
            bar.rectTransform.anchorMin = bar.rectTransform.anchorMax = bar.rectTransform.pivot = new Vector2(0f, 0.5f);
            bar.rectTransform.anchoredPosition = new Vector2(i * 9f, 0f);
            bar.rectTransform.sizeDelta = new Vector2(4f, 4f);
            wave.Add(bar.rectTransform);
        }
        var place = Ui.NewLabel("Place", briefing, Where, 12f, Ui.Faint);
        Ui.Place(place.rectTransform, new Vector2(0f, 1f), new Vector2(0f, -42f), new Vector2(640f, 18f));
        Ui.NewUnderline(briefing, new Vector2(0f, -74f));

        var body = Ui.NewRect("Lines", briefing);
        Ui.Place(body, new Vector2(0f, 1f), new Vector2(0f, -100f), new Vector2(640f, 420f));
        var layout = body.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 22f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        foreach (var text in Briefing)
        {
            var line = Ui.NewText("Line", body, text.Replace("{clock}", ClockLine(GameManager.Instance)), 27f, Color.white);
            line.lineSpacing = 6f;
            line.maxVisibleCharacters = 0;
            lines.Add(line);
        }

        var go = Ui.NewButton("Ready", briefing, "I'm ready", Ui.ButtonStyle.Accent, () => ready = true, 24f);
        var grt = (RectTransform)go.transform;
        grt.anchorMin = grt.anchorMax = new Vector2(0f, 1f);
        grt.pivot = new Vector2(0.5f, 0.5f); // pops in from its centre
        grt.anchoredPosition = new Vector2(150f, -576f);
        grt.sizeDelta = new Vector2(300f, 72f);
        readyButton = go.gameObject;
        readyButton.SetActive(false);
        var skip = Ui.NewButton("Skip", briefing, "Skip", Ui.ButtonStyle.GhostOnDark, () => hurry = true, 15f);
        Ui.Place((RectTransform)skip.transform, new Vector2(1f, 1f), new Vector2(-104f, 4f), new Vector2(92f, 36f));
    }

    // The column on the right where the title and the briefing appear.
    RectTransform Block(string name)
    {
        var block = Ui.NewRect(name, root);
        block.anchorMin = block.anchorMax = new Vector2(1f, 0.5f);
        block.pivot = new Vector2(1f, 1f);
        block.anchoredPosition = new Vector2(-120f, 320f); // vertically centred: the block is 640 tall
        block.sizeDelta = new Vector2(640f, 640f);
        return block;
    }

    // A piece of the title: full width of the column, top at y, this tall.
    static void Piece(TMP_Text text, float y, float height)
    {
        text.textWrappingMode = TextWrappingModes.Normal;
        Ui.Place(text.rectTransform, new Vector2(0f, 1f), new Vector2(0f, y), new Vector2(640f, height));
    }
}
