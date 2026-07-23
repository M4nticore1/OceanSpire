using System;
using UnityEngine;

public class TutorialStep : MonoBehaviour
{
    [SerializeField] private GameObject content;

    [SerializeField] private EventListener completeEventListener;
    [SerializeField] private InteractionToggler[] disabledInteractions;
    [SerializeField] private bool blockCameraMovement = false;

    public bool IsCompleted { get; private set; } = false;

    public event Action OnShowed;
    public event Action OnCompleted;

    public static event Action<TutorialStep> OnTutorialStepShowed;
    public static event Action<TutorialStep> OnTutorialStepCompleted;

    private void OnEnable()
    {
        if (completeEventListener) {
            completeEventListener.OnTriggered += OnCompleteEventListenerTriggered;
        }
        else {
            Debug.Log($"eventListener not found at {name}");
        }
    }

    private void OnDisable()
    {
        if (completeEventListener) {
            completeEventListener.OnTriggered -= OnCompleteEventListenerTriggered;
        }
    }

    public void TryShow()
    {
        if (IsCompleted) return;

        Show();
    }

    public void Show()
    {
        OnShow();
        OnShowed?.Invoke();
        OnTutorialStepShowed?.Invoke(this);
    }

    public void Complete()
    {
        OnComplete();
        OnCompleted?.Invoke();
        OnTutorialStepCompleted?.Invoke(this);
    }

    public void SetCompleted(bool value)
    {
        IsCompleted = value;
    }

    protected virtual void OnShow()
    {
        content.gameObject.SetActive(true);
        IsCompleted = true;

        foreach (var disabler in disabledInteractions) {
            disabler.DisableInteraction();
        }

        if (blockCameraMovement)
            InputStateManager.Instance.AddBlockTarget(this);
    }

    protected virtual void OnComplete()
    {
        content.gameObject.SetActive(false);

        foreach (var disabler in disabledInteractions) {
            disabler.EnableInteraction();
        }

        if (blockCameraMovement)
            InputStateManager.Instance.RemoveBlockTarget(this);
    }

    private void OnCompleteEventListenerTriggered()
    {
        Complete();
    }
}