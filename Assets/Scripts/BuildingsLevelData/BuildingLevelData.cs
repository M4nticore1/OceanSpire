using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ResourceToBuild
{
    public ItemDefinition itemData;
    public int amount;
}

public abstract class BuildingLevelData : ScriptableObject
{
    [Header("Building")]
    [SerializeField] private ItemInstance[] resourcesToBuild;
    public ItemInstance[] ResourcesToBuild => resourcesToBuild;

    [SerializeField] private int maxResidentsCount = 0;
    public int MaxHumansCount => maxResidentsCount;

    [SerializeField] private int constructionTime;
    public int UpgradeTime => constructionTime;
}
