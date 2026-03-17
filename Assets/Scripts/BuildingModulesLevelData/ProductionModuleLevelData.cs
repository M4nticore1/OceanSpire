using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProduceResource
{
    public ItemInstance produceItem;
    public List<ItemInstance> consumeResources;
    public int produceTime;
}

[CreateAssetMenu(fileName = "ProductionBuildingLevelData", menuName = "Scriptable Objects/ProductionBuildingLevelData")]
public class ProductionModuleLevelData : BuildingModuleLevelData
{
    public List<ProduceResource> producedResources = new List<ProduceResource>();
}
