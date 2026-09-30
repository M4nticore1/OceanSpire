using UnityEngine;

public class InGameNotificationData
{
    public LocalizationItem NameLocalization {  get; private set; }
    public LocalizationItem DescriptionLocaliztion {  get; private set; }

    public InGameNotificationSeverityDefinition SeverityDefinition { get; private set; }
    public int Priority { get; private set; }

    public ILocalizable NameLocalizationHolder { get; private set; }
    public ILocalizable DescriptionLocalizationHolder { get; private set; }

    public InGameNotificationData(LocalizationItem nameLocalization, LocalizationItem descriptionLocaliztion, InGameNotificationSeverityDefinition severityDefinition, int priority, ILocalizable nameLocalizationHolder = null, ILocalizable descriptionLocalizationHolder = null)
    {
        if (nameLocalization == null) {
            Debug.LogError($"{nameof(InGameNotificationData)} NameLocalization is not valid!");
        }
        if (descriptionLocaliztion == null) {
            Debug.LogError($"{nameof(InGameNotificationData)} DescriptionLocaliztion is not valid!");
        }

        NameLocalization = nameLocalization;
        DescriptionLocaliztion = descriptionLocaliztion;

        SeverityDefinition = severityDefinition;
        Priority = priority;

        NameLocalizationHolder = nameLocalizationHolder;
        DescriptionLocalizationHolder = descriptionLocalizationHolder;
    }
}