using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Buildings/RoomBuilding")]
public class RoomBuilding : Building
{
    protected BuildingPosition buildingPosition = BuildingPosition.Straight;
    [HideInInspector] public bool isConnectedLeft = false;
    [HideInInspector] public bool isConnectedRight = false;
    [HideInInspector] public bool isConnectedAbove = false;
    [HideInInspector] public bool isConnectedBelow = false;

    [HideInInspector] public RoomBuilding leftConnectedBuilding = null;
    [HideInInspector] public RoomBuilding rightConnectedBuilding = null;
    [HideInInspector] public RoomBuilding aboveConnectedBuilding = null;
    [HideInInspector] public RoomBuilding belowConnectedBuilding = null;

    protected override void Start()
    {
        base.Start();
    }

    public override void Place(BuildingPlace buildingPlace)
    {
        base.Place(buildingPlace);

        if (GetType() == typeof(RoomBuilding))
            InvokeBuildingPlaced(this);
    }

    protected override void UpdateBuildingConstruction()
    {
        base.UpdateBuildingConstruction();

        if (buildingIndex % 2 == 0)
        {
            buildingPosition = BuildingPosition.Corner;
        }
        else
        {
            buildingPosition = BuildingPosition.Straight;
        }

        if (buildingData.connectionType == ConnectionType.Horizontal)
        {
            // Check the left room
            RoomBuilding leftRoom = null;

            if (buildingIndex < CityManager.roomsCountPerFloor - 1)
            {
                leftRoom = cityManager.spawnedFloors[floorIndex].roomBuildingPlaces[buildingIndex + 1].placedBuilding as RoomBuilding;
            }
            else if (buildingIndex == CityManager.roomsCountPerFloor - 1)
            {
                leftRoom = cityManager.spawnedFloors[floorIndex].roomBuildingPlaces[0].placedBuilding as RoomBuilding;
            }

            if (leftRoom && leftRoom.buildingData.buildingIdName == buildingData.buildingIdName && leftRoom.levelIndex == levelIndex)
            {
                isConnectedLeft = true;
                leftConnectedBuilding = leftRoom;
                leftRoom.isConnectedRight = true;
                leftRoom.BuildConstruction();
            }

            // Check the Right room
            RoomBuilding rightRoom = null;

            if (buildingIndex > 0)
            {
                rightRoom = cityManager.spawnedFloors[floorIndex].roomBuildingPlaces[buildingIndex - 1].placedBuilding as RoomBuilding;
            }
            else if (buildingIndex == 0)
            {
                rightRoom = cityManager.spawnedFloors[floorIndex].roomBuildingPlaces[CityManager.roomsCountPerFloor - 1].placedBuilding as RoomBuilding;
            }

            if (rightRoom && rightRoom.buildingData.buildingIdName == buildingData.buildingIdName && rightRoom.levelIndex == levelIndex)
            {
                isConnectedRight = true;
                rightConnectedBuilding = rightRoom;
                rightRoom.isConnectedLeft = true;
                rightRoom.BuildConstruction();
            }
        }
        else if (buildingData.connectionType == ConnectionType.Vertical)
        {
            RoomBuilding topRoom = null;

            if (floorIndex < cityManager.builtFloorsCount - 1)
            {
                topRoom = cityManager.spawnedFloors[floorIndex + 1].roomBuildingPlaces[buildingIndex].placedBuilding as RoomBuilding;
            }

            if (topRoom && topRoom.buildingData.buildingIdName == buildingData.buildingIdName)
            {
                if (topRoom.levelIndex == levelIndex)
                {
                    isConnectedAbove = true;
                    aboveConnectedBuilding = topRoom;
                    topRoom.isConnectedBelow = true;
                    topRoom.BuildConstruction();
                }
                else
                {
                    if (topRoom.isConnectedBelow)
                    {
                        topRoom.isConnectedBelow = false;
                        topRoom.BuildConstruction();
                    }
                }
            }

            RoomBuilding belowRoom = null;

            if (floorIndex > 0)
            {
                belowRoom = cityManager.spawnedFloors[floorIndex - 1].roomBuildingPlaces[buildingIndex].placedBuilding as RoomBuilding;
            }

            if (belowRoom && belowRoom.buildingData.buildingIdName == buildingData.buildingIdName)
            {
                if (belowRoom.levelIndex == levelIndex)
                {
                    isConnectedBelow = true;
                    belowConnectedBuilding = belowRoom;
                    belowRoom.isConnectedAbove = true;
                    belowRoom.BuildConstruction();
                }
                else
                {
                    if (belowRoom.isConnectedAbove)
                    {
                        belowRoom.isConnectedAbove = false;
                        belowRoom.BuildConstruction();
                    }
                }
            }
        }

        BuildConstruction();
    }

    protected override void BuildConstruction()
    {
        base.BuildConstruction();

        RoomBuildingLevelData roomLevelData = buildingLevelsData[levelIndex] as RoomBuildingLevelData;

        if (buildingData.connectionType == ConnectionType.Horizontal)
        {
            if (buildingPosition == BuildingPosition.Straight)
            {
                if (isConnectedLeft && isConnectedRight && roomLevelData.buildingStraightLeftRight)
                    spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraightLeftRight, gameObject.transform);
                else if (isConnectedLeft && roomLevelData.buildingStraightLeft)
                    spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraightLeft, gameObject.transform);
                else if (isConnectedRight && roomLevelData.buildingStraightRight)
                    spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraightRight, gameObject.transform);
                else if (!isConnectedLeft && !isConnectedRight && roomLevelData.buildingStraight)
                    spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraight, gameObject.transform);
            }
            else if (buildingPosition == BuildingPosition.Corner)
            {
                if (isConnectedLeft && isConnectedRight && roomLevelData.buildingCornerLeftRight)
                    spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCornerLeftRight, gameObject.transform);
                else if (isConnectedLeft && roomLevelData.buildingCornerLeft)
                    spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCornerLeft, gameObject.transform);
                else if (isConnectedRight && roomLevelData.buildingCornerRight)
                    spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCornerRight, gameObject.transform);
                else if (!isConnectedLeft && !isConnectedRight && roomLevelData.BuildingCorner)
                    spawnedBuildingConstruction = Instantiate(roomLevelData.BuildingCorner, gameObject.transform);
            }
        }
        else if (buildingData.connectionType == ConnectionType.Vertical)
        {
            if (buildingPosition == BuildingPosition.Straight)
            {
                if (isConnectedAbove && isConnectedBelow && roomLevelData.buildingStraightAboveBelow)
                    spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraightAboveBelow, gameObject.transform);
                else if (isConnectedAbove && roomLevelData.buildingStraightAbove)
                    spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraightAbove, gameObject.transform);
                else if (isConnectedBelow && roomLevelData.buildingStraightBelow)
                    spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraightBelow, gameObject.transform);
                else if (!isConnectedAbove && !isConnectedBelow && roomLevelData.buildingStraight)
                    spawnedBuildingConstruction = Instantiate(roomLevelData.buildingStraight, gameObject.transform);
            }
            else if (buildingPosition == BuildingPosition.Corner)
            {
                if (isConnectedAbove && isConnectedBelow && roomLevelData.buildingCornerAboveBelow)
                    spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCornerAboveBelow, gameObject.transform);
                else if (isConnectedAbove && roomLevelData.buildingCornerAbove)
                    spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCornerAbove, gameObject.transform);
                else if (isConnectedBelow && roomLevelData.buildingCornerBelow)
                    spawnedBuildingConstruction = Instantiate(roomLevelData.buildingCornerBelow, gameObject.transform);
                else if (!isConnectedAbove && !isConnectedBelow && roomLevelData.BuildingCorner)
                    spawnedBuildingConstruction = Instantiate(roomLevelData.BuildingCorner, gameObject.transform);
            }
        }

        spawnedBuildingConstruction.Build();
    }

    public override void EnterBuilding(Entity entity)
    {
        base.EnterBuilding(entity);
    }

    //public override void Upgrade()
    //{
    //    base.Upgrade();

    //    //SetBuildingConstruction();
    //    //BuildConstruction();
    //}

    //public override void Demolish()
    //{
    //    base.Demolish();
    //}
}
