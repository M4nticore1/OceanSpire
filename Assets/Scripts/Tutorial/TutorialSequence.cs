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
            Debug.Log($"[{nameof(TutorialSequence)}] EventListener not found at {name}");
        }

        TutorialStep.OnTutorialStepCompleted += OnTutorialStepCompleted;
    }

    private void OnDisable()
    {
        if (showEventListener) {
            showEventListener.OnTriggered -= OnShowEventListenerTriggered;
        }
        else {
            Debug.Log($"[{nameof(TutorialSequence)}] EventListener not found at {name}");
        }

        TutorialStep.OnTutorialStepCompleted -= OnTutorialStepCompleted;
    }

    public void Init()
    {
        Init(TutorialSequenceData.Default() ?? new TutorialSequenceData());
    }

    public void Init(TutorialSequenceData tutorialSequenceData)
    {
        if (tutorialSequenceData == null) {
            Debug.LogError($"[{nameof(TutorialSequence)}] TutorialSequenceData is not valid!");
            Init();
            return;
        }

        SetCurrentStep(tutorialSequenceData.CurrentStep);
        SetInProgress(IsInProgress);
        SetCompleted(tutorialSequenceData.Completed || tutorialSequenceData.InProgress || CurrentStep > 0);
        TryShowStep(tutorialSequenceData.CurrentStep);
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

        if (value) {
            CompleteAllSteps();
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

    private void TryShowStep(int tutorialStepIndex)
    {
        if (!ShouldShowStep(tutorialStepIndex)) return;

        tutorialSteps[tutorialStepIndex].Show();
    }

    private void CompleteAllSteps()
    {
        foreach (var step in TutorialSteps) {
            step.Complete();
        }
    }

    private void TryCompleteStep(int tutorialStepIndex)
    {
        if (tutorialStepIndex >= tutorialSteps.Length) return;

        var tutorialStep = tutorialSteps[tutorialStepIndex];
        tutorialStep.Complete();
    }

    private void OnShowEventListenerTriggered()
    {
        if (!ShouldStartSequence()) return;

        SetInProgress(true);
        TryShowStep(CurrentStep);
    }

    private void OnTutorialStepCompleted(TutorialStep tutorialStep)
    {
        if (!ShouldShowNextStep(tutorialStep)) return;

        AddCurrentStep();
        UpdateInProgress();
        UpdateCompleted();
        TryShowStep(CurrentStep);
    }

    private bool ShouldShowStep(int stepIndex)
    {
        if (!IsInProgress) return false;
        if (IsCompleted) return false;
        if (stepIndex >= tutorialSteps.Length) return false;

        return true;
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

        return true;
    }
}