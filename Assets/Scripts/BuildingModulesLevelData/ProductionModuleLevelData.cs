using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProduceResource
{
    public ItemInstance produceItem;
    public int maxAmount;
    public int produceTime;
    public List<ItemInstance> consumeResources;
}

[CreateAssetMenu(fileName = "ProductionBuildingLevelData", menuName = "Scriptable Objects/ProductionBuildingLevelData")]
public class ProductionModuleLevelData : BuildingModuleLevelData
{
    public List<ProduceResource> producedResources = new List<ProduceResource>();
}
