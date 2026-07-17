using System;
using UnityEngine;

[Serializable]
public class TutorialData
{
    public TutorialSequenceData[] TutorialSequences = null;

    public static TutorialData Default()
    {
        return new TutorialData();
    }

    public static TutorialData Create(TutorialManager tutorialManager)
    {
        if (!tutorialManager) {
            Debug.LogError($"[{nameof(TutorialData)}] Tutorial Manager is not valid!");
            return Default();
        }

        return new TutorialData()
        {
            TutorialSequences = TutorialSequenceData.Create(tutorialManager.TutorialSequences),
        };
    }
}