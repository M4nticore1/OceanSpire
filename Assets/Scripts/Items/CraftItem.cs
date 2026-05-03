using UnityEngine;

[CreateAssetMenu(fileName = "CraftItem", menuName = "Items/CraftItem")]
public class CraftItem : ScriptableObject
{
    [SerializeField] private ItemInstance produceItem;
    public ItemInstance ProduceItem => produceItem;

    [SerializeField] private ItemInstance[] consumeResources;
    public ItemInstance[] ConsumeResources => consumeResources;

    [SerializeField] private int produceTime;
    public int ProduceTime => produceTime;
}
