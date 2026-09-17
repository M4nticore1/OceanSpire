using System.Collections.Generic;
using UnityEngine;

public class StorageInfoPanelData : InfoPanelData
{
    public IReadOnlyList<ItemStackInstance> Stacks { get; private set; }

    public StorageInfoPanelData(IReadOnlyList<ItemStackInstance> items) : base()
    {
        Stacks = items;
    }
}