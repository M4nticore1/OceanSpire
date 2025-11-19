using Mono.Cecil.Cil;
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

        ConstructionLevelData levelData = buildingLevelsData[levelIndex];

		if (levelData)
        {
            BuildingConstruction construction = Instantiate(levelData.ConstructionStraight, transform);
            constructionComponent.SetConstruction(construction);
        }
    }
}
