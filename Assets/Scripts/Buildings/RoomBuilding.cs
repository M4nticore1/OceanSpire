using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Buildings/RoomBuilding")]
public class RoomBuilding : Building
{
    protected BuildingPosition buildingPosition = BuildingPosition.Straight;

    public RoomBuilding leftConnectedBuilding = null;
    public RoomBuilding rightConnectedBuilding = null;
    public RoomBuilding aboveConnectedBuilding = null;
    public RoomBuilding belowConnectedBuilding = null;

    protected override void UpdateBuildingConstruction(int levelIndex)
    {
        base.UpdateBuildingConstruction(levelIndex);

        if (buildingPlace)
        {
            if (GetPlaceIndex() % 2 == 0)
                buildingPosition = BuildingPosition.Corner;
            else
                buildingPosition = BuildingPosition.Straight;

            if (buildingData.connectionType == ConnectionType.None)
            {
                BuildConstruction(levelIndex);
            }
            else if (buildingData.connectionType == ConnectionType.Horizontal)
            {
                if (!leftConnectedBuilding)
                {
                    int leftRoomIndex = (GetPlaceIndex() + 1 + CityManager.roomsCountPerFloor) % (CityManager.roomsCountPerFloor - 1);
                    RoomBuilding neighboringRoom = cityManager.builtFloors[GetFloorIndex()].roomBuildingPlaces[leftRoomIndex].placedBuilding as RoomBuilding;
                    leftConnectedBuilding = neighboringRoom;

                    if (neighboringRoom && !neighboringRoom.isUnderConstruction && neighboringRoom.buildingData.buildingIdName == buildingData.buildingIdName && neighboringRoom.levelIndex == levelIndex)
                        neighboringRoom.BuildConstruction(levelIndex);
                }

                if (!rightConnectedBuilding)
                {
                    int rightRoomIndex = (GetPlaceIndex() - 1 + CityManager.roomsCountPerFloor) % (CityManager.roomsCountPerFloor - 1);
                    RoomBuilding neighboringRoom = cityManager.builtFloors[GetFloorIndex()].roomBuildingPlaces[rightRoomIndex].placedBuilding as RoomBuilding;
                    rightConnectedBuilding = neighboringRoom;

                    if (neighboringRoom && !neighboringRoom.isUnderConstruction && neighboringRoom.buildingData.buildingIdName == buildingData.buildingIdName && neighboringRoom.levelIndex == levelIndex)
                        neighboringRoom.BuildConstruction(levelIndex);
                }

                BuildConstruction(levelIndex);
            }
            else if (buildingData.connectionType == ConnectionType.Vertical)
            {
                if (!aboveConnectedBuilding && cityManager.builtFloors.Count > 0 && cityManager.builtFloors.Count > GetFloorIndex() + 1)
                {
                    RoomBuilding neighboringRoom = cityManager.builtFloors[GetFloorIndex() + 1].roomBuildingPlaces[GetPlaceIndex()].placedBuilding as RoomBuilding;
                    aboveConnectedBuilding = neighboringRoom;

                    if (neighboringRoom && !neighboringRoom.isUnderConstruction && neighboringRoom.buildingData.buildingIdName == buildingData.buildingIdName && neighboringRoom.levelIndex == levelIndex)
                        neighboringRoom.BuildConstruction(levelIndex);
                }

                if (!belowConnectedBuilding && GetFloorIndex() > 0)
                {
                    RoomBuilding neighboringRoom = cityManager.builtFloors[GetFloorIndex() - 1].roomBuildingPlaces[GetPlaceIndex()].placedBuilding as RoomBuilding;
                    belowConnectedBuilding = neighboringRoom;

                    if (neighboringRoom && !neighboringRoom.isUnderConstruction && neighboringRoom.buildingData.buildingIdName == buildingData.buildingIdName && neighboringRoom.levelIndex == levelIndex)
                        neighboringRoom.BuildConstruction(levelIndex);
                }

                BuildConstruction(levelIndex);
            }
        }
    }

    protected override void BuildConstruction(int levelIndex)
    {
        base.BuildConstruction(levelIndex);

        RoomBuildingLevelData roomLevelData = buildingLevelsData[levelIndex] as RoomBuildingLevelData;

        if (isUnderConstruction)
        {
            if (buildingPosition == BuildingPosition.Straight)
            {
                if (roomLevelData.constructionStraight)
                    spawnedBuildingConstruction = Instantiate(roomLevelData.constructionStraight, gameObject.transform);
            }
            else if (buildingPosition == BuildingPosition.Corner)
            {
                if (roomLevelData.constructionCorner)
                    spawnedBuildingConstruction = Instantiate(roomLevelData.constructionCorner, gameObject.transform);
            }
        }
        else
        {
            if (buildingData.connectionType == ConnectionType.None)
            {
                if (buildingPosition == BuildingPosition.Straight)
                {
                    if (roomLevelData.buildingStraight)
                        spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraight, gameObject.transform);
                }
                else if (buildingPosition == BuildingPosition.Corner)
                {
                    if (roomLevelData.buildingCorner)
                        spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCorner, gameObject.transform);
                }
            }
            else if (buildingData.connectionType == ConnectionType.Horizontal)
            {
                if (buildingPosition == BuildingPosition.Straight)
                {
                    if (leftConnectedBuilding && rightConnectedBuilding && roomLevelData.buildingStraightLeftRight)
                        spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraightLeftRight, gameObject.transform);
                    else if (leftConnectedBuilding && roomLevelData.buildingStraightLeft)
                        spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraightLeft, gameObject.transform);
                    else if (rightConnectedBuilding && roomLevelData.buildingStraightRight)
                        spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraightRight, gameObject.transform);
                    else if (!leftConnectedBuilding && !rightConnectedBuilding && roomLevelData.buildingStraight)
                        spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraight, gameObject.transform);
                }
                else if (buildingPosition == BuildingPosition.Corner)
                {
                    if (leftConnectedBuilding && rightConnectedBuilding && roomLevelData.buildingCornerLeftRight)
                        spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCornerLeftRight, gameObject.transform);
                    else if (leftConnectedBuilding && roomLevelData.buildingCornerLeft)
                        spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCornerLeft, gameObject.transform);
                    else if (rightConnectedBuilding && roomLevelData.buildingCornerRight)
                        spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCornerRight, gameObject.transform);
                    else if (!leftConnectedBuilding && !rightConnectedBuilding && roomLevelData.buildingCorner)
                        spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCorner, gameObject.transform);
                }
            }
            else if (buildingData.connectionType == ConnectionType.Vertical)
            {
                if (buildingPosition == BuildingPosition.Straight)
                {
                    if (aboveConnectedBuilding && belowConnectedBuilding && roomLevelData.buildingStraightAboveBelow)
                        spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraightAboveBelow, gameObject.transform);
                    else if (aboveConnectedBuilding && roomLevelData.buildingStraightAbove)
                        spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraightAbove, gameObject.transform);
                    else if (belowConnectedBuilding && roomLevelData.buildingStraightBelow)
                        spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraightBelow, gameObject.transform);
                    else if (!aboveConnectedBuilding && !belowConnectedBuilding && roomLevelData.buildingStraight)
                        spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraight, gameObject.transform);
                }
                else if (buildingPosition == BuildingPosition.Corner)
                {
                    if (aboveConnectedBuilding && belowConnectedBuilding && roomLevelData.buildingCornerAboveBelow)
                        spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCornerAboveBelow, gameObject.transform);
                    else if (aboveConnectedBuilding && roomLevelData.buildingCornerAbove)
                        spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCornerAbove, gameObject.transform);
                    else if (belowConnectedBuilding && roomLevelData.buildingCornerBelow)
                        spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCornerBelow, gameObject.transform);
                    else if (!aboveConnectedBuilding && !belowConnectedBuilding && roomLevelData.buildingCorner)
                        spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCorner, gameObject.transform);
                }
            }

            spawnedBuildingConstruction.Build();
        }

        if (isSelected)
            Select();
    }

    public override void EnterBuilding(Entity entity)
    {
        base.EnterBuilding(entity);
    }
}
