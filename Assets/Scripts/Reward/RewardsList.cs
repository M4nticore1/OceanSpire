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
            if (!instance) {
                instance = Resources.Load<RewardsList>("Lists/RewardsList");
            }

            return instance;
        }
    }

    [SerializeField] private AdRewardDefinition[] rewardDefinitions;

    private Dictionary<int, AdRewardDefinition> rewardDefinitionsDict;

    private Dictionary<int, AdRewardDefinition> RewardDefinitionsDict
    {
        get
        {
            if (rewardDefinitionsDict == null) {
                rewardDefinitionsDict = new();

                foreach (var def in rewardDefinitions) {
                    rewardDefinitionsDict.Add((int)def.RewardId, def);
                }
            }

            return rewardDefinitionsDict;
        }
    }

    public AdRewardDefinition GetRewardDefinition(int id)
    {
        RewardDefinitionsDict.TryGetValue(id, out var definition);
        return definition;
    }
}