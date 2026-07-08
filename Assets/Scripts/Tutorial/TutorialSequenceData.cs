using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TutorialSequenceData
{
    public int CurrentStep = 0;
    public bool InProgress = false;
    public bool Completed = false;

    public static TutorialSequenceData Default()
    {
        return new TutorialSequenceData();
    }

    public static TutorialSequenceData Create(TutorialSequence tutorialSequence)
    {
        return new TutorialSequenceData()
        {
            CurrentStep = tutorialSequence.CurrentStep,
            InProgress = tutorialSequence.IsInProgress,
            Completed = tutorialSequence.IsCompleted
        };
    }

    public static TutorialSequenceData[] Create(TutorialSequence[] tutorialSequences)
    {
        List<TutorialSequenceData> tutorialSequencesData = new();

        foreach (var sequence in tutorialSequences) {
            tutorialSequencesData.Add(Create(sequence));
        }

        return tutorialSequencesData.ToArray();
    }
}