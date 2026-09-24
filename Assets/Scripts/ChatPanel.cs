using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Scrolling message list, input field, send button. Talks only to IAdvisor - it does
// not know or care whether that's FakeAdvisor or the real ClaudeClient. Locks input
// while a reply is in flight, which the FakeAdvisor's artificial delay forces us to
// get right before any real network call exists.
public class ChatPanel : MonoBehaviour
{
    [SerializeField] MonoBehaviour advisorSource; // drag FakeAdvisor or ClaudeClient here - must implement IAdvisor
    [SerializeField] TMP_InputField inputField;
    [SerializeField] Button sendButton;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] RectTransform contentParent; // the ScrollRect's Content
    [SerializeField] TMP_Text messagePrefab;      // one chat line, saved as a Prefab
    [SerializeField] Color playerTextColor = Color.black;
    [SerializeField] Color claudeTextColor = Color.blue;

    IAdvisor advisor;
    TMP_Text pendingMessage;

    void Awake()
    {
        advisor = advisorSource as IAdvisor;
        if (advisor == null)
            Debug.LogError("[ChatPanel] Advisor Source does not implement IAdvisor.");
    }

    void Start()
    {
        sendButton.onClick.AddListener(OnSendClicked);
        inputField.onSubmit.AddListener(_ => OnSendClicked());
        SetBusy(false);
    }

    void OnSendClicked()
    {
        string text = inputField.text.Trim();
        if (string.IsNullOrEmpty(text) || advisor == null) return;

        SoundManager.Instance.PlayMessageSound();
        AddMessage("You", text, playerTextColor);
        inputField.text = "";
        SetBusy(true);
        pendingMessage = AddMessage("Claude", "...", claudeTextColor);

        StartCoroutine(advisor.Send(text, OnReplyReceived));
    }

    void OnReplyReceived(string reply)
    {
        SoundManager.Instance.PlayMessageSound();
        if (pendingMessage != null) pendingMessage.text = $"<b>Claude:</b> {reply}";
        else AddMessage("Claude", reply, claudeTextColor);

        pendingMessage = null;
        SetBusy(false);
    }

    TMP_Text AddMessage(string sender, string text, Color color)
    {
        TMP_Text msg = Instantiate(messagePrefab, contentParent);
        msg.text = $"<b>{sender}:</b> {text}";
        msg.color = color;
        msg.gameObject.SetActive(true);

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
        return msg;
    }

    void SetBusy(bool busy)
    {
        inputField.interactable = !busy;
        sendButton.interactable = !busy;
        if (!busy) inputField.ActivateInputField();
    }
}
