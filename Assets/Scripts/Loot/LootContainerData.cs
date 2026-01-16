using UnityEngine;

[CreateAssetMenu(fileName = "LootContainerData", menuName = "Loot/LootContainerData")]
public class LootContainerData : ScriptableObject
{
    [field: SerializeField] public int lootContainerId { get; private set; } = 0;
}
