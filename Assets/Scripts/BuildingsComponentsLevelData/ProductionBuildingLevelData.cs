using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ProducedResource
{
    public ItemInstance producedResource;
    public int maxResourceAmount;
    public int produceTime;
    public List<ItemInstance> consumeResources;
}

[CreateAssetMenu(fileName = "ProductionBuildingLevelData", menuName = "Scriptable Objects/ProductionBuildingLevelData")]
public class ProductionBuildingLevelData : BuildingComponentLevelData
{
    public List<ProducedResource> producedResources = new List<ProducedResource>();
}
