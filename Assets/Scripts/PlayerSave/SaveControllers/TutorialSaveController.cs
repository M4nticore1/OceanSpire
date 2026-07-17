using UnityEngine;

public class TutorialSaveController : PlayerSaveController
{
    [Header("Tutorial")]
    [SerializeField] private TutorialManager tutorialManager;
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

        var currentData = PlayerSaveSystem.GetData();
        if (currentData == null) {
            currentData = PlayerData.Default();
        }

        currentData.Tutorial = TutorialData.Create(tutorialManager);
        PlayerSaveSystem.SaveData(currentData);
    }
}