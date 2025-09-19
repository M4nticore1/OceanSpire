using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[AddComponentMenu("BuildingComponents/StorageBuildingComponent")]
public class StorageBuildingComponent : BuildingComponent
{
    public List<StorageBuildingLevelData> levelsData = new List<StorageBuildingLevelData>();

    protected override void Awake()
    {
        base.Awake();

    }

    public override void Build()
    {
        base.Build();

        AddStorageCapacity(levelsData[ownedBuilding.levelIndex]);
    }

    public override void LevelUp()
    {
        base.LevelUp();

        int currentLevel = ownedBuilding.levelIndex;
        AddStorageCapacity(levelsData[currentLevel]);

        int previousLevel = ownedBuilding.levelIndex - 1;
        SubtractStorageCapacity(levelsData[previousLevel]);
    }

    private void AddStorageCapacity(StorageBuildingLevelData levelData)
    {
		cityManager.AddStorageCapacity(levelData);
    }

    private void SubtractStorageCapacity(StorageBuildingLevelData levelData)
    {
		cityManager.SubtractStorageCapacity(levelData);
    }

    //private void ChangeItemCapacity(int level, bool isIncreasing)
    //{
    //    for (int i = 0; i < levelsData[level].storageItems.Count(); i++)
    //    {
    //        int changeValue = levelsData[level].storageItems[i].capacity;

    //        cityManager.items[cityManager.GetItemIndexByIdName(levelsData[level].storageItems[i].itemdata.itemIdName)].maxAmount += isIncreasing ? changeValue : -changeValue;
    //    }

    //    for (int i = 0; i < levelsData[level].storageItemCategories.Count(); i++)
    //    {
    //        for (int j = 0; j < cityManager.items.Count(); j++)
    //        {
    //            if (cityManager.items[j].itemData.itemCategory == levelsData[level].storageItemCategories[i].itemCategory)
    //            {
    //                int changeValue = levelsData[level].storageItemCategories[i].capacity;

    //                cityManager.items[j].maxAmount += isIncreasing ? changeValue : -changeValue;
    //            }
    //        }
    //    }
    //}
}
