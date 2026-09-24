using TMPro;
using UnityEngine;
using UnityEngine.UI;

// First-run API key entry. Shown automatically if no key is stored yet. A Settings
// button elsewhere in the UI can call Show() to reopen this later.
public class KeyPanel : MonoBehaviour
{
    [SerializeField] GameObject panelRoot; // the whole panel - can be this object or a child
    [SerializeField] TMP_InputField keyInputField;
    [SerializeField] Button submitButton;
    [SerializeField] TMP_Text errorText;

    const string KeyPrefix = "sk-ant-";

    void Start()
    {
        submitButton.onClick.AddListener(OnSubmit);

        if (ClaudeClient.HasKey) Hide();
        else Show();
    }

    public void Show()
    {
        panelRoot.SetActive(true);
        keyInputField.text = "";
        if (errorText != null) errorText.text = "";
        keyInputField.ActivateInputField();
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
    }

    void OnSubmit()
    {
        string input = keyInputField.text.Trim();

        if (!input.StartsWith(KeyPrefix))
        {
            if (errorText != null) errorText.text = $"Key should start with \"{KeyPrefix}\".";
            return;
        }

        ClaudeClient.ApiKey = input;
        Hide();
    }
}
