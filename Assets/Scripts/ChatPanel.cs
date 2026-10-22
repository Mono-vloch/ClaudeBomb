using System.Collections;
using System.Text.RegularExpressions;
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

    // Claude starts each reply with a hidden tag - [ACT] an instruction, [ASK] it
    // needs more from the operator, [PAGE] it needs a manual page, [INFO] anything
    // else - so Looma can react to how the conversation goes. Never shown.
    static readonly Regex ReplyTag = new Regex(@"^\s*\[(ACT|ASK|PAGE|INFO)\]\s*", RegexOptions.IgnoreCase);

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
        var desk = PaperDesk.Instance;
        bool hasPages = desk != null && desk.HasAttachments;
        if ((string.IsNullOrEmpty(text) && !hasPages) || advisor == null) return;

        // Attached manual pages go out with this message. A page that still shows
        // private information is blocked here, before anything is sent - Looma
        // explains why and the typed text stays in the box.
        string pagesNote = "";
        if (hasPages && !desk.TryShareAttachments(out pagesNote)) return;

        SoundManager.Instance.PlayMessageSound();
        AddMessage("You", hasPages ? $"<i>{pagesNote}</i> {text}" : text, playerTextColor);
        inputField.text = "";
        SetBusy(true);
        pendingMessage = AddMessage("Claude", "...", claudeTextColor);

        StartCoroutine(advisor.Send(hasPages ? $"{pagesNote}\n{text}" : text, OnReplyReceived));
    }

    // A line from the game itself (not the player or Claude), e.g. why a strike happened.
    public void AddNote(string text, Color color)
    {
        TMP_Text msg = Instantiate(messagePrefab, contentParent);
        msg.text = text;
        msg.color = color;
        msg.gameObject.SetActive(true);

        ScrollToBottom();
    }

    void OnReplyReceived(string reply)
    {
        string tag = "";
        var match = ReplyTag.Match(reply);
        if (match.Success)
        {
            tag = match.Groups[1].Value.ToUpperInvariant();
            reply = reply.Substring(match.Length);
        }

        SoundManager.Instance.PlayMessageSound();
        if (pendingMessage != null) pendingMessage.text = $"<b>Claude:</b> {reply}";
        else AddMessage("Claude", reply, claudeTextColor);

        pendingMessage = null;
        SetBusy(false);
        // The "..." placeholder just grew into the full reply: scroll again, or the
        // end of a long reply stays hidden behind the input box.
        ScrollToBottom();

        if (tag != "" && LoomaCoach.Instance != null) LoomaCoach.Instance.OnClaudeReply(tag);
    }

    TMP_Text AddMessage(string sender, string text, Color color)
    {
        TMP_Text msg = Instantiate(messagePrefab, contentParent);
        msg.text = $"<b>{sender}:</b> {text}";
        msg.color = color;
        msg.gameObject.SetActive(true);

        ScrollToBottom();
        return msg;
    }

    // Rebuild the content's layout now so the new height is known before scrolling,
    // then pin to the bottom once more next frame, after TMP has settled the
    // final height of any wrapped lines.
    void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
        scrollRect.verticalNormalizedPosition = 0f;
        StartCoroutine(ScrollToBottomNextFrame());
    }

    IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
        scrollRect.verticalNormalizedPosition = 0f;
    }

    void SetBusy(bool busy)
    {
        inputField.interactable = !busy;
        sendButton.interactable = !busy;
        if (!busy) inputField.ActivateInputField();
    }
}
