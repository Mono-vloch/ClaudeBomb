using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Inside a room: the room itself in three dimensions on the left, its door open,
// and on the right the system as it was handed over: the rows (assets, accounts,
// messages, rules, steps) and for each the decisions to make, as chips that
// cycle through their options. Rows that must be ordered have arrows. A record
// that must be signed has a signature line. Then: run the attack.
public class RoomScreen
{
    public readonly RectTransform root;
    readonly App app;
    readonly Room room;
    readonly RoomState st;
    readonly RectTransform frame, panel;
    readonly TextMeshProUGUI note, stepNote;
    RectTransform content;
    bool locked;

    public RoomScreen(Transform parent, App app)
    {
        this.app = app;
        room = app.room;
        st = app.state;
        locked = app.step == App.Step.SeeItWork;
        root = Ui.Node("Room", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        frame = Ui.Node("Frame", root);
        Ui.Place(frame, 60, 300, 660, 600);
        app.world.Build(false);
        int index = Facility.RoomIndex(room.room);
        for (int i = 0; i < 12; i++) app.world.SetDoor(i, i == index ? Building.DoorState.Open : i < app.LevelsDone ? Building.DoorState.Held : Building.DoorState.Locked);
        app.world.ShowRoom(frame, index);

        var top = Ui.Box(root, "Top", new Color(1, 1, 1, 0.86f), false);
        Ui.Place(top.rectTransform, -400, -300, 2800, 460);
        top.raycastTarget = false;
        var eyebrow = Ui.Eyebrow(root, $"LEVEL {room.id}  ·  {room.room.ToUpperInvariant()}", Theme.TealDeep);
        Ui.Place(eyebrow.rectTransform, 60, 24, 600, 20);
        var title = Ui.Text(root, room.title, 30, Theme.Ink, true);
        Ui.Place(title.rectTransform, 60, 46, 1000, 40);
        title.enableWordWrapping = false;
        var back = Ui.Button(root, "Briefing", app.ShowLesson, Ui.ButtonStyle.Quiet, 15);
        Ui.Place(back.GetComponent<RectTransform>(), 1080, 44, 100, 42);
        var run = Ui.Button(root, "RUN THE ATTACK", () => { if (Ready()) app.RunAttack(); }, Ui.ButtonStyle.Accent, 20);
        Ui.Place(run.GetComponent<RectTransform>(), 1600, 32, 260, 64);

        var labels = new List<string>(); var steps = new List<App.Step>();
        if (room.ready.Count > 0) { labels.Add("Watch the pro"); steps.Add(App.Step.SeeItWork); }
        if (room.broken.Count > 0) { labels.Add("Fix the broken room"); steps.Add(App.Step.FixIt); }
        labels.Add("Your run"); steps.Add(App.Step.YourTurn);
        if (labels.Count > 1)
        {
            var stepsLabel = Ui.Eyebrow(root, "THE RUNS", Theme.Muted);
            Ui.Place(stepsLabel.rectTransform, 60, 104, 100, 20);
            var seg = new Ui.Segmented(root, labels.ToArray(), steps.IndexOf(app.step), 600, 40, 14);
            Ui.Place(seg.root, 150, 100, 600, 40);
            seg.OnSelect = i => { app.SetStep(steps[i]); app.ShowRoom(); };
        }
        stepNote = Ui.Text(root, "", 13, Theme.Text2);
        Ui.Place(stepNote.rectTransform, 770, 100, 800, 44);
        stepNote.text = app.step == App.Step.SeeItWork ? "The room as it should be. Read it, then run the attack and watch it hold."
                      : app.step == App.Step.FixIt ? "Fix it: " + room.mistake + ". " + room.why
                      : "Your own decisions. This is the run that counts; it goes to the record.";

        var card = Ui.Soft(root, "Fixes");
        Ui.Place(card.rectTransform, 60, 176, 1800, 108);
        var tag = Ui.Eyebrow(card.transform, "YOU FIX", Theme.TealDeep);
        Ui.Place(tag.rectTransform, 24, 12, 900, 22);
        note = Ui.Text(card.transform, room.fixes, 18, Theme.Ink);
        Ui.Place(note.rectTransform, 24, 36, 1750, 64);

        var captionCard = Ui.Box(root, "Caption", new Color(1, 1, 1, 0.86f));
        Ui.Place(captionCard.rectTransform, 60, 910, 660, 150);
        captionCard.raycastTarget = false;
        var caption = Ui.Text(captionCard.transform, app.luma.Say("live-" + room.number), 14, Theme.Text2);
        Ui.Place(caption.rectTransform, 16, 12, 628, 126);

        panel = Ui.Node("Panel", root);
        Ui.Place(panel, 760, 300, 1100, 760);
        Build();
        Ui.Emblem(root);
    }

    bool Ready()
    {
        if (room.freeText && string.IsNullOrWhiteSpace(st.text)) { note.text = "Write your line first."; note.color = Theme.Danger; return false; }
        return true;
    }

    void Build()
    {
        Ui.Clear(panel);
        var card = Ui.Float(panel, "Card", 0, 0, 1100, 760);
        var t = card.transform;
        var head = Ui.Eyebrow(t, room.ordered ? "THE SYSTEM  ·  PUT THE ROWS IN ORDER, AND DECIDE EACH" : "THE SYSTEM  ·  DECIDE EACH ROW", Theme.TealDeep);
        head.font = Theme.Bold;
        Ui.Place(head.rectTransform, 24, 16, 800, 18);
        if (locked) { var l = Ui.Eyebrow(t, "READ ONLY  ·  THE READY ROOM", Theme.Muted); Ui.Place(l.rectTransform, 800, 16, 276, 18); l.alignment = TextAlignmentOptions.TopRight; }
        var scroll = Ui.Scroll(t, "Scroll", out content);
        Ui.Place(scroll.GetComponent<RectTransform>(), 16, 40, 1068, 704);

        float y = 0;
        var perRow = room.fields.Where(f => !f.once).ToList();
        var once = room.fields.Where(f => f.once).ToList();
        var rows = st.order.Where(k => room.items.Any(i => i.key == k)).ToList();
        foreach (var added in st.added) if (!rows.Contains(added)) rows.Add(added);
        for (int r = 0; r < rows.Count; r++)
        {
            var item = room.items.First(i => i.key == rows[r]);
            float h = 62 + Mathf.CeilToInt(perRow.Count / 2f) * 44 + (perRow.Any(f => Wide(f)) ? 44 * perRow.Count(Wide) : 0);
            if (perRow.Count == 0) h = 62;
            var row = Ui.Card(content, item.key, Theme.Panel, Theme.Border1);
            Ui.Place(row.rectTransform, 0, y, 1040, h);
            var rt = row.transform;
            if (room.ordered)
            {
                var num = Ui.Text(rt, (r + 1).ToString(), 22, Theme.Aqua, true, TextAlignmentOptions.Center);
                Ui.Place(num.rectTransform, 10, 12, 36, 30);
                if (!locked)
                {
                    int index = r;
                    var up = Ui.Button(rt, "▲", () => Move(index, -1), Ui.ButtonStyle.Quiet, 12); Ui.Place(up.GetComponent<RectTransform>(), 6, 42, 44, 24);
                    var down = Ui.Button(rt, "▼", () => Move(index, 1), Ui.ButtonStyle.Quiet, 12); Ui.Place(down.GetComponent<RectTransform>(), 6, 66, 44, 24);
                }
            }
            float x0 = room.ordered ? 56 : 16;
            var name = Ui.Text(rt, item.name, 16, Theme.Ink, true);
            Ui.Place(name.rectTransform, x0, 10, 1040 - x0 - 16, 24);
            name.enableWordWrapping = false; name.overflowMode = TextOverflowModes.Ellipsis;
            var detail = Ui.Text(rt, item.detail, 13, Theme.Text2);
            Ui.Place(detail.rectTransform, x0, 34, 1040 - x0 - 16, 22);
            detail.enableWordWrapping = false; detail.overflowMode = TextOverflowModes.Ellipsis;
            float fy = 60; int col = 0;
            foreach (var f in perRow)
            {
                bool wide = Wide(f);
                float fx = wide ? x0 : x0 + col * 500;
                Chip(rt, fx, fy, wide ? 1040 - x0 - 16 : 480, f, item.key);
                if (wide) { fy += 44; col = 0; }
                else { col++; if (col == 2) { col = 0; fy += 44; } }
            }
            y += h + 8;
        }
        // Things not listed yet, to add (the asset nobody listed).
        var hidden = room.items.Where(i => i.hidden && !st.added.Contains(i.key)).ToList();
        if (hidden.Count > 0 && !locked)
        {
            var addLabel = Ui.Eyebrow(content, "SOMETHING NOT ON THE LIST?  ·  ADD IT", Theme.Muted);
            Ui.Place(addLabel.rectTransform, 0, y + 6, 600, 18);
            float ax = 0;
            foreach (var h in hidden)
            {
                string key = h.key;
                var b = Ui.Button(content, "+ " + h.name, () => { st.added.Add(key); st.order.Add(key); Build(); }, Ui.ButtonStyle.Secondary, 13);
                Ui.Place(b.GetComponent<RectTransform>(), ax, y + 28, 340, 36);
                ax += 350;
            }
            y += 76;
        }
        // The room's one-off decisions.
        if (once.Count > 0)
        {
            var oh = Ui.Eyebrow(content, "FOR THE WHOLE ROOM", Theme.TealDeep);
            Ui.Place(oh.rectTransform, 0, y + 8, 600, 18);
            y += 30;
            foreach (var f in once) { Chip(content, 0, y, 1040, f, "room"); y += 48; }
        }
        if (room.signable)
        {
            var sh = Ui.Eyebrow(content, "THE RECORD", Theme.TealDeep);
            Ui.Place(sh.rectTransform, 0, y + 8, 600, 18);
            var sig = Ui.Text(content, st.signed ? $"Signed by {st.signedBy} on {st.signedOn}." : "Not signed. A review that is not written down, dated and signed is not done.", 14, st.signed ? Theme.TealDeep : Theme.Text2);
            Ui.Place(sig.rectTransform, 0, y + 30, 700, 24);
            if (!locked)
            {
                var sign = Ui.Button(content, st.signed ? "Signed" : "Sign and date the record", () => { st.signed = true; st.signedBy = "You, as the reviewer"; st.signedOn = System.DateTime.Now.ToString("d MMM yyyy"); Sfx.Buy(); Build(); }, st.signed ? Ui.ButtonStyle.Secondary : Ui.ButtonStyle.Primary, 14);
                Ui.Place(sign.GetComponent<RectTransform>(), 720, y + 22, 300, 40);
            }
            y += 72;
        }
        content.sizeDelta = new Vector2(0, y + 10);
    }

    static bool Wide(Field f) => f.options.Max(o => o.Length) > 34;

    // A decision as a chip: click it, it cycles through its options.
    void Chip(Transform t, float x, float y, float w, Field f, string itemKey)
    {
        int current = st.Pick(itemKey, f.key);
        string text = current >= 0 && current < f.options.Length ? f.options[current] : "?";
        bool unset = current < 0 || f.options[current] == "?";
        var label = Ui.Text(t, f.label, 12, Theme.Muted, true);
        Ui.Place(label.rectTransform, x, y + 2, 150, 18);
        var b = Ui.Button(t, text, () =>
        {
            if (locked) return;
            Sfx.Tick();
            int next = (st.Pick(itemKey, f.key) + 1) % f.options.Length;
            if (f.options[next] == "?" && f.options.Length > 1) next = (next + 1) % f.options.Length;
            st.Set(itemKey, f.key, next);
            Build();
        }, Ui.ButtonStyle.Secondary, 13);
        Ui.Place(b.GetComponent<RectTransform>(), x + 150, y, w - 150, 36);
        b.GetComponent<Image>().color = unset ? Theme.Border : Theme.BorderTeal;
        b.transform.Find("Fill").GetComponent<Image>().color = unset ? Theme.Panel : Theme.Surface2;
        var tl = b.GetComponentInChildren<TextMeshProUGUI>();
        tl.color = unset ? Theme.Muted : Theme.TealDeep;
        tl.alignment = TextAlignmentOptions.MidlineLeft;
        tl.enableWordWrapping = false; tl.overflowMode = TextOverflowModes.Ellipsis;
        Ui.Stretch(tl.rectTransform, 14, 0, 14, 0);
    }

    void Move(int index, int by)
    {
        int to = index + by;
        if (to < 0 || to >= st.order.Count) return;
        (st.order[index], st.order[to]) = (st.order[to], st.order[index]);
        Sfx.Tick();
        Build();
    }
}
