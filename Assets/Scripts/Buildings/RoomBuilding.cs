using UnityEngine;

[AddComponentMenu("Buildings/RoomBuilding")]
public class RoomBuilding : TowerBuilding
{
    //protected BuildingPosition buildingPosition = BuildingPosition.Straight;

    public override void BuildConstruction(int levelIndex)
    {
        //RoomLevelData roomLevelData = buildingLevelsData[levelIndex] as RoomLevelData;

        if (LevelData is RoomLevelData roomLevelData)
        {
            if (constructionComponent.isUnderConstruction)
            {
                if (buildingPosition == BuildingPosition.Straight)
                {
                    if (roomLevelData.ConstructionStraight)
                        constructionComponent.BuildConstruction(roomLevelData.ConstructionStraight);
                }
                else if (buildingPosition == BuildingPosition.Corner)
                {
                    if (roomLevelData.ConstructionCorner)
                        constructionComponent.BuildConstruction(roomLevelData.ConstructionCorner);
                }
            }
            else
            {
                if (buildingData.ConnectionType == ConnectionType.None)
                {
                    if (buildingPosition == BuildingPosition.Straight)
                    {
                        if (roomLevelData.ConstructionStraight)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionStraight);
                    }
                    else if (buildingPosition == BuildingPosition.Corner)
                    {
                        if (roomLevelData.ConstructionCorner)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionCorner);
                    }
                }
                else if (buildingData.ConnectionType == ConnectionType.Horizontal)
                {
                    if (buildingPosition == BuildingPosition.Straight)
                    {
                        if (leftNeighborBuilding && rightNeighborBuilding && roomLevelData.ConstructionStraightLeftRight)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionStraightLeftRight);
                        else if (leftNeighborBuilding && roomLevelData.ConstructionStraightLeft)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionStraightLeft);
                        else if (rightNeighborBuilding && roomLevelData.ConstructionStraightRight)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionStraightRight);
                        else if (!leftNeighborBuilding && !rightNeighborBuilding && roomLevelData.ConstructionStraight)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionStraight);
                    }
                    else if (buildingPosition == BuildingPosition.Corner)
                    {
                        if (leftNeighborBuilding && rightNeighborBuilding && roomLevelData.ConstructionCornerLeftRight)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionCornerLeftRight);
                        else if (leftNeighborBuilding && roomLevelData.ConstructionCornerLeft)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionCornerLeft);
                        else if (rightNeighborBuilding && roomLevelData.ConstructionCornerRight)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionCornerRight);
                        else if (!leftNeighborBuilding && !rightNeighborBuilding && roomLevelData.ConstructionCorner)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionCorner);
                    }
                }
                else if (buildingData.ConnectionType == ConnectionType.Vertical)
                {
                    //Debug.Log(GetfloorIndex + " " + GetplaceIndex);
                    if (buildingPosition == BuildingPosition.Straight)
                    {
                        if (upNeighborBuilding && downNeighborBuilding && roomLevelData.ConstructionStraightAboveBelow)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionStraightAboveBelow);
                        else if (upNeighborBuilding && roomLevelData.ConstructionStraightAbove)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionStraightAbove);
                        else if (downNeighborBuilding && roomLevelData.ConstructionStraightBelow)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionStraightBelow);
                        else if (!upNeighborBuilding && !downNeighborBuilding && roomLevelData.ConstructionStraight)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionStraight);
                    }
                    else if (buildingPosition == BuildingPosition.Corner)
                    {
                        if (upNeighborBuilding && downNeighborBuilding && roomLevelData.ConstructionCornerAboveBelow)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionCornerAboveBelow);
                        else if (upNeighborBuilding && roomLevelData.ConstructionCornerAbove)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionCornerAbove);
                        else if (downNeighborBuilding && roomLevelData.ConstructionCornerBelow)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionCornerBelow);
                        else if (!upNeighborBuilding && !downNeighborBuilding && roomLevelData.ConstructionCorner)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionCorner);
                    }
                }
            }

            if (IsSelected)
                Select();
        }
    }

    public override void EnterBuilding(Creature entity)
    {
        base.EnterBuilding(entity);
    }

    private bool GetPossibilityConnect(Building neightboorBuilding, int levelIndex)
    {
        return neightboorBuilding && !neightboorBuilding.constructionComponent.isUnderConstruction && neightboorBuilding.BuildingData.BuildingIdName == buildingData.BuildingIdName && neightboorBuilding.LevelIndex == levelIndex;
    }
}
