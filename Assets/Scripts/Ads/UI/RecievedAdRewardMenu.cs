using UnityEngine;
using UnityEngine.UI;

public class RecievedAdRewardMenu : MonoBehaviour
{
    [SerializeField] SlidePanel slidePanel;
    [SerializeField] Image rewardImage;
    [SerializeField] TextLocalizer receiveText;

    [SerializeField] float showTime = 1f;
    private float currentShowTime = 0f;

    private bool isOpened = false;

    private void OnEnable()
    {
        EventBus.onAdRewardRecieved += OnAdRewardRecieved;
    }

    private void OnDisable()
    {
        EventBus.onAdRewardRecieved -= OnAdRewardRecieved;
    }

    private void Update()
    {
        if (!isOpened) return;

        currentShowTime += Time.deltaTime;
        if (currentShowTime >= showTime) {
            Close();
        }
    }

    private void OnAdRewardRecieved(AdRewardInstance reward)
    {
        Open();
        AssignImage(reward);
        AssignText(reward);
    }

    private void Open()
    {
        slidePanel.Open();

        currentShowTime = 0;
        isOpened = true;
    }

    private void Close()
    {
        slidePanel.Close();
        isOpened = false;
    }

    private void AssignImage(AdRewardInstance reward)
    {
        rewardImage.sprite = reward.rewardData.RewardIcon;
    }

    private void AssignText(AdRewardInstance reward)
    {
        receiveText.SetLocalizationItem(reward.rewardData.ReceievedRewardLocalization);
        receiveText.SetPlaceHolderLocalization(reward);
        receiveText.UpdateText();
    }
}