using System;
using UnityEngine;

public class TutorialSequence : MonoBehaviour
{
    [SerializeField] private EventListener showEventListener;

    [SerializeField] private TutorialStep[] tutorialSteps;
    public TutorialStep[] TutorialSteps => tutorialSteps;

    public int CurrentStep = 0;
    public bool IsInProgress = false;
    public bool IsCompleted = false;

    public event Action OnCompleted;

    private void OnEnable()
    {
        if (showEventListener) {
            showEventListener.OnTriggered += OnShowEventListenerTriggered;
        }
        else {
            Debug.Log($"eventListener not found at {name}");
        }

        TutorialStep.OnTutorialStepCompleted += OnTutorialStepCompleted;
    }

    private void OnDisable()
    {
        TutorialStep.OnTutorialStepCompleted -= OnTutorialStepCompleted;
    }

    public void Init(TutorialSequenceData tutorialSequenceData)
    {
        SetCurrentStep(tutorialSequenceData.CurrentStep);
        SetCompleted(tutorialSequenceData.InProgress ? true : tutorialSequenceData.Completed);
        SetInProgress(IsInProgress ? true : tutorialSequenceData.InProgress);
        TryShowTutorialStep(tutorialSequenceData.CurrentStep);
    }

    public void SetCurrentStep(int value)
    {
        CurrentStep = value;
    }

    public void SetInProgress(bool value)
    {
        IsInProgress = value;
    }

    public void SetCompleted(bool value)
    {
        IsCompleted = value;

        if (IsCompleted) {
            OnCompleted?.Invoke();
        }
    }

    private void AddCurrentStep()
    {
        SetCurrentStep(CurrentStep + 1);
    }

    private void UpdateCompleted()
    {
        SetCompleted(CurrentStep >= tutorialSteps.Length);
    }

    private void UpdateInProgress()
    {
        SetInProgress(CurrentStep < tutorialSteps.Length);
    }

    private void TryShowTutorialStep(int tutorialStepIndex)
    {
        if (!IsInProgress) return;
        if (IsCompleted) return;
        if (tutorialStepIndex >= tutorialSteps.Length) return;

        var tutorialStep = tutorialSteps[tutorialStepIndex];
        tutorialStep.Show();
    }

    private void OnShowEventListenerTriggered()
    {
        if (!ShouldStartSequence()) return;

        SetInProgress(true);
        TryShowTutorialStep(CurrentStep);
    }

    private void OnTutorialStepCompleted(TutorialStep tutorialStep)
    {
        if (!ShouldShowNextStep(tutorialStep)) return;

        AddCurrentStep();
        UpdateInProgress();
        UpdateCompleted();
        TryShowTutorialStep(CurrentStep);
    }

    private bool ShouldShowNextStep(TutorialStep tutorialStep)
    {
        if (!IsInProgress) return false;
        if (IsCompleted) return false;
        if (CurrentStep >= tutorialSteps.Length) return false;
        if (tutorialStep != tutorialSteps[CurrentStep]) return false;

        return true;
    }

    private bool ShouldStartSequence()
    {
        if (IsCompleted) return false;
        if (IsInProgress) return false;

        return true;
    }
}