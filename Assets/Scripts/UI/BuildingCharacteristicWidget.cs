using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingCharacteristicWidget : MonoBehaviour
{
    [SerializeField] private ResourceWidget resourceWidget = null;
    public RectTransform characteristicValueBox = null;
    [SerializeField] private TextMeshProUGUI characteristicNameText = null;
    [SerializeField] private TextMeshProUGUI characteristicValueText = null;
    [SerializeField] private Image characteristicValueImage = null;

    public void SetCharacteristicName(string characteristicName)
    {
        characteristicNameText.SetText(characteristicName);
    }

    public void SetCharacteristicValue(int characteristicValueString)
    {
        characteristicValueImage.gameObject.SetActive(false);

        characteristicValueText.SetText(characteristicValueString.ToString());
    }

    public void SetCharacteristicValue(int characteristicValue, Sprite characteristicSprite)
    {
        characteristicValueText.gameObject.SetActive(false);
        characteristicValueImage.gameObject.SetActive(false);

        ResourceWidget spawnedResourceWidget = Instantiate(resourceWidget, characteristicValueBox.transform);
        spawnedResourceWidget.GetComponent<RectTransform>().anchoredPosition = characteristicValueText.GetComponent<RectTransform>().anchoredPosition;

        spawnedResourceWidget.SetWidgetResourceAmount(characteristicValue);
        spawnedResourceWidget.SetWidgetResourceImage(characteristicSprite);
    }

    public void SetCharacteristicValue(Sprite characteristicValueSprite)
    {
        characteristicValueText.gameObject.SetActive(false);

        characteristicValueImage.sprite = characteristicValueSprite;
    }
}
