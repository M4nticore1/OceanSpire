using System;
using UnityEngine;

[Serializable]
public class TutorialData
{
    public TutorialSequenceData[] TutorialSequences; 

    public static TutorialData Create(TutorialManager tutorialManager)
    {
        return new TutorialData()
        {
            TutorialSequences = TutorialSequenceData.Create(tutorialManager.TutorialSequences),
        };
    }
}