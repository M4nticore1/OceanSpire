using UnityEngine;

public class DescriptionInfoPanelData : InfoPanelData
{
    public LocalizationItem DescriptionLocalizationItem { get; private set; }

    public DescriptionInfoPanelData(LocalizationItem descriptionLocalizationItem) : base()
    {
        DescriptionLocalizationItem = descriptionLocalizationItem;
    }
}