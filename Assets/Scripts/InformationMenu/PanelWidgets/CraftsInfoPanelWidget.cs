using UnityEngine;

public class CraftsInfoPanelWidget : InfoPanelWidget
{
    [SerializeField] private CraftsPanel craftsPanel;

    protected override void HandleShow(InfoPanelData panelData)
    {
        base.HandleShow(panelData);

        var craftsPanelData = panelData as CraftsInfoPanelData;
        if (craftsPanelData == null) {
            Debug.LogError($"[{nameof(CraftsInfoPanelWidget)}] Crafts Panel Data is not valid!");
            return;
        }

        if (craftsPanel != null) {
            craftsPanel.SetCraftsAndApply(craftsPanelData.Crafts);
        }
        else {
            Debug.LogError($"[{nameof(CraftsInfoPanelWidget)}] Crafts Panel is not valid!");
        }
    }
}