using UnityEngine;

[AddComponentMenu("Buildings/EnvironmentBuilding")]
public class EnvironmentBuilding : Building
{
    protected override void UpdateBuildingConstruction(int levelIndex)
    {
        BuildConstruction(levelIndex);
    }

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
