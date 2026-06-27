using System;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private TutorialSequence[] tutorialSequences;
    public TutorialSequence[] TutorialSequences => tutorialSequences;

    public void Init()
    {
        var tutorialData = TutorialData.Create(this);
        Init(tutorialData);
    }

    public void Init(TutorialData tutorialData)
    {
        if (tutorialData == null) {
            Debug.LogError("Tutorial Data is not valid");
            Init();
            return;
        }

        for (int i = 0; i < tutorialData.TutorialSequences.Length; i++) {
            if (i >= tutorialSequences.Length) break;

            var sequenceData = tutorialData.TutorialSequences[i];
            if (sequenceData == null) {
                Debug.LogError("SequenceData is not valid");
                continue;
            }

            var sequence = tutorialSequences[i];
            if (!sequence) {
                Debug.LogError("Sequence is not valid");
                continue;
            }

            sequence.Init(sequenceData);
        }
    }
}