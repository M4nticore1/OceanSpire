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
    public List<ItemInstance> resourcesToBuild = new List<ItemInstance>();
    public Dictionary<int, ItemInstance> resourcesToBuildDict = new Dictionary<int, ItemInstance>();
    public int maxResidentsCount = 0;

    public BuildingConstruction constructionStraight = null;
    public BuildingConstruction constructionCorner = null;

    private void Awake()
    {
        for (int i = 0; i < resourcesToBuild.Count; i++)
        {
            resourcesToBuildDict.Add(resourcesToBuild[i].ItemData.ItemId, resourcesToBuild[i]);
        }
    }
}
