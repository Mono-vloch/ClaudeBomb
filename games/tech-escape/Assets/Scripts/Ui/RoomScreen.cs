using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Inside a room: the room itself fills the screen, seen from inside at shoulder height
// like a security camera's feed (corners, REC, the camera and the room, the clock), in
// lockdown, its lamps on and a board of lamps on its wall saying which rows are decided;
// and in a dark drawer on the right (a third of the screen at most, and it folds away)
// the system as it was handed over: the level, the rows (assets, accounts, messages,
// rules, steps) and for each the decisions to make, as chips that cycle through their
// options. Rows that must be ordered have arrows. A record that must be signed has a
// signature line. Then: run the attack.
public class RoomScreen
{
    public readonly RectTransform root;
    readonly App app;
    readonly Room room;
    readonly RoomState st;
    readonly RectTransform frame, drawer, tab;
    readonly TextMeshProUGUI note;
    readonly int index;
    RectTransform content, tabLamps;
    bool locked, open = true;

    const float DrawerW = 640f;          // 33 % of 1920: the world keeps two thirds and more
    const float TabW = 72f;
    static readonly Color Line = new Color(1f, 1f, 1f, 0.14f), RowFill = new Color(0.08f, 0.14f, 0.27f, 0.92f);

    public RoomScreen(Transform parent, App app)
    {
        this.app = app;
        room = app.room;
        st = app.state;
        locked = false;
        root = Ui.Node("Room", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        frame = Ui.Node("Frame", root);
        Ui.Place(frame, 0, 0, Ui.W - DrawerW, Ui.H);
        app.world.Build();
        index = Facility.RoomIndex(room.room);
        for (int i = 0; i < 12; i++) app.world.SetDoor(i, i == index ? Building.DoorState.Open : i < app.LevelsDone ? Building.DoorState.Held : Building.DoorState.Locked);
        app.world.ShowRoom(frame, index);
        Sfx.Ambient(true);
        if (app.world.HasInterior(index))
            CameraFeed.Build(frame, $"CAM {index + 1:00}  /  {room.room.ToUpperInvariant()}", "PERMIT OFFICE  /  LOCKDOWN");

        // The room as it was handed over: LUMA says what is wrong with it, once, over the
        // room, and her line fades; on a room with nothing planned, her line for the level.
        app.luma.Line(root, 72, 952, 760, string.IsNullOrEmpty(room.mistake) ? app.luma.Say("live-" + room.number) : "Fix it: " + room.mistake + ". " + room.why, true, false, 16);

        // The drawer: the level at its head, the buttons, then the system.
        drawer = Ui.Node("Drawer", root);
        Ui.Place(drawer, Ui.W - DrawerW, 0, DrawerW, Ui.H);
        var bg = Ui.Box(drawer, "Glass", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.94f), false);
        Ui.Stretch(bg.rectTransform);
        var keyline = Ui.Box(drawer, "Keyline", Theme.Teal, false);
        Ui.Place(keyline.rectTransform, 0, 0, 3, Ui.H);
        var eyebrow = Ui.Eyebrow(drawer, $"LEVEL {room.id}  ·  {room.room.ToUpperInvariant()}", Theme.Teal);
        eyebrow.fontSize = 16;
        Ui.Place(eyebrow.rectTransform, 28, 26, 500, 22);
        var fold = Ui.Button(drawer, "Hide", () => Fold(false), Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(fold.GetComponent<RectTransform>(), DrawerW - 112, 18, 88, 40);
        var title = Ui.Text(drawer, room.title, 28, Color.white, true);
        Ui.Place(title.rectTransform, 28, 62, DrawerW - 56, 76);
        // The step note and the "You fix" paragraph left the drawer's head on 7 November 2026:
        // LUMA says the mistake over the room and the file holds the paragraph, and the room's
        // rows need the height (Firewall Control's allow-all rule, the planned mistake, sat
        // below the drawer's fold).
        var back = Ui.Button(drawer, "Briefing", app.ShowLesson, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(back.GetComponent<RectTransform>(), 28, 150, 132, 52);
        var file = Ui.Button(drawer, "The file", () => app.ShowFile(), Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(file.GetComponent<RectTransform>(), 170, 150, 132, 52);
        var run = Ui.Button(drawer, "RUN THE ATTACK", () => { if (Ready()) app.RunAttack(); }, Ui.ButtonStyle.Accent, 18);
        Ui.Place(run.GetComponent<RectTransform>(), 312, 150, DrawerW - 340, 52);
        note = Ui.Text(drawer, "", 16, Look.Chalk); // only speaks up when the run cannot start yet
        Ui.Place(note.rectTransform, 28, 206, DrawerW - 56, 24);
        float top = 232;
        var rule = Ui.Box(drawer, "Rule", Line, false);
        Ui.Place(rule.rectTransform, 28, top, DrawerW - 56, 1);
        var head = Ui.Eyebrow(drawer, room.ordered ? "THE SYSTEM  ·  ORDER AND DECIDE" : "THE SYSTEM  ·  DECIDE EACH ROW", Theme.Teal);
        head.fontSize = 16; head.font = Theme.Bold;
        Ui.Place(head.rectTransform, 28, top + 16, DrawerW - 56, 22);
        var scroll = Ui.Scroll(drawer, "Scroll", out content);
        Ui.Place(scroll.GetComponent<RectTransform>(), 20, top + 50, DrawerW - 40, Ui.H - top - 70);

        // The folded drawer: a slim tab with the rows' lamps and a way back.
        tab = Ui.Node("Tab", root);
        Ui.Place(tab, Ui.W - TabW, 0, TabW, Ui.H);
        var tabBg = Ui.Box(tab, "Glass", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.94f), false);
        Ui.Stretch(tabBg.rectTransform);
        var unfold = Ui.Button(tab, "Open", () => Fold(true), Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(unfold.GetComponent<RectTransform>(), 6, 18, TabW - 12, 44);
        tabLamps = Ui.Node("Lamps", tab);
        Ui.Place(tabLamps, 0, 80, TabW, 600);
        tab.gameObject.SetActive(false);
        Build();
    }

    // Fold the drawer away (the world takes the whole screen) or bring it back.
    void Fold(bool show)
    {
        open = show;
        drawer.gameObject.SetActive(show);
        tab.gameObject.SetActive(!show);
        Ui.Place(frame, 0, 0, Ui.W - (show ? DrawerW : TabW), Ui.H);
    }

    bool Ready()
    {
        if (room.freeText && string.IsNullOrWhiteSpace(st.text)) { note.text = "Write your line first."; note.color = Theme.Danger; if (!open) Fold(true); return false; }
        return true;
    }

    void Build()
    {
        Ui.Clear(content);
        const float W = DrawerW - 52;
        float y = 0;
        var perRow = room.fields.Where(f => !f.once).ToList();
        var once = room.fields.Where(f => f.once).ToList();
        var rows = st.order.Where(k => room.items.Any(i => i.key == k)).ToList();
        foreach (var added in st.added) if (!rows.Contains(added)) rows.Add(added);
        var decided = new List<bool>();
        for (int r = 0; r < rows.Count; r++)
        {
            var item = room.items.First(i => i.key == rows[r]);
            decided.Add(perRow.All(f => { int p = st.Pick(item.key, f.key); return p >= 0 && p < f.options.Length && f.options[p] != "?"; }));
            float x0 = room.ordered ? 58 : 18;
            var probe = Ui.Text(content, item.detail, 16, Theme.Faint);
            float detailH = string.IsNullOrEmpty(item.detail) ? 0f : probe.GetPreferredValues(item.detail, W - x0 - 18, 0).y;
            probe.fontSize = 18; probe.font = Theme.Bold;
            float nameH = Mathf.Max(26f, probe.GetPreferredValues(item.name ?? "", W - x0 - 18, 0).y);
            Object.Destroy(probe.gameObject);
            // Two decisions on a row sit side by side (half the width each), so five rules fit
            // the drawer; more than two stack as before.
            bool twoUp = perRow.Count == 2;
            float chipW = twoUp ? (W - x0 - 14 - 8) / 2f : W - x0 - 14;
            var chipH = perRow.Select(f => ChipHeight(f, item.key, chipW)).ToList();
            float chipBlock = twoUp ? chipH.Max() + 8 : chipH.Sum(c => c + 8);
            float h = 12 + nameH + 4 + detailH + (detailH > 0 ? 10 : 4) + chipBlock + 6;
            var row = Ui.Box(content, item.key, RowFill);
            Ui.Place(row.rectTransform, 0, y, W, h);
            var rt = row.transform;
            var keyline = Ui.Box(rt, "Keyline", decided[r] ? Theme.Teal : Theme.Amber, false);
            Ui.Place(keyline.rectTransform, 0, 10, 4, h - 20);
            keyline.raycastTarget = false;
            if (room.ordered)
            {
                var num = Ui.Text(rt, (r + 1).ToString(), 24, Theme.Teal, true, TextAlignmentOptions.Center);
                Ui.Place(num.rectTransform, 10, 12, 38, 30);
                if (!locked)
                {
                    int at = r;
                    var up = Ui.Button(rt, "▲", () => Move(at, -1), Ui.ButtonStyle.GhostOnDark, 16); Ui.Place(up.GetComponent<RectTransform>(), 8, 46, 42, 32);
                    var down = Ui.Button(rt, "▼", () => Move(at, 1), Ui.ButtonStyle.GhostOnDark, 16); Ui.Place(down.GetComponent<RectTransform>(), 8, 82, 42, 32);
                }
            }
            var name = Ui.Text(rt, item.name, 18, Color.white, true);
            Ui.Place(name.rectTransform, x0, 12, W - x0 - 18, nameH);
            var detail = Ui.Text(rt, item.detail, 16, Theme.Faint);
            Ui.Place(detail.rectTransform, x0, 16 + nameH, W - x0 - 18, detailH);
            float fy = 16 + nameH + detailH + (detailH > 0 ? 10 : 4);
            if (twoUp) { float hh = chipH.Max(); for (int k = 0; k < 2; k++) Chip(rt, x0 + k * (chipW + 8), fy, chipW, perRow[k], item.key, hh); }
            else for (int k = 0; k < perRow.Count; k++) { Chip(rt, x0, fy, W - x0 - 14, perRow[k], item.key, chipH[k]); fy += chipH[k] + 8; }
            y += h + 12;
        }
        app.world.ShowConfig(index, decided);
        TabLamps(decided);
        // Things not listed yet, to add (the asset nobody listed).
        var hidden = room.items.Where(i => i.hidden && !st.added.Contains(i.key)).ToList();
        if (hidden.Count > 0 && !locked)
        {
            var addLabel = Ui.Eyebrow(content, "SOMETHING NOT ON THE LIST?  ·  ADD IT", Theme.Faint);
            addLabel.fontSize = 16;
            Ui.Place(addLabel.rectTransform, 0, y + 6, W, 22);
            y += 36;
            foreach (var h in hidden)
            {
                string key = h.key;
                var b = Ui.Button(content, "+ " + h.name, () => { st.added.Add(key); st.order.Add(key); Sfx.Chime(); Build(); }, Ui.ButtonStyle.GhostOnDark, 16);
                Ui.Place(b.GetComponent<RectTransform>(), 0, y, W, 44);
                y += 52;
            }
        }
        // The room's one-off decisions.
        if (once.Count > 0)
        {
            var oh = Ui.Eyebrow(content, "FOR THE WHOLE ROOM", Theme.Teal);
            oh.fontSize = 16;
            Ui.Place(oh.rectTransform, 0, y + 8, W, 22);
            y += 38;
            foreach (var f in once) { float ch = ChipHeight(f, "room", W); Chip(content, 0, y, W, f, "room", ch); y += ch + 10; }
        }
        if (room.signable)
        {
            var sh = Ui.Eyebrow(content, "THE RECORD", Theme.Teal);
            sh.fontSize = 16;
            Ui.Place(sh.rectTransform, 0, y + 8, W, 22);
            var sig = Ui.Text(content, st.signed ? $"Signed by {st.signedBy} on {st.signedOn}." : "Not signed. A review that is not written down, dated and signed is not done.", 16, st.signed ? Theme.Teal : Look.Chalk);
            Ui.Place(sig.rectTransform, 0, y + 36, W, 48);
            y += 88;
            if (!locked)
            {
                var sign = Ui.Button(content, st.signed ? "Signed" : "Sign and date the record", () => { st.signed = true; st.signedBy = "You, as the reviewer"; st.signedOn = System.DateTime.Now.ToString("d MMM yyyy"); Sfx.Buy(); Build(); }, st.signed ? Ui.ButtonStyle.GhostOnDark : Ui.ButtonStyle.Primary, 16);
                Ui.Place(sign.GetComponent<RectTransform>(), 0, y, W, 48);
                y += 58;
            }
        }
        content.sizeDelta = new Vector2(0, y + 10);
    }

    // The folded drawer's column of lamps, one per row.
    void TabLamps(List<bool> decided)
    {
        if (tabLamps == null) return;
        Ui.Clear(tabLamps);
        for (int i = 0; i < decided.Count; i++)
        {
            var l = Ui.Box(tabLamps, "Lamp", decided[i] ? Theme.Aqua : Theme.Amber, false);
            Ui.Place(l.rectTransform, TabW / 2 - 9, i * 30, 18, 18);
            l.raycastTarget = false;
        }
    }

    // A decision as a chip on the dark: its label, then the choice; click it, it cycles through its options.
    // The label takes what it needs (up to 150), so a half-width chip keeps room for its choice.
    float LabelWidth(Field f, float w)
    {
        var probe = Ui.Text(content, f.label, 16, Theme.Faint, true);
        float lw = probe.GetPreferredValues(f.label, 0, 0).x + 24;
        Object.Destroy(probe.gameObject);
        return Mathf.Clamp(lw, 60f, Mathf.Min(150f, w * 0.45f));
    }

    // A chip is as high as its choice needs: one line, two, or three.
    float ChipHeight(Field f, string itemKey, float w)
    {
        int current = st.Pick(itemKey, f.key);
        string text = current >= 0 && current < f.options.Length ? f.options[current] : "?";
        var probe = Ui.Text(content, text, 16, Color.white, true);
        float th = probe.GetPreferredValues(text, w - LabelWidth(f, w) - 28, 0).y;
        Object.Destroy(probe.gameObject);
        return th <= 24f ? 40f : th <= 46f ? 62f : 84f;
    }

    void Chip(Transform t, float x, float y, float w, Field f, string itemKey, float height = 40f)
    {
        int current = st.Pick(itemKey, f.key);
        string text = current >= 0 && current < f.options.Length ? f.options[current] : "?";
        bool unset = current < 0 || f.options[current] == "?";
        float labelW = LabelWidth(f, w);
        var label = Ui.Text(t, f.label, 16, Theme.Faint, true);
        Ui.Place(label.rectTransform, x, y + 9, labelW - 8, 24);
        label.enableWordWrapping = false; label.overflowMode = TextOverflowModes.Ellipsis;
        var b = Ui.Button(t, text, () =>
        {
            if (locked) return;
            Sfx.Tick();
            int next = (st.Pick(itemKey, f.key) + 1) % f.options.Length;
            if (f.options[next] == "?" && f.options.Length > 1) next = (next + 1) % f.options.Length;
            st.Set(itemKey, f.key, next);
            Build();
        }, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(b.GetComponent<RectTransform>(), x + labelW, y, w - labelW, height);
        // Filled, no outline: a dark key when it is still open, the brand's deep blue once chosen.
        var fill = unset ? new Color(0.03f, 0.06f, 0.14f, 1f) : Theme.Ink2;
        b.GetComponent<Image>().color = fill;
        b.transform.Find("Fill").GetComponent<Image>().color = fill;
        var tl = b.GetComponentInChildren<TextMeshProUGUI>();
        tl.color = unset ? Theme.Faint : Theme.Teal;
        tl.alignment = TextAlignmentOptions.MidlineLeft;
        tl.enableWordWrapping = height > 40f; tl.overflowMode = TextOverflowModes.Ellipsis;
        Ui.Stretch(tl.rectTransform, 14, 4, 14, 4);
    }

    void Move(int at, int by)
    {
        int to = at + by;
        if (to < 0 || to >= st.order.Count) return;
        (st.order[at], st.order[to]) = (st.order[to], st.order[at]);
        Sfx.Tick();
        Build();
    }
}
