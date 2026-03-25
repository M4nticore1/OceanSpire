using UnityEngine;

public class RecievedAdRewardMenu : MonoBehaviour
{
    [SerializeField] SlidePanel slidePanel;

    private void OnEnable()
    {
        EventBus.onAdRewardRecieved += OnAdRewardRecieved;
    }

    private void OnDisable()
    {
        EventBus.onAdRewardRecieved -= OnAdRewardRecieved;
    }

    private void OnAdRewardRecieved(AdReward reward)
    {
        Open();
    }

    private void Open()
    {
        slidePanel.Open();
    }
}
