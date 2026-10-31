using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// LUMA's guided tour, before the clock starts (and again from the help screen). The
// screen dims and a soft spotlight moves from one thing to the next - the device and
// its numbered plates, the casing, Claude's chat, the manuals, the HUD, the camera
// arrows - while the camera glides to each and LUMA explains it aloud from a card
// beside the beam. The first tour ends with a countdown, and LUMA flies into the
// chat, where it stays for the rest of the game. NEXT (or Space) moves on early,
// SKIP TOUR (or Escape) ends it. Builds its own UI; added to every run by GameUi.
public class Tour : MonoBehaviour
{
    public static Tour Instance { get; private set; }
    public bool Running { get; private set; }

    enum Spot { Nothing, Device, Casing, Chat, Manuals, Hud, Arrows }

    static readonly (string line, Spot spot, LumaRig.Mood mood)[] Steps =
    {
        ("tour-hello", Spot.Nothing, LumaRig.Mood.Wave),
        ("tour-device", Spot.Device, LumaRig.Mood.Talk),
        ("tour-casing", Spot.Casing, LumaRig.Mood.Talk),
        ("tour-claude", Spot.Chat, LumaRig.Mood.Talk),
        ("tour-manuals", Spot.Manuals, LumaRig.Mood.Talk),
        ("tour-clock", Spot.Hud, LumaRig.Mood.Concern),
        ("tour-controls", Spot.Arrows, LumaRig.Mood.Talk),
        ("tour-go", Spot.Nothing, LumaRig.Mood.Cheer),
    };

    static readonly Color Dim = Ui.Ink.WithAlpha(0.94f); // blending is in linear space: this reads as about 80% dark
    const float Pad = 14f, Blur = 30f;
    static readonly Vector2 CardSize = new Vector2(660f, 240f), HeroBig = new Vector2(330f, 375f), HeroSmall = new Vector2(150f, 170f);

    BombCameraController cam;
    RectTransform root, card, heroSlot, hero, tags;
    CanvasGroup group, cardGroup;
    Image hole, ring, heroGlow;
    RawImage heroImage;
    CanvasGroup portrait;
    readonly Image[] dims = new Image[4];
    readonly List<Image> stepDots = new List<Image>();
    readonly List<(RectTransform rt, float phase, float speed, float radius)> sparkles = new List<(RectTransform, float, float, float)>();
    TMP_Text cardText, cardCount;

    Func<Rect?> target;  // where the beam points now, in screen pixels; null: nowhere
    Rect beam;            // the light's rectangle on the canvas, easing toward the target
    bool lightOn, heroIsBig, heroFree, advance, skipped;
    float lit;            // how lit the ring around the beam is (0 to 1)
    float darkness = 1f;  // the finale lifts the dark
    Coroutine typing, extra;

    void Awake() => Instance = this;

    // From the help screen: the same tour, while the clock is held.
    public void Replay()
    {
        if (!Running && GameManager.Instance.State == GameManager.RunState.Playing) StartCoroutine(Run(firstTime: false));
    }

    public IEnumerator Run(bool firstTime)
    {
        Running = true;
        advance = skipped = false;
        if (root == null) Build();
        root.gameObject.SetActive(true);
        var gm = GameManager.Instance;
        var luma = LoomaCoach.Instance;
        gm.HoldClock(this);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null); // keys belong to the tour now
        cam = FindFirstObjectByType<BombCameraController>();
        cam.Scripted = true;
        cam.Orbit(0f);
        cam.GlideHome(1.4f);
        if (luma != null) heroImage.texture = luma.View;
        darkness = 1f;

        heroIsBig = true;
        heroFree = false;
        lightOn = false;
        group.alpha = 0f;
        cardGroup.alpha = 1f;
        yield return Ui.Fade(group, 1f, 0.5f);

        for (int i = 0; i < Steps.Length && !skipped; i++) yield return Step(i);
        if (luma != null) luma.Hush();

        if (firstTime && !skipped) yield return Finale();
        else
        {
            cam.GlideHome(0.8f);
            yield return Ui.Fade(group, 0f, 0.4f);
        }

        root.gameObject.SetActive(false);
        ClearTags();
        cam.Scripted = false;
        gm.ReleaseClock(this);
        if (skipped && firstTime && luma != null) luma.Say("tour-skipped");
        Running = false;
    }

    void Update()
    {
        if (!Running || root == null) return;
        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame) skipped = true;
        if (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame)) advance = true;

        float ease = 1f - Mathf.Exp(-7f * Time.unscaledDeltaTime);
        Rect area = root.rect;

        // The light eases toward its target and the room stays dark around it. With
        // nothing to show it closes like an iris, into the middle of the screen.
        Rect? screen = lightOn && target != null ? target() : null;
        Rect goal = screen.HasValue ? Grow(Ui.ToCanvas(root, screen.Value), Pad) : new Rect(0f, 0f, 0f, 0f);
        beam = Rect.MinMaxRect(
            Mathf.Lerp(beam.xMin, goal.xMin, ease), Mathf.Lerp(beam.yMin, goal.yMin, ease),
            Mathf.Lerp(beam.xMax, goal.xMax, ease), Mathf.Lerp(beam.yMax, goal.yMax, ease));
        lit = Mathf.MoveTowards(lit, screen.HasValue ? 1f : 0f, Time.unscaledDeltaTime * 2.5f);
        hole.color = Dim.WithAlpha(Dim.a * darkness);
        foreach (var d in dims) d.color = hole.color;
        Rect h = Grow(beam, Blur);
        Fit(hole.rectTransform, h);
        Fit(dims[0].rectTransform, Rect.MinMaxRect(area.xMin, h.yMax, area.xMax, Mathf.Max(h.yMax, area.yMax)));
        Fit(dims[1].rectTransform, Rect.MinMaxRect(area.xMin, Mathf.Min(h.yMin, area.yMin), area.xMax, h.yMin));
        Fit(dims[2].rectTransform, Rect.MinMaxRect(Mathf.Min(h.xMin, area.xMin), h.yMin, h.xMin, h.yMax));
        Fit(dims[3].rectTransform, Rect.MinMaxRect(h.xMax, h.yMin, Mathf.Max(h.xMax, area.xMax), h.yMax));
        Fit(ring.rectTransform, Grow(beam, 2f));
        float pulse = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 3.2f);
        ring.color = Ui.Teal.WithAlpha(0.85f * lit * darkness * pulse);

        // LUMA's card goes beside the beam, on the side with the most room.
        Vector2 cardGoal = screen.HasValue ? Beside(beam, area) : new Vector2(0f, -190f);
        card.anchoredPosition = Vector2.Lerp(card.anchoredPosition, cardGoal, ease);

        // LUMA herself: big in the middle when nothing is lit, otherwise in her card.
        if (!heroFree)
        {
            Vector2 heroGoal = heroIsBig ? new Vector2(0f, 120f) : card.anchoredPosition + heroSlot.anchoredPosition;
            hero.anchoredPosition = Vector2.Lerp(hero.anchoredPosition, heroGoal, ease);
            hero.sizeDelta = Vector2.Lerp(hero.sizeDelta, heroIsBig ? HeroBig : HeroSmall, ease);
        }
        heroGlow.rectTransform.sizeDelta = hero.sizeDelta * 1.9f;
        portrait.alpha = Mathf.MoveTowards(portrait.alpha, heroIsBig ? 0f : 1f, Time.unscaledDeltaTime * 3f); // an empty frame while she's away

        // Sparkles drift around LUMA while she's big.
        float sparkleAlpha = heroIsBig || heroFree ? 1f : 0f;
        foreach (var (rt, phase, speed, radius) in sparkles)
        {
            float a = Time.unscaledTime * speed + phase;
            rt.anchoredPosition = new Vector2(Mathf.Cos(a), Mathf.Sin(a * 1.3f) * 0.8f) * radius * hero.sizeDelta.x / HeroBig.x;
            var img = rt.GetComponent<Image>();
            img.color = Ui.Cyan.WithAlpha(Mathf.MoveTowards(img.color.a, sparkleAlpha * (0.3f + 0.7f * Mathf.Abs(Mathf.Sin(a * 2.7f))), Time.unscaledDeltaTime * 3f));
        }
    }

    // ---- The steps ----

    IEnumerator Step(int i)
    {
        var (line, spot, mood) = Steps[i];
        var luma = LoomaCoach.Instance;
        advance = false;
        for (int d = 0; d < stepDots.Count; d++) stepDots[d].color = d == i ? Ui.Aqua : d < i ? Ui.Border3 : Ui.Border2;
        cardCount.text = $"{i + 1} / {Steps.Length}";
        heroIsBig = spot == Spot.Nothing;
        Point(spot);
        if (i > 0) SoundManager.Instance.PlayWhoosh();
        extra = spot == Spot.Device ? StartCoroutine(ShowPlates()) : spot == Spot.Casing ? StartCoroutine(ShowCasing()) : null;

        typing = StartCoroutine(Ui.Type(cardText, LoomaCoach.Lines[line], 60f, () => advance || skipped));
        bool spoken = false;
        if (luma != null) StartCoroutine(Then(luma.Speak(line, mood), () => spoken = true));
        else spoken = true;

        // On to the next step when the line is said (and a moment to read it), or on NEXT.
        float after = 0f;
        while (!advance && !skipped && after < 1.1f)
        {
            if (spoken) after += Time.unscaledDeltaTime;
            yield return null;
        }
        if (luma != null) luma.Hush();
        if (extra != null) StopCoroutine(extra);
        StopCoroutine(typing);
        cardText.maxVisibleCharacters = 99999;
        ClearTags();
    }

    static IEnumerator Then(IEnumerator routine, Action done)
    {
        yield return routine;
        done();
    }

    // Moves the beam and the camera to a step's subject.
    void Point(Spot spot)
    {
        lightOn = spot != Spot.Nothing;
        var casing = FindFirstObjectByType<CasingDisplay>();
        switch (spot)
        {
            case Spot.Device:
                cam.Glide(0f, 4f, 1.3f, null, 1.2f); // a step back, so LUMA's card fits below
                target = () => casing != null ? OnScreen(Bounds(casing.gameObject)) : (Rect?)null;
                break;
            case Spot.Casing:
                target = () => casing != null ? OnScreen(casing.LampsAndCells()) : (Rect?)null;
                break;
            case Spot.Chat:
                cam.GlideHome(1f);
                var chat = FindFirstObjectByType<ChatPanel>();
                target = () => chat != null ? Ui.ScreenRect(chat.Area) : (Rect?)null;
                break;
            case Spot.Manuals:
                target = () => PaperDesk.Instance != null && PaperDesk.Instance.ManualsButton != null
                    ? Ui.ScreenRect(PaperDesk.Instance.ManualsButton) : (Rect?)null;
                break;
            case Spot.Hud:
                target = () => Hud.Instance != null ? Ui.ScreenRect(Hud.Instance.Bar) : (Rect?)null;
                break;
            case Spot.Arrows:
                var arrows = FindFirstObjectByType<ViewButtons>();
                target = () => arrows != null && arrows.Cluster != null ? Ui.ScreenRect(arrows.Cluster) : (Rect?)null;
                break;
            default:
                target = null;
                break;
        }
    }

    // The device's numbered plates, named one after another with a rising chime.
    IEnumerator ShowPlates()
    {
        yield return new WaitForSecondsRealtime(1.2f);
        int n = 0;
        foreach (var plate in Plates.All)
        {
            var module = ModuleObject(plate.id);
            if (module == null) continue;
            var tag = Tag(plate.number.ToString(), plate.name);
            var bounds = Bounds(module.gameObject);
            StartCoroutine(Follow(tag, () => new Vector3(bounds.center.x, bounds.max.y, bounds.center.z)));
            StartCoroutine(Ui.Animate(0.45f, k => tag.localScale = Vector3.one * k, Ui.EaseOutBack));
            SoundManager.Instance.PlayChime(n++);
            yield return new WaitForSecondsRealtime(0.45f);
        }
    }

    // Round to the back, then underneath: the casing's lamps and cells, then its stamp.
    IEnumerator ShowCasing()
    {
        var casing = FindFirstObjectByType<CasingDisplay>();
        if (casing == null) yield break;
        var center = cam.Target.position;
        cam.GlideFrom(casing.LampsAndCells().center - center, 1.15f, 1.6f);
        yield return new WaitForSecondsRealtime(3.4f);
        var stamp = casing.StampArea();
        if ((stamp.center - casing.LampsAndCells().center).magnitude < 0.05f) yield break; // all on one side
        // From below, and a little from the front, so the device doesn't look upside down.
        cam.GlideFrom((stamp.center - center).normalized + Vector3.ProjectOnPlane(-casing.LampsAndCells().center + center, Vector3.up).normalized * 0.5f, 1.05f, 1.4f);
        target = () => OnScreen(stamp);
    }

    // The end of the first tour: a countdown, and LUMA flies into the chat.
    IEnumerator Finale()
    {
        StartCoroutine(Ui.Fade(cardGroup, 0f, 0.3f));
        cam.GlideHome(1.2f);
        var number = Ui.NewText("Count", root, "", 230f, Color.white, TextAlignmentOptions.Center, Ui.Display);
        number.textWrappingMode = TextWrappingModes.NoWrap;
        Ui.Place(number.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -250f), new Vector2(1600f, 300f)); // below LUMA
        heroIsBig = false;
        heroFree = true;
        Vector2 heroFrom = hero.anchoredPosition, sizeFrom = hero.sizeDelta;
        Vector2 heroTo = heroFrom, sizeTo = sizeFrom;
        var slot = LoomaCoach.Instance != null ? LoomaCoach.Instance.ViewSlot : null;
        if (slot != null)
        {
            Rect to = Ui.ToCanvas(root, Ui.ScreenRect(slot));
            heroTo = to.center;
            sizeTo = to.size;
        }

        for (int n = 3; n >= 1; n--)
        {
            number.text = n.ToString();
            SoundManager.Instance.PlayBeep();
            yield return Ui.Animate(0.8f, k =>
            {
                number.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.4f, 0.9f, k);
                number.color = Color.white.WithAlpha(1f - k * k);
            });
        }

        number.text = "Go";
        number.fontSize = 170f;
        SoundManager.Instance.PlayBeep(last: true);
        StartCoroutine(Ui.Animate(1.1f, k =>
        {
            number.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.8f, 1.15f, k);
            number.color = Ui.Teal.WithAlpha(1f - k);
        }));
        // LUMA takes her seat in the chat as the lights come up.
        yield return Ui.Animate(1.1f, k =>
        {
            hero.anchoredPosition = Vector2.Lerp(heroFrom, heroTo, k);
            hero.sizeDelta = Vector2.Lerp(sizeFrom, sizeTo, k);
            darkness = 1f - k;
        }, Ui.EaseInOut);
        yield return Ui.Fade(group, 0f, 0.25f);

        Destroy(number.gameObject);
        heroFree = false;
    }

    // ---- Where things are ----

    static Component ModuleObject(ModuleId id) => id switch
    {
        ModuleId.Wires => FindFirstObjectByType<ModuleA_Wires>(),
        ModuleId.Glyphs => FindFirstObjectByType<ModuleB_Glyphs>(),
        ModuleId.Sequence => FindFirstObjectByType<ModuleC_Sequence>(),
        ModuleId.Grid => FindFirstObjectByType<ModuleD_Grid>(),
        _ => FindFirstObjectByType<ModuleE_Switches>(),
    };

    static Bounds Bounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>().Where(r => !(r is ParticleSystemRenderer)).ToArray();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one * 0.1f);
        var b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        return b;
    }

    // A box in the world, as the rectangle it covers on screen.
    static Rect? OnScreen(Bounds b)
    {
        var camera = Camera.main;
        if (camera == null) return null;
        float xMin = float.MaxValue, yMin = float.MaxValue, xMax = float.MinValue, yMax = float.MinValue;
        for (int i = 0; i < 8; i++)
        {
            var corner = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
            var p = camera.WorldToScreenPoint(corner);
            if (p.z < 0f) return null;
            xMin = Mathf.Min(xMin, p.x); yMin = Mathf.Min(yMin, p.y);
            xMax = Mathf.Max(xMax, p.x); yMax = Mathf.Max(yMax, p.y);
        }
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    // Where LUMA's card goes: beside the beam, on the side with the most room.
    Vector2 Beside(Rect lit, Rect area)
    {
        const float gap = 44f, margin = 24f;
        Vector2 half = CardSize / 2f;
        float right = area.xMax - lit.xMax, left = lit.xMin - area.xMin, above = area.yMax - lit.yMax, below = lit.yMin - area.yMin;
        Vector2 p;
        if (Mathf.Max(right, left) >= CardSize.x + gap + margin)
            p = new Vector2(right >= left ? lit.xMax + gap + half.x : lit.xMin - gap - half.x, lit.center.y);
        else if (above >= CardSize.y + gap + margin && above > below)
            p = new Vector2(lit.center.x, lit.yMax + gap + half.y);
        else // below, even if it overlaps a little: the top of the screen holds the HUD
            p = new Vector2(lit.center.x, lit.yMin - gap - half.y);
        p.x = Mathf.Clamp(p.x, area.xMin + margin + half.x, area.xMax - margin - half.x);
        p.y = Mathf.Clamp(p.y, area.yMin + margin + half.y, area.yMax - margin - half.y);
        return p;
    }

    static Rect Grow(Rect r, float by) => Rect.MinMaxRect(r.xMin - by, r.yMin - by, r.xMax + by, r.yMax + by);

    static void Fit(RectTransform rt, Rect r)
    {
        rt.anchoredPosition = r.center;
        rt.sizeDelta = new Vector2(Mathf.Max(0f, r.width), Mathf.Max(0f, r.height));
    }

    // ---- Plate tags ----

    RectTransform Tag(string number, string name)
    {
        var tag = Ui.NewCard("Tag " + number, tags, Color.white, out var background, 4);
        var digit = Ui.NewHeading("Number", tag, number, 17f, Ui.TealDeep, TextAlignmentOptions.MidlineLeft);
        Ui.Stretch(digit.rectTransform, new Vector2(14f, 0f), Vector2.zero);
        var label = Ui.NewHeading("Name", tag, name, 16f, Ui.Ink, TextAlignmentOptions.MidlineLeft);
        float width = label.GetPreferredValues().x + 54f;
        tag.anchorMin = tag.anchorMax = new Vector2(0.5f, 0.5f);
        tag.pivot = new Vector2(0.5f, 0f);
        tag.sizeDelta = new Vector2(width, 40f);
        Ui.Stretch(label.rectTransform, new Vector2(36f, 0f), new Vector2(-14f, 0f));
        tag.localScale = Vector3.zero;
        return tag;
    }

    IEnumerator Follow(RectTransform tag, Func<Vector3> world)
    {
        while (tag != null)
        {
            var p = Camera.main.WorldToScreenPoint(world());
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, p, null, out var local);
            tag.anchoredPosition = local + new Vector2(0f, 10f);
            yield return null;
        }
    }

    void ClearTags()
    {
        if (tags == null) return;
        foreach (Transform t in tags) Destroy(t.gameObject);
    }

    // ---- UI construction ----

    void Build()
    {
        var canvas = Ui.NewCanvas("TourCanvas", transform, 35); // over the chat, the HUD and the help; under the title (40)
        root = (RectTransform)canvas.transform;
        group = Ui.Group(canvas);

        // Catches every click: during the tour, nothing behind it reacts.
        var blocker = Ui.NewImage("Blocker", root, Color.clear);
        Ui.Stretch(blocker.rectTransform);

        hole = Ui.NewImage("Light", root, Dim, Ui.Hole(18, (int)Blur));
        hole.type = Image.Type.Sliced;
        hole.pixelsPerUnitMultiplier = 2f;
        hole.raycastTarget = false;
        Centre(hole.rectTransform);
        for (int i = 0; i < 4; i++)
        {
            dims[i] = Ui.NewImage("Dark", root, Dim);
            dims[i].raycastTarget = false;
            Centre(dims[i].rectTransform);
        }
        ring = Ui.NewImage("Ring", root, Ui.Teal.WithAlpha(0f));
        Ui.Round(ring, 18, outlineOnly: true);
        ring.raycastTarget = false;
        Centre(ring.rectTransform);

        tags = Ui.NewRect("Tags", root);
        Ui.Stretch(tags);

        // LUMA's card: her portrait on the left, what she says, the steps and the buttons.
        card = Ui.NewCard("Card", root, Color.white, out var background, 16);
        Ui.Place(card, new Vector2(0.5f, 0.5f), new Vector2(0f, -190f), CardSize);
        cardGroup = Ui.Group(card);
        var frame = Ui.NewPanel("Portrait", card, Ui.Surface2, 10);
        Ui.Place(frame.rectTransform, new Vector2(0f, 0.5f), new Vector2(22f, 0f), HeroSmall + new Vector2(10f, 10f));
        portrait = Ui.Group(frame);
        heroSlot = Ui.NewRect("HeroSlot", card);
        heroSlot.anchorMin = heroSlot.anchorMax = heroSlot.pivot = new Vector2(0.5f, 0.5f);
        heroSlot.anchoredPosition = new Vector2(-CardSize.x / 2f + 22f + (HeroSmall.x + 10f) / 2f, 0f);

        var name = Ui.NewHeading("Name", card, "LUMA", 20f, Ui.TealDeep);
        Ui.Place(name.rectTransform, new Vector2(0f, 1f), new Vector2(200f, -22f), new Vector2(200f, 26f));
        cardCount = Ui.NewLabel("Count", card, "", 12f, Ui.Text2, TextAlignmentOptions.TopRight);
        Ui.Place(cardCount.rectTransform, new Vector2(1f, 1f), new Vector2(-28f, -27f), new Vector2(120f, 20f));
        cardText = Ui.NewText("Text", card, "", 23f, Ui.Ink);
        cardText.lineSpacing = 6f;
        cardText.enableAutoSizing = true;
        cardText.fontSizeMin = 18f;
        cardText.fontSizeMax = 23f;
        Ui.Stretch(cardText.rectTransform, new Vector2(200f, 72f), new Vector2(-28f, -58f));

        var dotsRow = Ui.NewRect("Steps", card);
        Ui.Place(dotsRow, new Vector2(0f, 0f), new Vector2(200f, 30f), new Vector2(Steps.Length * 18f, 12f));
        for (int i = 0; i < Steps.Length; i++)
        {
            var dot = Ui.NewIcon("Step", dotsRow, Ui.Circle, Ui.Border2, 9f);
            Ui.Place(dot.rectTransform, new Vector2(0f, 0.5f), new Vector2(i * 18f, 0f), new Vector2(9f, 9f));
            stepDots.Add(dot);
        }
        var next = Ui.NewButton("Next", card, "Next", Ui.ButtonStyle.Primary, () => advance = true, 17f);
        Ui.Place((RectTransform)next.transform, new Vector2(1f, 0f), new Vector2(-24f, 20f), new Vector2(124f, 46f));
        var skip = Ui.NewButton("Skip", card, "Skip tour", Ui.ButtonStyle.Ghost, () => skipped = true, 14f);
        Ui.Place((RectTransform)skip.transform, new Vector2(1f, 0f), new Vector2(-160f, 20f), new Vector2(150f, 46f));

        // LUMA herself, over everything: big at the start, then in her card - a soft
        // glow behind her and sparkles around her.
        hero = Ui.NewRect("Luma", root);
        Centre(hero);
        hero.sizeDelta = HeroBig;
        hero.anchoredPosition = new Vector2(0f, 120f);
        heroGlow = Ui.NewGlow("Glow", hero, Ui.Teal.WithAlpha(0.3f), HeroBig * 1.9f);
        heroGlow.rectTransform.anchoredPosition = new Vector2(0f, -10f);
        heroImage = Ui.NewRect("View", hero).gameObject.AddComponent<RawImage>();
        heroImage.raycastTarget = false;
        Ui.Stretch(heroImage.rectTransform);
        for (int i = 0; i < 18; i++)
        {
            var sparkle = Ui.NewGlow("Sparkle", hero, Ui.Cyan.WithAlpha(0f), Vector2.one * UnityEngine.Random.Range(8f, 20f));
            sparkles.Add((sparkle.rectTransform, UnityEngine.Random.value * 10f, UnityEngine.Random.Range(0.25f, 0.6f), UnityEngine.Random.Range(150f, 260f)));
        }
    }

    static void Centre(RectTransform rt) => rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
}
