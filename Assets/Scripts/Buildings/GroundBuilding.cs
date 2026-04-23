using System;
using UnityEngine;

public class GroundBuilding : Building
{
    protected override void OnInit(BuildingData saveData)
    {
        
    }

    protected override BuildingConstruction GetConstructionToSpawn()
    {
        GroundBuildingLevelData levelData = LevelData as GroundBuildingLevelData;
        return constructionComponent.IsUnderConstruction ? levelData.ConstructionFrame : levelData.Construction;
    }
}
