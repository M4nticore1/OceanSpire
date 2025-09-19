using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceWidget : MonoBehaviour
{
    [SerializeField] private ItemData resourceData;
    [SerializeField] private Image resourceImage;
    [SerializeField] private TextMeshProUGUI resourceAmountText;
    [SerializeField] private Image resourceAmountBar;
    [HideInInspector]

    private void Start()
    {

    }

    public void UpdateStorageWidget(int currentResourceAmount, int maxResourceAmount)
    {
        resourceAmountText.text = currentResourceAmount.ToString() + "/" + maxResourceAmount.ToString();

        float alpha = (float)currentResourceAmount / (float)maxResourceAmount;
        resourceAmountBar.fillAmount = alpha;
    }

    public void UpdateBuildWidget(int resourceAmount)
    {
        resourceAmountText.text = resourceAmount.ToString();
    }
}
