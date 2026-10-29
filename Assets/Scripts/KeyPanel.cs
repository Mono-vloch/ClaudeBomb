using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// First-run credentials. In the Editor / desktop: an Anthropic API key (only needed
// when there is neither a local key file nor Claude Code). In the web build: the access
// code for the site's key keeper. The opening (Opening) shows it after BEGIN when
// nothing usable is stored, and it comes back whenever the server rejects what was
// given. The clock is frozen while it is open. Laid out in the scene; Start gives it
// the game's look - a card over everything, the title screen too.
public class KeyPanel : MonoBehaviour
{
    // Also while hidden (the panel's object is then inactive, and can't be found).
    public static KeyPanel Instance { get; private set; }
    public bool IsOpen => panelRoot != null && panelRoot.activeInHierarchy;

    [SerializeField] GameObject panelRoot; // the whole panel - can be this object or a child
    [SerializeField] TMP_InputField keyInputField;
    [SerializeField] Button submitButton;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text titleText;   // relabelled for the access code in web builds
    [SerializeField] TMP_Text promptText;

    const string KeyPrefix = "sk-ant-";
    bool checking;

    void Awake() => Instance = this;

    void Start()
    {
        submitButton.onClick.AddListener(OnSubmit);
        ClaudeClient.OnCredentialsRejected += Show;

        Restyle();
        Hide();
    }

    void OnDestroy()
    {
        ClaudeClient.OnCredentialsRejected -= Show;
        GamePause.Release(this);
    }

    public void Show()
    {
        panelRoot.SetActive(true);
        keyInputField.text = "";
        if (errorText != null) errorText.text = "";
        keyInputField.ActivateInputField();
        GamePause.Hold(this); // the clock doesn't run while the player types
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
        GamePause.Release(this);
    }

    void OnSubmit()
    {
        if (checking) return;
        string input = keyInputField.text.Trim();

        if (ClaudeClient.UsesProxy)
        {
            StartCoroutine(CheckAccessCode(input));
            return;
        }

        if (!input.StartsWith(KeyPrefix))
        {
            if (errorText != null) errorText.text = $"Key should start with \"{KeyPrefix}\".";
            return;
        }

        ClaudeClient.ApiKey = input;
        Hide();
    }

    void Restyle()
    {
        // Its own canvas layer, over the title screen (40) and the debrief (45).
        var layer = panelRoot.AddComponent<Canvas>();
        layer.overrideSorting = true;
        layer.sortingOrder = 50;
        panelRoot.AddComponent<GraphicRaycaster>();
        if (panelRoot.TryGetComponent(out Image dim)) dim.color = Ui.Ink.WithAlpha(0.93f);

        var card = Ui.NewCard("Card", panelRoot.transform, Color.white, out var background, 20);
        card.SetAsFirstSibling(); // behind the scene's fields and button
        Ui.Place(card, new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(580f, 400f));

        if (titleText != null)
        {
            titleText.text = ClaudeClient.UsesProxy ? "Access code" : "Claude API key";
            titleText.font = Ui.Display;
            titleText.fontSize = 30f;
            titleText.fontStyle = FontStyles.Normal;
            titleText.characterSpacing = 0f;
            titleText.color = Ui.Ink;
            titleText.alignment = TextAlignmentOptions.Center;
            Ui.Place(titleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(500f, 44f));
        }
        if (promptText != null)
        {
            promptText.text = ClaudeClient.UsesProxy
                ? "This game talks to Claude through the project's key keeper. Enter the code you were given."
                : "Paste an Anthropic API key - or install Claude Code and sign in, to use your Claude subscription instead.";
            promptText.font = Ui.Body;
            promptText.fontSize = 20f;
            promptText.color = Ui.Text2;
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.textWrappingMode = TextWrappingModes.Normal;
            Ui.Place(promptText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 82f), new Vector2(480f, 70f));
        }
        Ui.Style(keyInputField, ClaudeClient.UsesProxy ? "Access code" : "sk-ant-...", 22f);
        Ui.Place((RectTransform)keyInputField.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(460f, 62f));
        Ui.Style(submitButton, Ui.ButtonStyle.Primary);
        foreach (var label in submitButton.GetComponentsInChildren<TMP_Text>(true)) { label.text = "Continue"; label.fontSize = 22f; }
        Ui.Place((RectTransform)submitButton.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -82f), new Vector2(460f, 62f));
        keyInputField.onSubmit.AddListener(_ => OnSubmit());
        if (errorText != null)
        {
            errorText.font = Ui.Body;
            errorText.fontSize = 18f;
            errorText.color = Ui.Danger;
            errorText.alignment = TextAlignmentOptions.Center;
            Ui.Place(errorText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -142f), new Vector2(480f, 30f));
        }
    }

    IEnumerator CheckAccessCode(string code)
    {
        checking = true;
        if (errorText != null) { errorText.text = "Checking..."; errorText.color = Ui.Text2; }

        bool ok = false;
        yield return ClaudeClient.CheckAccessCode(code, result => ok = result);
        checking = false;

        if (!ok)
        {
            if (errorText != null) { errorText.text = "That code doesn't work."; errorText.color = Ui.Danger; }
            yield break;
        }
        ClaudeClient.AccessCode = code;
        Hide();
    }
}
