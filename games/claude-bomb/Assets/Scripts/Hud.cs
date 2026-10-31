using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The heads-up display over the device: the clock, the strikes and the five modules,
// and what happens when they change - a toast and a green flash for a defused module,
// a red flash and a shake for a strike, a beating clock in the last minute. Only
// reads GameManager. Added to every run by GameUi; builds its own UI.
public class Hud : MonoBehaviour
{
    public static Hud Instance { get; private set; }

    // The bar itself, for the tour to point at.
    public RectTransform Bar { get; private set; }

    CanvasGroup group, areaGroup;
    RectTransform area;
    TMP_Text clock;
    Image vignette;
    readonly List<Image> strikePips = new List<Image>();
    readonly Dictionary<ModuleId, (Image tile, TMP_Text number, Image live)> tiles = new Dictionary<ModuleId, (Image, TMP_Text, Image)>();
    readonly Queue<(string text, Color color, Sprite icon)> toasts = new Queue<(string, Color, Sprite)>();
    Coroutine toasting;
    float flash;
    Color flashColor;

    void Awake() => Instance = this;

    void Start()
    {
        Build();
        var gm = GameManager.Instance;
        gm.OnStrike += HandleStrike;
        gm.OnModuleSolved += HandleSolved;
        gm.OnGameEnded += HandleEnded;
    }

    void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnStrike -= HandleStrike;
        GameManager.Instance.OnModuleSolved -= HandleSolved;
        GameManager.Instance.OnGameEnded -= HandleEnded;
    }

    void Update()
    {
        var gm = GameManager.Instance;
        float left = gm.TimeRemaining, fraction = left / gm.StartingSeconds;
        string time = GameManager.FormatTime(left); // fixed-width digits, so the clock doesn't jitter
        clock.text = gm.NoClock ? "<size=80%>No clock</size>" : $"<mspace=0.6em>{time.Substring(0, 2)}</mspace>:<mspace=0.6em>{time.Substring(3)}</mspace>";
        clock.color = fraction > 1f / 3f ? Ui.Ink : Ui.Danger;

        // The last minute: the clock beats once a second and the edges of the screen glow red.
        bool lastMinute = gm.State == GameManager.RunState.Playing && gm.ClockStarted && left <= 60f;
        float beat = lastMinute ? Mathf.Pow(1f - Mathf.Repeat(left, 1f), 4f) : 0f;
        clock.rectTransform.localScale = Vector3.one * (1f + 0.1f * beat);
        // Out of the way while a manual page is open up close: it would cover the page's title.
        bool reading = PaperDesk.Instance != null && PaperDesk.Instance.ReaderOpen;
        areaGroup.alpha = Mathf.MoveTowards(areaGroup.alpha, reading ? 0f : 1f, Time.unscaledDeltaTime * 6f);
        areaGroup.blocksRaycasts = !reading;
        float glow = lastMinute ? 0.1f + (left <= 10f ? 0.3f : 0.18f) * beat : 0f;
        flash = Mathf.MoveTowards(flash, 0f, Time.unscaledDeltaTime * 1.1f);
        vignette.color = flash > glow ? flashColor.WithAlpha(flash) : Ui.Danger.WithAlpha(glow);
    }

    // A message across the top of the device view, e.g. at the start of a run.
    public void Announce(string text, Color color, Sprite icon = null)
    {
        toasts.Enqueue((text, color, icon));
        if (toasting == null) toasting = StartCoroutine(ShowToasts());
    }

    void HandleStrike(ModuleId module)
    {
        var gm = GameManager.Instance;
        int i = gm.Strikes - 1;
        if (i < strikePips.Count)
        {
            strikePips[i].sprite = Ui.Circle;
            strikePips[i].color = Ui.Danger;
            StartCoroutine(Pop(strikePips[i].rectTransform, 1.8f));
        }
        Flash(Ui.Danger, 0.75f);
        StartCoroutine(Wiggle(Bar));
        var cam = FindFirstObjectByType<BombCameraController>();
        if (cam != null) cam.Shake(0.014f, 0.45f);
        if (gm.Strikes < gm.MaxStrikes) Announce($"Strike {gm.Strikes} of {gm.MaxStrikes}", Ui.Danger, Ui.Cross);
    }

    void HandleSolved(ModuleId module)
    {
        var plate = Plates.Of(module);
        if (tiles.TryGetValue(module, out var tile))
        {
            tile.tile.color = Ui.Aqua;
            tile.number.color = Ui.Ink;
            tile.live.gameObject.SetActive(false);
            StartCoroutine(Pop(tile.tile.rectTransform, 1.5f));
        }
        Flash(Ui.Teal, 0.35f);
        if (GameManager.Instance.SolvedCount < Plates.All.Length)
            Announce($"Module {plate.number}, {plate.name.ToLowerInvariant()}: defused", Ui.TealDeep, Ui.Check);
    }

    void HandleEnded(GameManager.RunState state) => StartCoroutine(Ui.Fade(group, 0f, 0.6f));

    // ---- Motion ----

    IEnumerator ShowToasts()
    {
        while (toasts.Count > 0)
        {
            var (text, color, icon) = toasts.Dequeue();
            var card = Ui.NewCard("Toast", area, Color.white, out var background, 6);
            background.raycastTarget = false;
            var label = Ui.NewHeading("Text", card, text, 18f, Ui.Ink, TextAlignmentOptions.MidlineLeft);
            float width = label.GetPreferredValues().x + (icon != null ? 84f : 52f);
            Ui.Place(card, new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(width, 52f));
            Ui.Stretch(label.rectTransform, new Vector2(icon != null ? 58f : 26f, 0f), new Vector2(-24f, 0f));
            if (icon != null)
            {
                var mark = Ui.NewIcon("Icon", card, icon, color, 30f); // bare: no icon circles in this brand
                Ui.Place(mark.rectTransform, new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(30f, 30f));
            }

            var cardGroup = Ui.Group(card);
            yield return Ui.Animate(0.35f, k => { cardGroup.alpha = k; card.anchoredPosition = new Vector2(0f, -86f - 26f * k); card.localScale = Vector3.one * (0.9f + 0.1f * k); }, Ui.EaseOutBack);
            yield return new WaitForSecondsRealtime(toasts.Count > 0 ? 1.3f : 2.2f);
            yield return Ui.Animate(0.3f, k => { cardGroup.alpha = 1f - k; card.anchoredPosition = new Vector2(0f, -112f + 16f * k); });
            Destroy(card.gameObject);
        }
        toasting = null;
    }

    // The edges of the screen light up in a colour, then fade.
    void Flash(Color color, float alpha)
    {
        flashColor = color;
        flash = alpha;
    }

    static IEnumerator Pop(RectTransform rt, float size)
    {
        yield return Ui.Animate(0.45f, k => rt.localScale = Vector3.one * Mathf.Lerp(size, 1f, k), Ui.EaseOutBack);
    }

    static IEnumerator Wiggle(RectTransform rt)
    {
        Vector2 rest = rt.anchoredPosition;
        yield return Ui.Animate(0.4f, k => rt.anchoredPosition = rest + new Vector2(Mathf.Sin(k * 40f) * 10f * (1f - k), 0f), t => t);
    }

    // ---- UI construction ----

    void Build()
    {
        var canvas = Ui.NewCanvas("HudCanvas", transform, 5); // over the manual table (-1), under the help (20)
        group = Ui.Group(canvas);

        vignette = Ui.NewImage("Vignette", canvas.transform, Ui.Danger.WithAlpha(0f), Ui.Vignette);
        vignette.raycastTarget = false;
        Ui.Stretch(vignette.rectTransform);

        // Everything sits over the device, left of the chat.
        var chat = FindFirstObjectByType<ChatPanel>();
        area = Ui.NewRect("DeviceArea", canvas.transform);
        Ui.Stretch(area, Vector2.zero, new Vector2(chat != null ? -chat.Area.rect.width : -470f, 0f));
        areaGroup = Ui.Group(area);

        Bar = Ui.NewCard("Bar", area, Color.white, out var background, 8);
        Ui.Place(Bar, new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(720f, 84f));

        Section(GameManager.Instance.NoClock ? "Practice" : "Time", 26f);
        clock = Ui.NewText("Clock", Bar, "10:00", 40f, Ui.Ink, TextAlignmentOptions.Left, Ui.Display);
        clock.fontStyle = FontStyles.Bold;
        Ui.Place(clock.rectTransform, new Vector2(0f, 1f), new Vector2(24f, -30f), new Vector2(170f, 46f));
        clock.rectTransform.pivot = new Vector2(0.25f, 0.5f);
        clock.rectTransform.anchoredPosition = new Vector2(24f + 170f * 0.25f, -53f);

        Divider(206f);
        Section("Strikes", 228f);
        for (int i = 0; i < GameManager.Instance.MaxStrikes; i++)
        {
            var pip = Ui.NewIcon("Strike", Bar, Ui.Ring, Ui.Border2, 24f);
            Ui.Place(pip.rectTransform, new Vector2(0f, 1f), new Vector2(240f + i * 36f, -53f), new Vector2(24f, 24f));
            pip.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            strikePips.Add(pip);
        }

        Divider(352f);
        Section("Modules", 374f);
        foreach (var plate in Plates.All)
        {
            var tile = Ui.NewPanel("Module " + plate.number, Bar, Ui.Surface3, 4);
            tile.raycastTarget = false;
            Ui.Place(tile.rectTransform, new Vector2(0f, 1f), new Vector2(389f + (plate.number - 1) * 48f, -53f), new Vector2(38f, 34f));
            tile.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            var number = Ui.NewText("Number", tile.transform, plate.number.ToString(), 18f, Ui.Ink, TextAlignmentOptions.Center, Ui.Display);
            number.fontStyle = FontStyles.Bold;
            Ui.Stretch(number.rectTransform);
            var live = Ui.NewIcon("Live", tile.transform, Ui.Circle, Ui.Danger, 8f);
            Ui.Place(live.rectTransform, new Vector2(1f, 1f), new Vector2(-4f, -4f), new Vector2(8f, 8f));
            tiles[plate.id] = (tile, number, live);
        }

        // The menu, also on Esc and the chat's gear.
        Divider(632f);
        var pause = Ui.NewButton("Menu", Bar, "", Ui.ButtonStyle.Secondary, () => { if (GameMenu.Instance != null) GameMenu.Instance.Open(); });
        Ui.Place((RectTransform)pause.transform, new Vector2(0f, 0.5f), new Vector2(648f, 0f), new Vector2(54f, 54f));
        ((RectTransform)pause.transform).pivot = new Vector2(0.5f, 0.5f);
        ((RectTransform)pause.transform).anchoredPosition = new Vector2(675f, 0f);
        Ui.NewIcon("Icon", pause.transform, Ui.PauseIcon, Ui.Ink, 34f);
    }

    void Section(string name, float x)
    {
        var label = Ui.NewLabel(name, Bar, name, 11f, Ui.Text2);
        Ui.Place(label.rectTransform, new Vector2(0f, 1f), new Vector2(x, -14f), new Vector2(150f, 16f));
    }

    void Divider(float x)
    {
        var line = Ui.NewImage("Divider", Bar, Ui.Border2);
        line.raycastTarget = false;
        Ui.Place(line.rectTransform, new Vector2(0f, 0.5f), new Vector2(x, 0f), new Vector2(1f, 52f));
    }
}
