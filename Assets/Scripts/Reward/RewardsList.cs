using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RewardsList", menuName = "Lists/Rewards List")]
public class RewardsList : ScriptableObject
{
    private static RewardsList instance;
    public static RewardsList Instance
    {
        get
        {
            if (instance == null) {
                instance = Resources.Load<RewardsList>("Lists/RewardsList");
            }

            return instance;
        }
    }

    [SerializeField] private RewardDefinition[] rewardDefinitions;

    private Dictionary<int, RewardDefinition> rewardDefinitionsDict;

    private Dictionary<int, RewardDefinition> RewardDefinitionsDict
    {
        get
        {
            if (rewardDefinitionsDict == null) {
                rewardDefinitionsDict = new();

                foreach (var def in rewardDefinitions) {
                    if (def == null) continue;

                    rewardDefinitionsDict.Add((int)def.RewardId, def);
                }
            }

            return rewardDefinitionsDict;
        }
    }

    public RewardDefinition GetRewardDefinition(int id)
    {
        RewardDefinitionsDict.TryGetValue(id, out var definition);
        return definition;
    }
}