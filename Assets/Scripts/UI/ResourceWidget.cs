using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceWidget : MonoBehaviour
{
    [SerializeField] private ItemData resourceData;
    [SerializeField] private Image resourceImage;
    [SerializeField] private TextMeshProUGUI resourceAmountText;
    [SerializeField] private Image resourceAmountBar;

    public void SetWidgetResourceAmount(int resourecAmount)
    {
        resourceAmountText.SetText(resourecAmount.ToString());
    }

    public void SetWidgetResourceImage(Sprite resourceSprite)
    {
        resourceImage.sprite = resourceSprite;
    }

    public void UpdateStorageWidget(int currentResourceAmount, int maxResourceAmount)
    {
        resourceAmountText.text = currentResourceAmount.ToString() + "/" + maxResourceAmount.ToString();

        if (resourceAmountBar)
        {
            float alpha = (float)currentResourceAmount / (float)maxResourceAmount;
            resourceAmountBar.fillAmount = alpha;
        }
    }

    public void UpdateBuildWidget(int resourceAmount)
    {
        resourceAmountText.text = resourceAmount.ToString();
    }
}
