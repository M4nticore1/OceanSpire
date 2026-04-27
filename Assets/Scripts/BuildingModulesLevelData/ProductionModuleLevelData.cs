using UnityEngine;

[System.Serializable]
public class ProducedItem
{
    [SerializeField] private ItemInstance produceItem;
    public ItemInstance ProductionItem => produceItem;

    [SerializeField] private ItemInstance[] consumeResources;
    public ItemInstance[] ConsumeResources => consumeResources;

    [SerializeField] private int produceTime;
    public int ProduceTime => produceTime;
}

[CreateAssetMenu(fileName = "ProductionBuildingLevelData", menuName = "Scriptable Objects/ProductionBuildingLevelData")]
public class ProductionModuleLevelData : BuildingModuleLevelData
{
    public ProducedItem[] producedResources;
}
