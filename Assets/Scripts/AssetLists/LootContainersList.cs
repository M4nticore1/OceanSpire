using UnityEngine;

[CreateAssetMenu(fileName = "LootContainersList", menuName = "GameContent/LootContainersList")]
public class LootContainersList : ScriptableObject
{
    [field: SerializeField] public LootContainer[] lootContainers;
}
