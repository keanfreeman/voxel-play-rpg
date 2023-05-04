using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using Ink.Runtime;
using System.Linq;
using System;
using UnityEngine.InputSystem;
using Orders;
using NonVoxel;

public class DialogueUI : UIHandler {
    [SerializeField] UIDocument uiDocument;
    [SerializeField] CombatUI combatUI;
    [SerializeField] InputManager inputManager;

    private string speakerNameBlock;
    private string currentLine;
    private Story currentStory;
    private bool inChoice = false;
    private Action callback = null;

    // UIDocument
    private TemplateContainer dialogueRoot;
    private Label dialogueText;
    private VisualElement choiceHolder;

    private const float TEXT_WAIT_SPEED = 0.01f;
    private const float PUNCTUATION_WAIT_SPEED = 0.5f;
    private HashSet<char> punctuation = new HashSet<char> { '.', '?', '!' };
    private const string DIALOGUE_UI = "DialogueUI";
    private const string GIVEN_TEXT = "DialogueGivenText";
    private const string CHOICE_HOLDER = "DialogueChoiceHolder";

    // ink variables
    private DialogueOrder orderInfo;
    private static string PLAY_SOUND_EFFECT = "play_sound_effect";
    private static string JOIN_PARTY_REQUEST = "join_party_request";
    private List<string> dialogueVariables = new List<string> { PLAY_SOUND_EFFECT, JOIN_PARTY_REQUEST };
    private List<Order> storyOrders = new();

    private void Awake() {
        dialogueRoot = uiDocument.rootVisualElement.Q<TemplateContainer>(DIALOGUE_UI);
        dialogueText = dialogueRoot.Q<Label>(GIVEN_TEXT);
        choiceHolder = dialogueRoot.Q<VisualElement>(CHOICE_HOLDER);

        dialogueText.text = string.Empty;
        choiceHolder.Clear();
    }

    public bool IsDisplaying() {
        return dialogueRoot.style.display.value != DisplayStyle.None;
    }

    public bool HasStoryOrder() {
        return storyOrders.Count != 0;
    }

    public Order PopLatestStoryOrder() {
        if (storyOrders.Count == 0) return null;

        int lastIndex = storyOrders.Count - 1;
        Order lastOrder = storyOrders[lastIndex];
        storyOrders.RemoveAt(lastIndex);
        return lastOrder;
    }

    public void VariableObserver(string variableName, object newValue) {
        if (variableName == PLAY_SOUND_EFFECT) {
            // todo implement
        }
        else if (variableName == JOIN_PARTY_REQUEST) {
            string partyMemberName = (string)newValue;
            Guid partyMemberID = orderInfo.joinPartyTargets[partyMemberName];
            storyOrders.Add(new JoinPartyOrder(partyMemberID));
        }
    }

    private void SetUpVariableObservation(Story story) {
        List<string> variablesToObserve = dialogueVariables
            .Where((string variable) => {
                return story.variablesState.GetVariableWithName(variable) != null;
            })
            .ToList();
        currentStory.ObserveVariables(variablesToObserve, VariableObserver);
    }

    public void StartDialogue(Story story, DialogueOrder orderInfo, Action callback = null) {
        this.orderInfo = orderInfo;

        string speakerName = orderInfo.speakerName;
        this.speakerNameBlock = speakerName == null ? "" : speakerName + ": ";
        this.callback = callback;
        dialogueText.text = speakerNameBlock;
        SetDisplayState(true);

        currentStory = story;
        SetUpVariableObservation(currentStory);
        currentLine = currentStory.Continue();

        StartCoroutine(TypeLine());
        inputManager.UnlockUIControls(this);
    }

    public void StopDialogue() {
        inputManager.LockUIControls();
        SetDisplayState(false);
        speakerNameBlock = null;
        inChoice = false;
        callback?.Invoke();
    }

    private IEnumerator TypeLine() {
        char[] charArray = currentLine.ToCharArray();
        for (int i = 0; i < charArray.Length; i++) {
            char c = charArray[i];
            dialogueText.text += c;
            if (i < charArray.Length) {
                yield return new WaitForSeconds(
                    punctuation.Contains(c) ? PUNCTUATION_WAIT_SPEED : TEXT_WAIT_SPEED
                );
            }
        }

        DisplayChoices();
    }

    private void DisplayChoices() {
        choiceHolder.Clear();
        List<Choice> choices = currentStory.currentChoices;
        if (choices.Count > 0) {
            for (int i = 0; i < choices.Count; i++) {
                choiceHolder.Add(CreateButton(choices[i].text, i));
            }
            choiceHolder.Children().First().Focus();
            inChoice = true;
        }
    }

    private Button CreateButton(string text, int choiceIndex) {
        Button button = new Button {
            text = text
        };
        button.clicked += () => HandleButtonClick(choiceIndex);
        return button;
    }

    private void HandleButtonClick(int choiceIndex) {
        currentStory.ChooseChoiceIndex(choiceIndex);
        choiceHolder.Clear();
        if (currentStory.canContinue) {
            dialogueText.text = speakerNameBlock;
            currentLine = currentStory.Continue();
            StartCoroutine(TypeLine());
        }
        else {
            StopDialogue();
        }
        inChoice = false;
    }

    public override void SetDisplayState(bool isVisible) {
        if (isVisible) {
            dialogueRoot.style.display = DisplayStyle.Flex;
        }
        else {
            dialogueRoot.style.display = DisplayStyle.None;
        }
    }

    public override void HandleNavigate(InputAction.CallbackContext obj) { }
    public override void HandleCancelNavigate(InputAction.CallbackContext obj) { }
    public override void HandleSubmit(InputAction.CallbackContext obj) {
        if (inChoice) {
            return;
        }

        if (IsDoneDisplayingText()) {
            if (currentStory.canContinue) {
                dialogueText.text = speakerNameBlock;
                currentLine = currentStory.Continue();
                StartCoroutine(TypeLine());
            }
            else {
                StopDialogue();
            }
        }
        else {
            // skip dialogue display
            StopAllCoroutines();
            dialogueText.text = speakerNameBlock + currentLine;
            DisplayChoices();
        }
    }

    private bool IsDoneDisplayingText() {
        return currentLine.Length + speakerNameBlock.Length == dialogueText.text.Length;
    }

    public override void HandleCancel(InputAction.CallbackContext obj) { }
}
