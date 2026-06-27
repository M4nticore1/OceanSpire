using UnityEngine;

public class TutorialSaveController : PlayerSaveController
{
    [SerializeField] private TutorialLoader tutorialLoader;

    protected override void OnSubscribe()
    {
        base.OnSubscribe();

        TutorialStep.OnTutorialStepCompleted += OnTutorialStepCompleted;
    }

    protected override void OnUnsubscribe()
    {
        base.OnUnsubscribe();

        TutorialStep.OnTutorialStepCompleted -= OnTutorialStepCompleted;
    }

    private void OnTutorialStepCompleted(TutorialStep tutorialStep)
    {
        if (!tutorialLoader.IsLoaded) return;

        SavePlayer();
    }
}