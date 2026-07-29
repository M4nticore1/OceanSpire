using UnityEngine;

[CreateAssetMenu(fileName = "CraftItem", menuName = "Items/CraftItem")]
public class CraftItemDefinition : ScriptableObject
{
    [SerializeField] private ItemInstance produceItem;
    public ItemInstance ProduceItem => produceItem;

    [SerializeField] private ItemInstance[] consumeResources;
    public ItemInstance[] ConsumeResources => consumeResources;

    [SerializeField] private int produceTime;
    public int ProduceTime => produceTime;

    public CraftItemInstance CreateInstance(CraftItemData data)
    {
        var item = new CraftItemInstance(this, data);
        return item;
    }
}
