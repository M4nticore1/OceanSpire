using UnityEngine;

public class StatInfoPanelWidget : InfoPanelWidget
{
    [Header("Stat")]
    [SerializeField] private TextLocalizer statText;

    protected override void HandleShow(InfoPanelData panelData)
    {
        base.HandleShow(panelData);

        var statPanelData = panelData as StatInfoPanelData;
        if (statPanelData == null) {
            Debug.LogError($"[{nameof(StatInfoPanelWidget)}] Stat Info Data is not valid!");
            return;
        }

        if (statText != null) {
            statText.SetPlaceHolderLocalization(statPanelData);
        }
        else {
            Debug.LogError($"[{nameof(StatInfoPanelWidget)}] Stat Text is not valid!");
        }
    }
}