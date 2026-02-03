using System;
using UnityEngine;

[Serializable]
public class GroundBuildingEntry : BuildingEntry
{

}

public class GroundBuilding : Building
{
    protected override void Start()
    {
        
    }

    protected override void OnInit(BuildingEntry saveData)
    {
        
    }

    protected override BuildingConstruction GetConstruction()
    {
        return (LevelData as GroundBuildingLevelData).Construction;
    }
}
