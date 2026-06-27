using UnityEngine;

public class TutorialLoader : PlayerLoader
{
    [SerializeField] private TutorialManager tutorialManager;

    protected override void Load(PlayerData playerData)
    {
        var tutorialData = playerData?.Tutorial;

        if (tutorialData != null) {
            tutorialManager.Init(tutorialData);
        }
        else {
            tutorialManager.Init();
        }
    }
}