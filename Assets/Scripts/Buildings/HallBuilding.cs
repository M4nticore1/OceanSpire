using UnityEngine;

[AddComponentMenu("Buildings/HallBuilding")]
public class HallBuilding : Building
{
    protected override void Start()
    {
        base.Start();

	}

    protected override void UpdateBuildingConstruction()
    {
        base.UpdateBuildingConstruction();

        BuildConstruction();
	}

	protected override void BuildConstruction()
    {
        base.BuildConstruction();

		HallBuildingLevelData hallLevelData = buildingLevelsData[levelIndex] as HallBuildingLevelData;

		if (hallLevelData.buildingConstruction)
			spawnedBuildingConstruction = Instantiate(hallLevelData.buildingConstruction, transform);

        spawnedBuildingConstruction.Build();
    }
}
