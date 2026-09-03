using System.Collections.Generic;
using UnityEngine;

public class CraftsInfoSectionData : InfoSectionData
{
    public List<CraftItemDefinition> Crafts {  get; private set; }

    public CraftsInfoSectionData(List<CraftItemDefinition> crafts) : base()
    {
        Crafts = crafts;
    }
}