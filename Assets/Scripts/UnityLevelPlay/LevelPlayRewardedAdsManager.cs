using System;
using Unity.Services.LevelPlay;
using UnityEngine;

public class LevelPlayRewardedAdsManager : MonoBehaviour
{
    private const string k_AndroidAppKey = "25b4e02fd";
    private const string k_AppleAppKey = "25b4e02fd";

    private const float k_RewardCooldownSeconds = 3;

    [SerializeField] private string m_AdUnitId = "l5dwrstszqpgcc7l";
    [SerializeField] private string m_PlacementName = "Main_Menu";

    private bool m_IsInitialized;
    private LevelPlayRewardedAd m_RewardedAd;

    private string m_LastAdToken;
    private DateTime m_LastAdCompletionTime;

    public event Action<bool> adSuccessfullyCompleted;
    public event Action<bool> adAvailable;

    private void Start()
    {
        
    }

    private void RegisterSDKEvents()
    {
        //LevelPlay.OnInitFailed +=
    }
}
