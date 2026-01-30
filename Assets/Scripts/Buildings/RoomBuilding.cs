using UnityEngine;

[AddComponentMenu("Buildings/RoomBuilding")]
public class RoomBuilding : TowerBuilding
{
    protected override void BuildConstruction(int levelIndex)
    {
        if (LevelData is RoomLevelData roomLevelData) {
            if (ConstructionComponent.isUnderConstruction) {
                if (buildingPosition == BuildingPosition.Straight)  {
                    if (roomLevelData.ConstructionStraight)
                        ConstructionComponent.BuildConstruction(roomLevelData.ConstructionStraight);
                }
                else if (buildingPosition == BuildingPosition.Corner) {
                    if (roomLevelData.ConstructionCorner)
                        ConstructionComponent.BuildConstruction(roomLevelData.ConstructionCorner);
                }
            }
            else {
                if (buildingData.ConnectionType == ConnectionType.None) {
                    if (buildingPosition == BuildingPosition.Straight) {
                        if (roomLevelData.ConstructionStraight)
                            ConstructionComponent.BuildConstruction(roomLevelData.ConstructionStraight);
                    }
                    else if (buildingPosition == BuildingPosition.Corner) {
                        if (roomLevelData.ConstructionCorner)
                            ConstructionComponent.BuildConstruction(roomLevelData.ConstructionCorner);
                    }
                }
                else if (buildingData.ConnectionType == ConnectionType.Horizontal) {
                    if (buildingPosition == BuildingPosition.Straight) {
                        if (leftNeighborBuilding && rightNeighborBuilding && roomLevelData.ConstructionStraightLeftRight)
                            ConstructionComponent.BuildConstruction(roomLevelData.ConstructionStraightLeftRight);
                        else if (leftNeighborBuilding && roomLevelData.ConstructionStraightLeft)
                            ConstructionComponent.BuildConstruction(roomLevelData.ConstructionStraightLeft);
                        else if (rightNeighborBuilding && roomLevelData.ConstructionStraightRight)
                            ConstructionComponent.BuildConstruction(roomLevelData.ConstructionStraightRight);
                        else if (!leftNeighborBuilding && !rightNeighborBuilding && roomLevelData.ConstructionStraight)
                            ConstructionComponent.BuildConstruction(roomLevelData.ConstructionStraight);
                    }
                    else if (buildingPosition == BuildingPosition.Corner) {
                        if (leftNeighborBuilding && rightNeighborBuilding && roomLevelData.ConstructionCornerLeftRight)
                            ConstructionComponent.BuildConstruction(roomLevelData.ConstructionCornerLeftRight);
                        else if (leftNeighborBuilding && roomLevelData.ConstructionCornerLeft)
                            ConstructionComponent.BuildConstruction(roomLevelData.ConstructionCornerLeft);
                        else if (rightNeighborBuilding && roomLevelData.ConstructionCornerRight)
                            ConstructionComponent.BuildConstruction(roomLevelData.ConstructionCornerRight);
                        else if (!leftNeighborBuilding && !rightNeighborBuilding && roomLevelData.ConstructionCorner)
                            ConstructionComponent.BuildConstruction(roomLevelData.ConstructionCorner);
                    }
                }
                else if (buildingData.ConnectionType == ConnectionType.Vertical) {
                    if (buildingPosition == BuildingPosition.Straight) {
                        if (upNeighborBuilding && downNeighborBuilding && roomLevelData.ConstructionStraightAboveBelow)
                            ConstructionComponent.BuildConstruction(roomLevelData.ConstructionStraightAboveBelow);
                        else if (upNeighborBuilding && roomLevelData.ConstructionStraightAbove)
                            ConstructionComponent.BuildConstruction(roomLevelData.ConstructionStraightAbove);
                        else if (downNeighborBuilding && roomLevelData.ConstructionStraightBelow)
                            ConstructionComponent.BuildConstruction(roomLevelData.ConstructionStraightBelow);
                        else if (!upNeighborBuilding && !downNeighborBuilding && roomLevelData.ConstructionStraight)
                            ConstructionComponent.BuildConstruction(roomLevelData.ConstructionStraight);
                    }
                    else if (buildingPosition == BuildingPosition.Corner) {
                        if (upNeighborBuilding && downNeighborBuilding && roomLevelData.ConstructionCornerAboveBelow)
                            ConstructionComponent.BuildConstruction(roomLevelData.ConstructionCornerAboveBelow);
                        else if (upNeighborBuilding && roomLevelData.ConstructionCornerAbove)
                            ConstructionComponent.BuildConstruction(roomLevelData.ConstructionCornerAbove);
                        else if (downNeighborBuilding && roomLevelData.ConstructionCornerBelow)
                            ConstructionComponent.BuildConstruction(roomLevelData.ConstructionCornerBelow);
                        else if (!upNeighborBuilding && !downNeighborBuilding && roomLevelData.ConstructionCorner)
                            ConstructionComponent.BuildConstruction(roomLevelData.ConstructionCorner);
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
        return neightboorBuilding && !neightboorBuilding.ConstructionComponent.isUnderConstruction && neightboorBuilding.BuildingData.BuildingIdName == buildingData.BuildingIdName && neightboorBuilding.LevelIndex == levelIndex;
    }
}
