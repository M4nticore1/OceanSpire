using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ResourceToBuild
{
    public ItemData itemData;
    public int amount;
}

[CreateAssetMenu(fileName = "ConstructionLevelData", menuName = "Constructions Level Data/ConstructionLevelData")]
public class ConstructionLevelData : ScriptableObject
{
    [Header("Main")]
    [SerializeField] private List<ItemInstance> resourcesToBuild = new List<ItemInstance>();
    public List<ItemInstance> ResourcesToBuild => resourcesToBuild;
    public int maxResidentsCount = 0;
}
