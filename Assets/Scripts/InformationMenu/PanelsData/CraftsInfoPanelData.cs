using System.Collections.Generic;
using UnityEngine;

public class CraftsInfoPanelData : InfoPanelData
{
    public CraftItemDefinition[] Crafts {  get; private set; }

    public CraftsInfoPanelData(CraftItemDefinition[] crafts) : base()
    {
        Crafts = crafts;
    }
}