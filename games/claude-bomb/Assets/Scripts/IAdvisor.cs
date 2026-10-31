using System;
using System.Collections;

// Both FakeAdvisor (for building/testing the chat UI) and ClaudeClient (the real
// thing) implement this. ChatPanel only ever talks to this interface.
public interface IAdvisor
{
    IEnumerator Send(string playerText, Action<string> onReply);
}
