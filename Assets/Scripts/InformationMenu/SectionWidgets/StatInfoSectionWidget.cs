using UnityEngine;

public class StatInfoSectionWidget : InfoSectionWidget
{
    [Header("Stat")]
    [SerializeField] private TextLocalizer statText;

    protected override void HandleInit(InfoSectionData sectionData)
    {
        base.HandleInit(sectionData);

        var statSectionData = sectionData as StatInfoSectionData;
        if (statSectionData == null) {
            Debug.LogError($"[{nameof(StatInfoSectionWidget)}] Stat Info Data is not valid!");
            return;
        }

        if (statText != null) {
            statText.SetLocalizationItem(statSectionData.LocalizationItem);
            statText.SetPlaceHolderLocalization(statSectionData);
        }
        else {
            Debug.LogError($"[{nameof(StatInfoSectionWidget)}] Stat Text is not valid!");
        }
    }
}