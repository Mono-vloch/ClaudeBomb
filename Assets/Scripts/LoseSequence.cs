using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Handles both game-end outcomes. Lose: explosion -> smoke -> debrief. Win: a moment
// to hear the defuse, a burst of light, then the debrief. The debrief is one screen
// for both: the result, the time, strikes and modules, a star rating, and LUMA's
// recap of what the player practised (from LoomaCoach), read aloud. It is the
// Maaloomatiia closing screen: Deep Knowledge Blue, the full wordmark top-left.
// Triggered automatically by GameManager.OnGameEnded; builds its own UI.
public class LoseSequence : MonoBehaviour
{
    [SerializeField] GameObject explosionEffect;
    [SerializeField] GameObject smokeEffect;
    [SerializeField] GameObject bombObject; // the whole device - hidden the moment it explodes
    [SerializeField] float explosionToSmokeDelay = 1.5f;
    [SerializeField] float smokeToPanelDelay = 1.5f;
    [SerializeField] float winToPanelDelay = 1.6f;

    void Start()
    {
        GameManager.Instance.OnGameEnded += HandleGameEnded;

        if (explosionEffect != null) explosionEffect.SetActive(false);
        if (smokeEffect != null) smokeEffect.SetActive(false);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameEnded -= HandleGameEnded;
    }

    void HandleGameEnded(GameManager.RunState state)
    {
        if (state == GameManager.RunState.Lost)
            StartCoroutine(PlayLoseSequence());
        else if (state == GameManager.RunState.Won)
            StartCoroutine(PlayWinSequence());
    }

    IEnumerator PlayLoseSequence()
    {
        if (explosionEffect != null) explosionEffect.SetActive(true);

        yield return new WaitForSeconds(explosionToSmokeDelay);
        if (bombObject != null) bombObject.SetActive(false);
        if (smokeEffect != null) smokeEffect.SetActive(true);

        yield return new WaitForSeconds(smokeToPanelDelay);
        yield return Debrief(won: false);
    }

    IEnumerator PlayWinSequence()
    {
        var cam = FindFirstObjectByType<BombCameraController>();
        if (cam != null)
        {
            cam.Scripted = true;
            cam.GlideHome(1.2f);
        }
        yield return new WaitForSecondsRealtime(winToPanelDelay);
        yield return Debrief(won: true);
    }


    void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ---- The debrief ----

    IEnumerator Debrief(bool won)
    {
        var gm = GameManager.Instance;
        var luma = LoomaCoach.Instance;
        Color tone = won ? Ui.Teal : Ui.Danger;
        int stars = won ? Mathf.Max(1, 3 - gm.Strikes) : 0;
        string left = GameManager.FormatTime(gm.TimeRemaining);

        var canvas = Ui.NewCanvas("DebriefCanvas", transform, 45);
        var root = (RectTransform)canvas.transform;
        var dim = Ui.NewImage("Dim", root, Ui.Ink); // solid: the explosion's glare would shine through
        Ui.Stretch(dim.rectTransform);
        var dimGroup = Ui.Group(dim);
        dimGroup.alpha = 0f;
        var vignette = Ui.NewImage("Vignette", dim.transform, tone.WithAlpha(0.12f), Ui.Vignette);
        vignette.raycastTarget = false;
        Ui.Stretch(vignette.rectTransform);
        if (won) StartCoroutine(Burst(root));

        var card = Ui.NewPanel("Card", root, Ui.Ink2, 24).rectTransform;
        Ui.Place(card, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100f, 800f));
        var cardGroup = Ui.Group(card);
        cardGroup.alpha = 0f;

        // The result, under the wordmark.
        var logo = Ui.NewLogo(card, dark: true, 220f);
        Ui.Place(logo.rectTransform, new Vector2(0f, 1f), new Vector2(48f, -40f), logo.rectTransform.sizeDelta);
        Text(Ui.NewLabel("Kicker", card, "Debrief  /  Bay 3", 12f, Ui.Faint), 48f, -100f, 600f, 18f);
        var headline = Ui.NewHeading("Headline", card, won ? "Device defused" : "Detonation", 64f, tone);
        Text(headline, 46f, -122f, 760f, 80f);
        string sub = won ? (gm.NoClock ? "Bay 3 is safe. You and Claude defused every module." : $"Bay 3 is safe. You and Claude finished with {left} to spare.")
                   : gm.TimeRemaining <= 0f ? "Time ran out before the last module was safe."
                   : gm.NoClock ? "Three mistakes, and the device went off." : $"Three mistakes. The device went off with {left} on the clock.";
        Text(Ui.NewText("Sub", card, sub, 22f, Ui.Faint), 48f, -206f, 700f, 32f);

        var starIcons = new List<Image>();
        for (int i = 0; i < 3; i++)
        {
            var star = Ui.NewIcon("Star", card, Ui.Star, Ui.Faint.WithAlpha(0.25f), 58f);
            star.rectTransform.anchorMin = star.rectTransform.anchorMax = Vector2.one; // centred on a point from the top right
            star.rectTransform.anchoredPosition = new Vector2(-77f - (2 - i) * 66f, -160f);
            starIcons.Add(star);
        }

        // The numbers.
        var tiles = new[]
        {
            gm.NoClock ? ("Clock", "Practice", Color.white) : ("Time left", left, gm.TimeRemaining > 0f ? Color.white : Ui.Danger),
            ("Strikes", $"{gm.Strikes} / {gm.MaxStrikes}", gm.Strikes == 0 ? Ui.Teal : Color.white),
            ("Modules defused", $"{gm.SolvedCount} / {Plates.All.Length}", gm.SolvedCount == Plates.All.Length ? Ui.Teal : Color.white),
        };
        for (int i = 0; i < tiles.Length; i++)
        {
            var tile = Ui.NewPanel("Tile", card, Ui.Ink, 8);
            Ui.Place(tile.rectTransform, new Vector2(0f, 1f), new Vector2(48f + i * 340f, -262f), new Vector2(324f, 100f));
            Text(Ui.NewLabel("Label", tile.transform, tiles[i].Item1, 11f, Ui.Faint), 22f, -18f, 280f, 16f);
            var value = Ui.NewHeading("Value", tile.transform, tiles[i].Item2, 38f, tiles[i].Item3);
            Text(value, 22f, -40f, 280f, 48f);
        }

        // LUMA's recap.
        var recapRows = new List<CanvasGroup>();
        if (luma != null)
        {
            var portrait = Ui.NewPanel("Portrait", card, Ui.Ink, 12);
            Ui.Place(portrait.rectTransform, new Vector2(0f, 1f), new Vector2(48f, -392f), new Vector2(200f, 226f));
            var view = Ui.NewRect("Luma", portrait.transform).gameObject.AddComponent<RawImage>();
            view.texture = luma.View;
            view.raycastTarget = false;
            Ui.Stretch(view.rectTransform, new Vector2(6f, 6f), new Vector2(-6f, -6f));
            Text(Ui.NewLabel("LumaName", card, "LUMA's recap", 12f, Ui.Faint), 48f, -632f, 220f, 18f);

            string said = luma.ReadRecap(won ? GameManager.RunState.Won : GameManager.RunState.Lost);
            var quote = Ui.NewText("Quote", card, said, 23f, Ui.Teal);
            quote.fontStyle = FontStyles.Italic;
            float quoteHeight = quote.GetPreferredValues(said, 770f, 0f).y;
            Text(quote, 284f, -392f, 770f, quoteHeight);
            recapRows.Add(Ui.Group(quote));

            float y = -392f - quoteHeight - 20f; // the rows start under however many lines the quote takes
            foreach (var (result, label) in luma.Recap())
            {
                var row = Ui.NewRect("Row", card);
                Ui.Place(row, new Vector2(0f, 1f), new Vector2(284f, y), new Vector2(770f, 36f));
                // A bare mark: teal tick, red cross, or a faint ring when it never came up.
                Color c = result == LoomaCoach.Practised.Yes ? Ui.Teal : result == LoomaCoach.Practised.Missed ? Ui.Danger : Ui.Faint;
                var mark = Ui.NewIcon("Mark", row, result == LoomaCoach.Practised.Yes ? Ui.Check : result == LoomaCoach.Practised.Missed ? Ui.Cross : Ui.Ring, c, 30f);
                Ui.Place(mark.rectTransform, new Vector2(0f, 0.5f), Vector2.zero, new Vector2(30f, 30f));
                var text = Ui.NewText("Label", row, label, 21f, result == LoomaCoach.Practised.NotNeeded ? Ui.Faint : Color.white, TextAlignmentOptions.MidlineLeft);
                Ui.Stretch(text.rectTransform, new Vector2(44f, 0f), Vector2.zero);
                recapRows.Add(Ui.Group(row));
                y -= 44f;
            }
            var strikes = Ui.NewText("Strikes", card, luma.StrikeSummary(), 18f, Ui.Faint);
            Text(strikes, 284f, y - 6f, 770f, 26f);
            recapRows.Add(Ui.Group(strikes));
            foreach (var g in recapRows) g.alpha = 0f;
        }

        // What next.
        // What next: a new device, this one again (the best way to practise what LUMA
        // pointed out), or back to the title.
        var again = Ui.NewButton("PlayAgain", card, "New device", Ui.ButtonStyle.Accent, () => GameMenu.Restart(sameDevice: false), 22f);
        Ui.Place((RectTransform)again.transform, new Vector2(1f, 0f), new Vector2(-48f, 40f), new Vector2(240f, 64f));
        var same = Ui.NewButton("SameDevice", card, "Try this device again", Ui.ButtonStyle.GhostOnDark, () => GameMenu.Restart(sameDevice: true), 18f);
        Ui.Place((RectTransform)same.transform, new Vector2(1f, 0f), new Vector2(-304f, 40f), new Vector2(260f, 64f));
        var title = Ui.NewButton("Title", card, "Title screen", Ui.ButtonStyle.GhostOnDark, GameMenu.BackToTitle, 18f);
        Ui.Place((RectTransform)title.transform, new Vector2(0f, 0f), new Vector2(48f, 40f), new Vector2(180f, 64f));
#if !UNITY_WEBGL || UNITY_EDITOR
        var quit = Ui.NewButton("Quit", card, "Quit", Ui.ButtonStyle.GhostOnDark, OnQuitClicked, 18f); // a web page can't close its own tab
        Ui.Place((RectTransform)quit.transform, new Vector2(0f, 0f), new Vector2(244f, 40f), new Vector2(120f, 64f));
#endif

        // In it comes: the dark, the card rising, the stars one by one, then the recap.
        yield return Ui.Fade(dimGroup, 1f, 0.5f);
        yield return Ui.Animate(0.6f, k => { cardGroup.alpha = k; card.anchoredPosition = new Vector2(0f, -40f * (1f - k)); });
        for (int i = 0; i < stars; i++)
        {
            starIcons[i].color = Ui.Teal;
            SoundManager.Instance.PlayChime(i + 2);
            var rt = starIcons[i].rectTransform;
            yield return Ui.Animate(0.35f, k => rt.localScale = Vector3.one * Mathf.Lerp(1.8f, 1f, k), Ui.EaseOutBack);
        }
        foreach (var g in recapRows)
        {
            StartCoroutine(Ui.Fade(g, 1f, 0.35f));
            yield return new WaitForSecondsRealtime(0.18f);
        }
    }

    // Light bursting out from the middle of the screen: the device is safe.
    IEnumerator Burst(RectTransform root)
    {
        var colors = new[] { Ui.Teal, Ui.Cyan, Ui.Aqua, Color.white };
        var bits = new List<(RectTransform rt, Image img, Vector2 velocity)>();
        for (int i = 0; i < 60; i++)
        {
            var img = Ui.NewGlow("Spark", root, colors[i % colors.Length], Vector2.one * Random.Range(10f, 26f));
            float angle = Random.Range(0f, Mathf.PI * 2f), speed = Random.Range(300f, 900f);
            bits.Add((img.rectTransform, img, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed + Vector2.up * 200f));
        }
        for (float t = 0f; t < 2.6f; t += Time.unscaledDeltaTime)
        {
            for (int i = 0; i < bits.Count; i++)
            {
                var (rt, img, v) = bits[i];
                v += Vector2.down * 520f * Time.unscaledDeltaTime;
                bits[i] = (rt, img, v);
                rt.anchoredPosition += v * Time.unscaledDeltaTime;
                img.color = img.color.WithAlpha(1f - t / 2.6f);
            }
            yield return null;
        }
        foreach (var bit in bits) Destroy(bit.rt.gameObject);
    }

    // Places a text from its parent's top-left corner, at x and y (negative, down).
    static void Text(TMP_Text text, float x, float y, float width, float height)
    {
        Ui.Place(text.rectTransform, new Vector2(0f, 1f), new Vector2(x, y), new Vector2(width, height));
    }
}
