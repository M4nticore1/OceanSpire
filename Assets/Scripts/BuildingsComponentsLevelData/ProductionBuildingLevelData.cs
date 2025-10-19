using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProductionBuildingLevelData", menuName = "Scriptable Objects/ProductionBuildingLevelData")]
public class ProductionBuildingLevelData : BuildingComponentLevelData
{
    public ItemData produceResource = null;
    public int produceResourceAmount = 0;

    public ItemData consumeResource = null;
    public int consumeResourceAmount = 0;

    public int maxResourceAmount = 0;
    public int produceTime = 0;
}
