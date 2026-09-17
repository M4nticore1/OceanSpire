using UnityEngine;

public class StorageInfoPanelWidget : InfoPanelWidget
{
    [SerializeField] private StoragePanel storagePanel;

    protected override void HandleShow(InfoPanelData infoPanelData)
    {
        base.HandleShow(infoPanelData);

        var storageInfoPanelData = infoPanelData as StorageInfoPanelData;
        if (storageInfoPanelData == null) {
            Debug.LogError($"[{nameof(StorageInfoPanelWidget)}] Storage Info Panel Data is not valid!");
            return;
        }

        storagePanel.SetStacksAndApply(storageInfoPanelData.Stacks);
    }
}