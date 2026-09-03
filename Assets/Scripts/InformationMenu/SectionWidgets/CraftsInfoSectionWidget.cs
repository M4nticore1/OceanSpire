using UnityEngine;

public class CraftsInfoSectionWidget : InfoSectionWidget
{
    [SerializeField] private CraftsPanel craftsPanel;

    protected override void HandleInit(InfoSectionData sectionData)
    {
        base.HandleInit(sectionData);

        var craftsSectionData = sectionData as CraftsInfoSectionData;
        if (craftsSectionData == null) {
            Debug.LogError($"[{nameof(CraftsInfoSectionWidget)}] Crafts Section Data is not valid!");
            return;
        }

        if (craftsPanel != null) {
            craftsPanel.SetCraftsAndApply(craftsSectionData.Crafts);
        }
        else {
            Debug.LogError($"[{nameof(CraftsInfoSectionWidget)}] CraftsPanel is not valid!");
        }
    }
}