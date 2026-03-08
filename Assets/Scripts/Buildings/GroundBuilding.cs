using System;
using UnityEngine;

[Serializable]
public class GroundBuildingEntry : BuildingEntry
{

}

public class GroundBuilding : Building
{
    protected override void OnInit(BuildingEntry saveData)
    {
        
    }

    protected override BuildingConstruction GetConstructionToSpawn()
    {
        return (LevelData as GroundBuildingLevelData).Construction;
    }
}
