using System;
using UnityEngine;

[Serializable]
public class TutorialData
{
    public TutorialSequenceData[] TutorialSequences = null;

    public static TutorialData Create(TutorialManager tutorialManager)
    {
        return new TutorialData()
        {
            TutorialSequences = TutorialSequenceData.Create(tutorialManager.TutorialSequences),
        };
    }
}