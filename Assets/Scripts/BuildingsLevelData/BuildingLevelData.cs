using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ResourceToBuild
{
    public ItemData itemData;
    public int amount;
}

[CreateAssetMenu(fileName = "BuildingLevelData", menuName = "Scriptable Objects/BuildingLevelData")]
public class BuildingLevelData : ScriptableObject
{
    [Header("Main")]
    public List<ItemEntry> resourcesToBuild = new List<ItemEntry>();
    public int maxResidentsCount = 0;

    public BuildingConstruction constructionStraight = null;
    public BuildingConstruction constructionCorner = null;
}
