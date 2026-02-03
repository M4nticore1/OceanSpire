using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ResourceToBuild
{
    public ItemData itemData;
    public int amount;
}

public abstract class BuildingLevelData : ScriptableObject
{
    [Header("Main")]
    [SerializeField] private ItemInstance[] resourcesToBuild;
    public ItemInstance[] ResourcesToBuild => resourcesToBuild;
    public int maxResidentsCount = 0;
}
