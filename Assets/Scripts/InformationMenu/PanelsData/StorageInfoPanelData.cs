using System.Collections.Generic;
using UnityEngine;

public class StorageInfoPanelData : InfoPanelData
{
    public IReadOnlyList<ItemStack> Items { get; private set; }

    public StorageInfoPanelData(IReadOnlyList<ItemStack> items) : base()
    {
        Items = items;
    }
}