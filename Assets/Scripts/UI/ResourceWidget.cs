using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceWidget : MonoBehaviour
{
    private ItemInstance item;
    [SerializeField] private Image resourceImage;
    [SerializeField] private TextMeshProUGUI resourceAmountText;
    [SerializeField] private Image resourceAmountBar;

    public void SetItem(ItemInstance item)
    {
        this.item = item;
    }

    public void SetAmount(int amount)
    {
        resourceAmountText.SetText(amount.ToString());
    }

    public void SetAmountAndMaxAmount(int amount, int maxAmount)
    {
        resourceAmountText.SetText(amount.ToString() + "/" + maxAmount.ToString());

        if (resourceAmountBar)
        {
            float alpha = 0;
            if (maxAmount > 0)
                alpha = (float)amount / maxAmount;
            else
                alpha = 0.0f;

            resourceAmountBar.fillAmount = alpha;
        }
    }

    public void SetImage(Sprite resourceSprite)
    {
        resourceImage.sprite = resourceSprite;
    }
}
