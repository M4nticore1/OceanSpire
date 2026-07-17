using System;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private TutorialSequence[] tutorialSequences;
    public TutorialSequence[] TutorialSequences => tutorialSequences;

    public void Init()
    {
        Init(TutorialData.Create(this));
    }

    public void Init(TutorialData tutorialData)
    {
        if (tutorialData == null || tutorialData.TutorialSequences == null) {
            Debug.LogError($"[{nameof(TutorialManager)}] Tutorial Data or Tutorial Sequences is not valid!");
            Init();
            return;
        }

        for (int i = 0; i < tutorialData.TutorialSequences.Length; i++) {
            if (i >= tutorialSequences.Length) break;

            var sequence = tutorialSequences[i];
            if (!sequence) {
                Debug.LogError($"[{nameof(TutorialManager)}] Sequence is not valid!");
                continue;
            }

            var sequenceData = tutorialData.TutorialSequences[i];
            if (sequenceData != null) {
                sequence.Init(sequenceData);
            }
            else {
                Debug.LogError($"[{nameof(TutorialManager)}] Sequence Data is not valid!");
                sequence.Init();
            }
        }
    }
}