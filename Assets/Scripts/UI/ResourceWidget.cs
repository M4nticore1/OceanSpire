using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceWidget : MonoBehaviour
{
    [SerializeField] private Image resourceImage;
    [SerializeField] private TextMeshProUGUI resourceAmountText;
    [SerializeField] private Image resourceAmountBar;

    public void Initialize(ItemData itemData, int currentResourceAmount, int maxResourceAmount)
    {
        //SetWidgetResourceImage(itemData.ItemIcon);
        //UpdateAmount(currentResourceAmount, maxResourceAmount);
    }

    public void SetWidgetResourceAmount(int resourecAmount)
    {
        resourceAmountText.SetText(resourecAmount.ToString());
    }

    public void UpdateAmount(int currentResourceAmount, int maxResourceAmount)
    {
        resourceAmountText.SetText(currentResourceAmount.ToString() + "/" + maxResourceAmount.ToString());

        if (resourceAmountBar)
        {
            float alpha = 0;
            if (maxResourceAmount > 0)
                alpha = (float)currentResourceAmount / (float)maxResourceAmount;
            else
                alpha = 0.0f;

            Debug.Log(maxResourceAmount);

            resourceAmountBar.fillAmount = alpha;
        }
    }

    public void UpdateAmount(int resourceAmount)
    {
        resourceAmountText.text = resourceAmount.ToString();
    }

    public void SetWidgetResourceImage(Sprite resourceSprite)
    {
        resourceImage.sprite = resourceSprite;
    }
}
