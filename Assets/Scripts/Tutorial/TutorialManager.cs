using System;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private TutorialSequence[] tutorialSequences;
    public TutorialSequence[] TutorialSequences => tutorialSequences;

    public void Init(TutorialData tutorialData)
    {
        if (tutorialData == null || tutorialData.TutorialSequences == null) return;

        for (int i = 0; i < tutorialData.TutorialSequences.Length; i++) {
            if (i >= tutorialSequences.Length) break;

            var sequenceData = tutorialData.TutorialSequences[i];

            var sequence = tutorialSequences[i];
            sequence.Init(sequenceData);
        }
    }
}