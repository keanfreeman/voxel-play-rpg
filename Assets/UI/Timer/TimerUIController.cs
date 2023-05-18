using Saving;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TimerUIController : MonoBehaviour, ISaveable
{
    [SerializeField] UIDocument timerUIDocument;
    [SerializeField] SaveManager saveManager;
    [SerializeField] CombatManager combatManager;

    VisualElement wholeScreen;
    Label daysLabel;
    Label hoursLabel;
    Label minutesLabel;
    Label secondsLabel;

    TimeRemaining timeRemaining;
    Coroutine passTimeCoroutine = null;

    void Awake() {
        wholeScreen = timerUIDocument.rootVisualElement.Q<VisualElement>("WholeScreen");
        daysLabel = wholeScreen.Q<Label>("DaysLabel");
        hoursLabel = wholeScreen.Q<Label>("HoursLabel");
        minutesLabel = wholeScreen.Q<Label>("MinutesLabel");
        secondsLabel = wholeScreen.Q<Label>("SecondsLabel");

        combatManager.roundEnded += CombatManager_roundEnded;
    }

    private void CombatManager_roundEnded() {
        // todo - lose game when time runs out
        bool outOfTime = DeductSeconds(6);
    }

    public void PopulateSaveData(SaveData saveData) {
        saveData.timeRemaining = timeRemaining;
    }

    public IEnumerator LoadFromSaveData(SaveData saveData) {
        ClearData();
        if (saveData.timeRemaining == null) {
            SetRemainingTime(new TimeRemaining(1, 0, 0, 10));
        }
        else {
            SetRemainingTime(saveData.timeRemaining);
        }
        StartTimer();
        yield break;
    }

    private void ClearData() {
        PauseTimer();
        timeRemaining = null;
    }

    public bool DeductMinutes(int minutesToDeduct) {
        timeRemaining.minutes -= minutesToDeduct;
        bool outOfTime = FixTime();
        UpdateClockDisplay();
        return outOfTime;
    }

    public bool DeductSeconds(int secondsToDeduct) {
        timeRemaining.seconds -= secondsToDeduct;
        bool outOfTime = FixTime();
        UpdateClockDisplay();
        return outOfTime;
    }

    // returns true if time has run out
    private bool FixTime() {
        while (timeRemaining.seconds < 0) {
            timeRemaining.minutes -= 1;
            timeRemaining.seconds += 60;
        }
        while (timeRemaining.minutes < 0) {
            timeRemaining.hours -= 1;
            timeRemaining.minutes += 60;
        }
        while (timeRemaining.hours < 0) {
            timeRemaining.days -= 1;
            timeRemaining.hours += 24;
        }
        return timeRemaining.days < 0;
    }

    public void SetRemainingTime(TimeRemaining timeRemaining) {
        if (timeRemaining.days > 364 || timeRemaining.hours > 23 
                || timeRemaining.minutes > 59 || timeRemaining.seconds > 59) {
            throw new ArgumentException("Invalid times provided.");
        }

        this.timeRemaining = timeRemaining;

        UpdateClockDisplay();
    }

    private void UpdateClockDisplay() {
        daysLabel.text = IntToClockString(timeRemaining.days, 3);
        hoursLabel.text = IntToClockString(timeRemaining.hours, 2);
        minutesLabel.text = IntToClockString(timeRemaining.minutes, 2);
        secondsLabel.text = IntToClockString(timeRemaining.seconds, 2);
    }

    private string IntToClockString(int time, int frontPadding) {
        string timeString = time.ToString();
        while (timeString.Length != frontPadding) {
            timeString = "0" + timeString;
        }
        return timeString;
    }

    private IEnumerator PassTime() {
        while (true) {
            DeductSeconds(1);
            yield return new WaitForSeconds(1);
        }
    }

    public void StartTimer() {
        passTimeCoroutine = StartCoroutine(PassTime());
    }

    public void PauseTimer() {
        if (passTimeCoroutine != null) StopCoroutine(passTimeCoroutine);
    }
}

[Serializable]
public class TimeRemaining {
    public int days, hours, minutes, seconds;

    public TimeRemaining(int days, int hours, int minutes, int seconds) {
        this.days = days;
        this.hours = hours;
        this.minutes = minutes;
        this.seconds = seconds;
    }
}
