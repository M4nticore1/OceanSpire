using UnityEngine;

public class DescriptionInfoSectionData : InfoSectionData
{
    public LocalizationItem DescriptionLocalizationItem { get; private set; }

    public DescriptionInfoSectionData(LocalizationItem descriptionLocalizationItem) : base()
    {
        DescriptionLocalizationItem = descriptionLocalizationItem;
    }
}