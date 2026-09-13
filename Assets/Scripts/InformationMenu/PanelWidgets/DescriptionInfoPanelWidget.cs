using UnityEngine;

public class DescriptionInfoPanelWidget : InfoPanelWidget
{
    [Header("Description")]
    [SerializeField] private TextLocalizer descriptionText;

    protected override void HandleShow(InfoPanelData panelData)
    {
        base.HandleShow(panelData);

        var descriptionPanelData = panelData as DescriptionInfoPanelData;
        if (descriptionPanelData == null) {
            Debug.LogError($"[{nameof(DescriptionInfoPanelWidget)}] Description Panel Data is not valid!");
            return;
        }

        if (descriptionText != null) {
            var localization = descriptionPanelData.DescriptionLocalizationItem;
            if (localization != null) {
                descriptionText.SetLocalizationItem(descriptionPanelData.DescriptionLocalizationItem);
            }
            else {
                Debug.LogError($"[{nameof(DescriptionInfoPanelWidget)}] Localization Item is not valid!");
            }
        }
        else {
            Debug.LogError($"[{nameof(DescriptionInfoPanelWidget)}] Description Text is not valid!");
        }
    }
}