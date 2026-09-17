using UnityEngine;

public class DiscardInfoPanelController : InfoPanelController
{
    private ItemInstance item;

    protected override bool ShouldDisplay(IInformationable informationable)
    {
        item = informationable as ItemInstance;
        if (item == null) return false;

        return true;
    }

    protected override InfoPanelData GetData(IInformationable informationable)
    {
        return new DiscardInfoPanelData(item);
    }
}