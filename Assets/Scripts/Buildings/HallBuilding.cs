using UnityEngine;

[AddComponentMenu("Buildings/HallBuilding")]
public class HallBuilding : TowerBuilding
{
    protected override void UpdateBuildingConstruction(int levelIndex)
    {
        base.UpdateBuildingConstruction(levelIndex);

        BuildConstruction(levelIndex);
	}

	protected override void BuildConstruction(int levelIndex)
    {
        base.BuildConstruction(levelIndex);

		HallBuildingLevelData hallLevelData = buildingLevelsData[levelIndex] as HallBuildingLevelData;

		if (hallLevelData.buildingConstruction)
			spawnedBuildingConstruction = Instantiate(hallLevelData.buildingConstruction, transform);

        spawnedBuildingConstruction.Build();
    }
}
