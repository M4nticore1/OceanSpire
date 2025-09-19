using UnityEngine;

[AddComponentMenu("Buildings/EnvironmentBuilding")]
public class EnvironmentBuilding : Building
{
    protected override void UpdateBuildingConstruction()
    {
        BuildConstruction();
    }

    protected override void BuildConstruction()
    {
        base.BuildConstruction();

        EnvironmentBuildingLevelData environmentBuildingLevelData = buildingLevelsData[levelIndex] as EnvironmentBuildingLevelData;
        BuildingConstruction buildingConstruction = environmentBuildingLevelData.buildingConstruction;

        if (buildingConstruction)
        {
            spawnedBuildingConstruction = Instantiate(buildingConstruction, transform);
        }
    }
}
