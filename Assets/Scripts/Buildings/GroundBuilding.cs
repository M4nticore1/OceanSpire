using System;
using UnityEngine;

public class GroundBuilding : Building
{
    protected override void OnDemolish()
    {
        
    }

    protected override BuildingConstruction GetConstructionToSpawn()
    {
        var levelData = LevelDefinition as GroundBuildingLevelData;

        return constructionComponent.GetUnderConstruction() && levelData.ConstructionFrame ? levelData.ConstructionFrame : levelData.Construction;
    }
}