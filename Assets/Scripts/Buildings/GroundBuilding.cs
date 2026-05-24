using System;
using UnityEngine;

public class GroundBuilding : Building
{
    protected override void OnDemolish()
    {
        
    }

    protected override BuildingConstruction GetConstructionToSpawn()
    {
        var levelData = LevelData as GroundBuildingLevelData;

        return constructionComponent.IsUnderConstruction && levelData.ConstructionFrame ? levelData.ConstructionFrame : levelData.Construction;
    }
}