using UnityEngine;

public class DiscardInfoPanelData : InfoPanelData
{
    public ItemInstance Item { get; private set; }

    public DiscardInfoPanelData(ItemInstance item) : base()
    {
        Item = item;
    }
}