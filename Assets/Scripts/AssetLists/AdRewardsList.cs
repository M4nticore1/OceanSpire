using UnityEngine;

[CreateAssetMenu(fileName = "AdRewardsList", menuName = "Lists/Ad Rewards List")]
public class AdRewardsList : ScriptableObject
{
    private static AdRewardsList _instance;
    public static AdRewardsList Instance
    {
        get
        {
            if (_instance == null) {
                _instance = Resources.Load<AdRewardsList>("Lists/AdRewardsList");
            }
            return _instance;
        }
    }

    [SerializeField] private AdReward[] adRewards;
    public AdReward[] AdRewards => adRewards;
}
