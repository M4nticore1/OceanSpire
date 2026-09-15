using UnityEngine;

public class StorageInfoPanelController : InfoPanelController
{
    public IStorageProvider storageProvider;

    protected override bool ShouldDisplay(IInformationable informationable)
    {
        storageProvider = informationable as IStorageProvider;
        if (storageProvider == null) return false;

        return storageProvider.StorageStacks.Count > 0;
    }

    protected override InfoPanelData GetData(IInformationable informationable)
    {
        return new StorageInfoPanelData(storageProvider.StorageStacks);
    }
}