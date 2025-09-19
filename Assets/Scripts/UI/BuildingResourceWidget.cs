using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingResourceWidget : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resourceAmountText = null;
    [SerializeField] private Image resourceImage = null;

    Color EnoughColor = new Color(0, 1.0f, 0);
    Color NotEnoughColor = new Color(1.0f, 0, 0);

    public void Initialize(int resourceAmount, Sprite resourceIcon)
    {
        resourceAmountText.text = resourceAmount.ToString();

        if (resourceIcon)
            resourceImage.sprite = resourceIcon;
    }

    public void SetResourceText(int inventoryResourceAmount, int resourceToBuildAmount)
    {
        resourceAmountText.SetText(inventoryResourceAmount.ToString() + "/" + resourceToBuildAmount);

        if (inventoryResourceAmount >= resourceToBuildAmount)
            resourceAmountText.color = EnoughColor;
        else
            resourceAmountText.color = NotEnoughColor;
    }
}
