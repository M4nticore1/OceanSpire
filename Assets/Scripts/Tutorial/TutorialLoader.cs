using UnityEngine;

public class TutorialLoader : WorldLoader
{
    [SerializeField] private TutorialManager tutorialManager;

    protected override void Load(WorldData worldData)
    {
        var tutorialData = worldData != null ? worldData.Tutorial : new TutorialData();
        tutorialManager.Init(tutorialData);
    }
}