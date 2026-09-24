using System;
using System.Collections;
using UnityEngine;

// No network calls. Used to build and test ChatPanel's busy/lock/scrolling behaviour
// before ClaudeClient exists. The delay is deliberate - it's what forces the chat UI
// to handle "waiting for a reply" correctly.
public class FakeAdvisor : MonoBehaviour, IAdvisor
{
    [SerializeField] string cannedReply = "Copy that. Which module are you at, and what do you see?";
    [SerializeField] float replyDelay = 2f;

    public IEnumerator Send(string playerText, Action<string> onReply)
    {
        yield return new WaitForSeconds(replyDelay);
        onReply?.Invoke(cannedReply);
    }
}
