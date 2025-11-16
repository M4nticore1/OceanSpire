using UnityEngine;

public class EnvironmetBuilding : Building
{
    protected override void BuildConstruction(int levelIndex)
    {
        base.BuildConstruction(levelIndex);

        EnvironmentBuildingLevelData environmentBuildingLevelData = buildingLevelsData[levelIndex] as EnvironmentBuildingLevelData;
        BuildingConstruction buildingConstruction = environmentBuildingLevelData.buildingConstruction;

        if (buildingConstruction)
        {
            spawnedBuildingConstruction = Instantiate(buildingConstruction, transform);
        }
    }
}
