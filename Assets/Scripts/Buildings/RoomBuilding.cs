using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Buildings/RoomBuilding")]
public class RoomBuilding : TowerBuilding
{
    protected BuildingPosition buildingPosition = BuildingPosition.Straight;

    protected override void UpdateBuildingConstruction(int levelIndex)
    {
        base.UpdateBuildingConstruction(levelIndex);

        if (buildingPlace)
        {
            if (GetPlaceIndex() % 2 == 0)
                buildingPosition = BuildingPosition.Corner;
            else
                buildingPosition = BuildingPosition.Straight;

            if (buildingData.ConnectionType == ConnectionType.None)
            {
                BuildConstruction(levelIndex);
            }
            else if (buildingData.ConnectionType == ConnectionType.Horizontal)
            {
                if (!leftConnectedBuilding)
                {
                    int leftRoomIndex = (GetPlaceIndex() + 1 + CityManager.roomsCountPerFloor) % (CityManager.roomsCountPerFloor - 1);
                    RoomBuilding neighboringRoom = cityManager.builtFloors[GetFloorIndex()].roomBuildingPlaces[leftRoomIndex].placedBuilding as RoomBuilding;
                    leftConnectedBuilding = neighboringRoom;

                    if (neighboringRoom && !neighboringRoom.constructionComponent.isUnderConstruction && neighboringRoom.buildingData.BuildingIdName == buildingData.BuildingIdName && neighboringRoom.levelComponent.levelIndex == levelIndex)
                        neighboringRoom.BuildConstruction(levelIndex);
                }

                if (!rightConnectedBuilding)
                {
                    int rightRoomIndex = (GetPlaceIndex() - 1 + CityManager.roomsCountPerFloor) % (CityManager.roomsCountPerFloor - 1);
                    RoomBuilding neighboringRoom = cityManager.builtFloors[GetFloorIndex()].roomBuildingPlaces[rightRoomIndex].placedBuilding as RoomBuilding;
                    rightConnectedBuilding = neighboringRoom;

                    if (neighboringRoom && !neighboringRoom.constructionComponent.isUnderConstruction && neighboringRoom.buildingData.BuildingIdName == buildingData.BuildingIdName && neighboringRoom.levelComponent.levelIndex == levelIndex)
                        neighboringRoom.BuildConstruction(levelIndex);
                }

                BuildConstruction(levelIndex);
            }
            else if (buildingData.ConnectionType == ConnectionType.Vertical)
            {
                if (!aboveConnectedBuilding && cityManager.builtFloors.Count > 0 && cityManager.builtFloors.Count > GetFloorIndex() + 1)
                {
                    RoomBuilding neighboringRoom = cityManager.builtFloors[GetFloorIndex() + 1].roomBuildingPlaces[GetPlaceIndex()].placedBuilding as RoomBuilding;
                    aboveConnectedBuilding = neighboringRoom;

                    if (neighboringRoom && !neighboringRoom.constructionComponent.isUnderConstruction && neighboringRoom.buildingData.BuildingIdName == buildingData.BuildingIdName && neighboringRoom.levelComponent.levelIndex == levelIndex)
                        neighboringRoom.BuildConstruction(levelIndex);
                }

                if (!belowConnectedBuilding && GetFloorIndex() > 0)
                {
                    RoomBuilding neighboringRoom = cityManager.builtFloors[GetFloorIndex() - 1].roomBuildingPlaces[GetPlaceIndex()].placedBuilding as RoomBuilding;
                    belowConnectedBuilding = neighboringRoom;

                    if (neighboringRoom && !neighboringRoom.constructionComponent.isUnderConstruction && neighboringRoom.buildingData.BuildingIdName == buildingData.BuildingIdName && neighboringRoom.levelComponent.levelIndex == levelIndex)
                        neighboringRoom.BuildConstruction(levelIndex);
                }

                BuildConstruction(levelIndex);
            }
        }
    }

    protected override void BuildConstruction(int levelIndex)
    {
        base.BuildConstruction(levelIndex);

        RoomLevelData roomLevelData = buildingLevelsData[levelIndex] as RoomLevelData;

        if (constructionComponent.isUnderConstruction)
        {
            if (buildingPosition == BuildingPosition.Straight)
            {
                if (roomLevelData.constructionStraight)
                    constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.constructionStraight, gameObject.transform);
            }
            else if (buildingPosition == BuildingPosition.Corner)
            {
                if (roomLevelData.constructionCorner)
                    constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.constructionCorner, gameObject.transform);
            }
        }
        else
        {
            if (buildingData.ConnectionType == ConnectionType.None)
            {
                if (buildingPosition == BuildingPosition.Straight)
                {
                    if (roomLevelData.buildingStraight)
                        constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraight, gameObject.transform);
                }
                else if (buildingPosition == BuildingPosition.Corner)
                {
                    if (roomLevelData.buildingCorner)
                        constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCorner, gameObject.transform);
                }
            }
            else if (buildingData.ConnectionType == ConnectionType.Horizontal)
            {
                if (buildingPosition == BuildingPosition.Straight)
                {
                    if (leftConnectedBuilding && rightConnectedBuilding && roomLevelData.buildingStraightLeftRight)
                        constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraightLeftRight, gameObject.transform);
                    else if (leftConnectedBuilding && roomLevelData.buildingStraightLeft)
                        constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraightLeft, gameObject.transform);
                    else if (rightConnectedBuilding && roomLevelData.buildingStraightRight)
                        constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraightRight, gameObject.transform);
                    else if (!leftConnectedBuilding && !rightConnectedBuilding && roomLevelData.buildingStraight)
                        constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraight, gameObject.transform);
                }
                else if (buildingPosition == BuildingPosition.Corner)
                {
                    if (leftConnectedBuilding && rightConnectedBuilding && roomLevelData.buildingCornerLeftRight)
                        constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCornerLeftRight, gameObject.transform);
                    else if (leftConnectedBuilding && roomLevelData.buildingCornerLeft)
                        constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCornerLeft, gameObject.transform);
                    else if (rightConnectedBuilding && roomLevelData.buildingCornerRight)
                        constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCornerRight, gameObject.transform);
                    else if (!leftConnectedBuilding && !rightConnectedBuilding && roomLevelData.buildingCorner)
                        constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCorner, gameObject.transform);
                }
            }
            else if (buildingData.ConnectionType == ConnectionType.Vertical)
            {
                if (buildingPosition == BuildingPosition.Straight)
                {
                    if (aboveConnectedBuilding && belowConnectedBuilding && roomLevelData.buildingStraightAboveBelow)
                        constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraightAboveBelow, gameObject.transform);
                    else if (aboveConnectedBuilding && roomLevelData.buildingStraightAbove)
                        constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraightAbove, gameObject.transform);
                    else if (belowConnectedBuilding && roomLevelData.buildingStraightBelow)
                        constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraightBelow, gameObject.transform);
                    else if (!aboveConnectedBuilding && !belowConnectedBuilding && roomLevelData.buildingStraight)
                        constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraight, gameObject.transform);
                }
                else if (buildingPosition == BuildingPosition.Corner)
                {
                    if (aboveConnectedBuilding && belowConnectedBuilding && roomLevelData.buildingCornerAboveBelow)
                        constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCornerAboveBelow, gameObject.transform);
                    else if (aboveConnectedBuilding && roomLevelData.buildingCornerAbove)
                        constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCornerAbove, gameObject.transform);
                    else if (belowConnectedBuilding && roomLevelData.buildingCornerBelow)
                        constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCornerBelow, gameObject.transform);
                    else if (!aboveConnectedBuilding && !belowConnectedBuilding && roomLevelData.buildingCorner)
                        constructionComponent.spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCorner, gameObject.transform);
                }
            }

            constructionComponent.spawnedBuildingConstruction.Build();
        }

        if (isSelected)
            selectComponent.Select();
    }

    public override void EnterBuilding(Entity entity)
    {
        base.EnterBuilding(entity);
    }
}
