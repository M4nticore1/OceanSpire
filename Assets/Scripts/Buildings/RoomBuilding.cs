//using UnityEngine;

//[AddComponentMenu("Buildings/RoomBuilding")]
//public class RoomBuilding : TowerBuilding
//{
//    protected override BuildingConstruction GetConstruction()
//    {
//        if (LevelData is RoomLevelData roomLevelData) {
//            if (buildingData.ConnectionType == ConnectionType.None) {
//                if (buildingPosition == BuildingPosition.Straight) {
//                    if (roomLevelData.ConstructionStraight)
//                        return roomLevelData.ConstructionStraight;
//                }
//                else if (buildingPosition == BuildingPosition.Corner) {
//                    if (roomLevelData.ConstructionCorner)
//                        return roomLevelData.ConstructionCorner;
//                }
//            }
//            else if (buildingData.ConnectionType == ConnectionType.Horizontal) {
//                if (buildingPosition == BuildingPosition.Straight) {
//                    if (leftNeighborBuilding && rightNeighborBuilding && roomLevelData.ConstructionStraightLeftRight)
//                        return roomLevelData.ConstructionStraightLeftRight;
//                    else if (leftNeighborBuilding && roomLevelData.ConstructionStraightLeft)
//                        return roomLevelData.ConstructionStraightLeft;
//                    else if (rightNeighborBuilding && roomLevelData.ConstructionStraightRight)
//                        return roomLevelData.ConstructionStraightRight;
//                    else if (!leftNeighborBuilding && !rightNeighborBuilding && roomLevelData.ConstructionStraight)
//                        return roomLevelData.ConstructionStraight;
//                }
//                else if (buildingPosition == BuildingPosition.Corner) {
//                    if (leftNeighborBuilding && rightNeighborBuilding && roomLevelData.ConstructionCornerLeftRight)
//                        return roomLevelData.ConstructionCornerLeftRight;
//                    else if (leftNeighborBuilding && roomLevelData.ConstructionCornerLeft)
//                        return roomLevelData.ConstructionCornerLeft;
//                    else if (rightNeighborBuilding && roomLevelData.ConstructionCornerRight)
//                        return roomLevelData.ConstructionCornerRight;
//                    else if (!leftNeighborBuilding && !rightNeighborBuilding && roomLevelData.ConstructionCorner)
//                        return roomLevelData.ConstructionCorner;
//                }
//            }
//            else if (buildingData.ConnectionType == ConnectionType.Vertical) {
//                if (buildingPosition == BuildingPosition.Straight) {
//                    if (upNeighborBuilding && downNeighborBuilding && roomLevelData.ConstructionStraightAboveBelow)
//                        return roomLevelData.ConstructionStraightAboveBelow;
//                    else if (upNeighborBuilding && roomLevelData.ConstructionStraightAbove)
//                        return roomLevelData.ConstructionStraightAbove;
//                    else if (downNeighborBuilding && roomLevelData.ConstructionStraightBelow)
//                        return roomLevelData.ConstructionStraightBelow;
//                    else if (!upNeighborBuilding && !downNeighborBuilding && roomLevelData.ConstructionStraight)
//                        return roomLevelData.ConstructionStraight;
//                }
//                else if (buildingPosition == BuildingPosition.Corner) {
//                    if (upNeighborBuilding && downNeighborBuilding && roomLevelData.ConstructionCornerAboveBelow)
//                        return roomLevelData.ConstructionCornerAboveBelow;
//                    else if (upNeighborBuilding && roomLevelData.ConstructionCornerAbove)
//                        return roomLevelData.ConstructionCornerAbove;
//                    else if (downNeighborBuilding && roomLevelData.ConstructionCornerBelow)
//                        return roomLevelData.ConstructionCornerBelow;
//                    else if (!upNeighborBuilding && !downNeighborBuilding && roomLevelData.ConstructionCorner)
//                        return roomLevelData.ConstructionCorner;
//                }
//            }
//        }
//        else {
//            Debug.LogError("LevelData is ");
//        }
//    }

//    public override void EnterBuilding(Creature entity)
//    {
//        base.EnterBuilding(entity);
//    }

//    private bool GetPossibilityConnect(Building neightboorBuilding, int levelIndex)
//    {
//        return neightboorBuilding && neightboorBuilding.BuildingData.BuildingIdName == buildingData.BuildingIdName && neightboorBuilding.LevelIndex == levelIndex;
//    }
//}
