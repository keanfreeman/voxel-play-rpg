using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageManager : MonoBehaviour
{
    [SerializeField] MessageUIController messageUIController;
    [SerializeField] float displayTime = 2f;

    private Queue<Message> messages = new();
    private bool erasePermanentMessage = false;
    Coroutine messageCoroutine = null;

    // todo remove
    //private void Update() {
    //    if (Input.GetKeyUp(KeyCode.Keypad0)) {
    //        DisplayMessage(new Message("asdf", false));
    //    }
    //    if (Input.GetKeyUp(KeyCode.Keypad1)) {
    //        DisplayMessage(new Message("asdf2", true));
    //    }
    //    if (Input.GetKeyUp(KeyCode.Keypad2)) {
    //        StopDisplayingPermanentMessage();
    //    }
    //}

    public void DisplayMessage(string message) {
        DisplayMessage(new Message(message));
    }

    public void DisplayMessage(Message message) {
        messages.Enqueue(message);
        if (messageCoroutine == null) {
            messageCoroutine = StartCoroutine(WorkThroughQueue());
        }
    }

    // allows the caller to wait for the message's display finish
    public IEnumerator DisplayMessageCoroutine(Message message) {
        if (message.isPermanent) {
            Debug.Log("Cannot provide a permanent message here.");
            yield break;
        }

        while (messageCoroutine != null) {
            yield return null;
        }
        messages.Enqueue(message);
        yield return WorkThroughQueue();
    }

    private IEnumerator WorkThroughQueue() {
        while (messages.Count > 0) {
            Message next = messages.Dequeue();
            yield return messageUIController.DisplayText(next.text);
            yield return new WaitForSeconds(displayTime);
            while (next.isPermanent && !erasePermanentMessage) {
                yield return null;
            }
            erasePermanentMessage = false;
            yield return messageUIController.Hide();
        }
        messageCoroutine = null;
    }

    public void StopDisplayingPermanentMessage() {
        erasePermanentMessage = true;
    }

}

public class Message {
    public string text;
    public bool isPermanent;

    public Message(string text, bool isPermanent = false) {
        this.text = text;
        this.isPermanent = isPermanent;
    }
}
