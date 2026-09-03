using UnityEngine;

public class DescriptionInfoSectionWidget : InfoSectionWidget
{
    [Header("Description")]
    [SerializeField] private TextLocalizer descriptionText;

    protected override void HandleInit(InfoSectionData sectionData)
    {
        base.HandleInit(sectionData);

        var descriptionSectionData = sectionData as DescriptionInfoSectionData;
        if (descriptionSectionData == null) {
            Debug.LogError($"[{nameof(DescriptionInfoSectionWidget)}] Description Section Data is not valid!");
            return;
        }

        if (descriptionText != null) {
            var localization = descriptionSectionData.DescriptionLocalizationItem;
            if (localization != null) {
                descriptionText.SetLocalizationItem(descriptionSectionData.DescriptionLocalizationItem);
            }
            else {
                Debug.LogError($"[{nameof(DescriptionInfoSectionWidget)}] Localization Item is not valid!");
            }
        }
        else {
            Debug.LogError($"[{nameof(DescriptionInfoSectionWidget)}] Description Text is not valid!");
        }
    }
}