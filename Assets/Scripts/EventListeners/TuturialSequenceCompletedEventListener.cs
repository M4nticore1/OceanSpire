using UnityEngine;

public class TuturialSequenceCompletedEventListener : EventListener
{
    [SerializeField] private TutorialSequence tutorialSequence;

    protected override void Subscribe()
    {
        base.Subscribe();

        tutorialSequence.OnCompleted += OnCompleted;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        tutorialSequence.OnCompleted += OnCompleted;
    }

    private void OnCompleted()
    {
        HandleTriggered();
    }
}