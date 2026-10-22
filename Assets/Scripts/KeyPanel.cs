using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// First-run credentials. In the Editor / desktop: an Anthropic API key (only needed
// when there is no local key file). In the web build: the access code for the
// site's key keeper. Shown automatically when nothing usable is stored, and again
// whenever the server rejects what was given. The clock is frozen while it is open.
public class KeyPanel : MonoBehaviour
{
    [SerializeField] GameObject panelRoot; // the whole panel - can be this object or a child
    [SerializeField] TMP_InputField keyInputField;
    [SerializeField] Button submitButton;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text titleText;   // relabelled for the access code in web builds
    [SerializeField] TMP_Text promptText;

    const string KeyPrefix = "sk-ant-";
    bool checking;

    void Start()
    {
        submitButton.onClick.AddListener(OnSubmit);
        ClaudeClient.OnCredentialsRejected += Show;

        if (ClaudeClient.UsesProxy)
        {
            if (titleText != null) titleText.text = "Access code";
            if (promptText != null) promptText.text = "Enter the access code :";
        }

        if (ClaudeClient.Ready) Hide();
        else Show();
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

    IEnumerator CheckAccessCode(string code)
    {
        checking = true;
        if (errorText != null) errorText.text = "Checking...";

        bool ok = false;
        yield return ClaudeClient.CheckAccessCode(code, result => ok = result);
        checking = false;

        if (!ok)
        {
            if (errorText != null) errorText.text = "That code doesn't work.";
            yield break;
        }
        ClaudeClient.AccessCode = code;
        Hide();
    }
}
