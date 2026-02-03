using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingPosition
{
    Straight,
    Corner
}

public enum Side
{
    Left,
    Right,
    Up,
    Down
}

[System.Flags]
public enum NeighborMask
{
    None = 0,
    Left = 1 << 0,
    Right = 1 << 1,
    Up = 1 << 2,
    Down = 1 << 3,

    Horizontal = Left | Right,
    Vertical = Up | Down,
    All = Horizontal | Vertical
}

[Serializable]
public class TowerBuildingEntry : BuildingEntry
{
    public int floorIndex = 0;
    public int placeIndex = 0;
}

public class TowerBuilding : Building
{
    public BuildingPosition buildingPosition { get; private set; } = BuildingPosition.Straight;
    public int floorIndex = 0;
    public int placeIndex = 0;

    public TowerBuilding leftNeighborBuilding;
    public TowerBuilding rightNeighborBuilding;
    public TowerBuilding upNeighborBuilding;
    public TowerBuilding downNeighborBuilding;

    public TowerBuilding leftConnectedBuilding => CheckConnectionPossibility(leftNeighborBuilding, ConnectionType.Horizontal);
    public TowerBuilding rightConnectedBuilding => CheckConnectionPossibility(rightNeighborBuilding, ConnectionType.Horizontal);
    public TowerBuilding upConnectedBuilding => CheckConnectionPossibility(upNeighborBuilding, ConnectionType.Vertical);
    public TowerBuilding downConnectedBuilding => CheckConnectionPossibility(downNeighborBuilding, ConnectionType.Vertical);

    public IEnumerable NeighborBuildings(NeighborMask mask)
    {
        if (mask.HasFlag(NeighborMask.Left)) {
            yield return leftNeighborBuilding;
        }
        if (mask.HasFlag(NeighborMask.Right)) {
            yield return rightNeighborBuilding;
        }
        if (mask.HasFlag(NeighborMask.Up)) {
            yield return upNeighborBuilding;
        }
        if (mask.HasFlag(NeighborMask.Down)) {
            yield return downNeighborBuilding;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        EventBus.onBuildingPlaced += OnConstructionPlaced;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        EventBus.onBuildingPlaced -= OnConstructionPlaced;
    }

    protected override void Start()
    {

    }

    protected override void OnInit(BuildingEntry data)
    {
        TowerBuildingEntry towerData = data as TowerBuildingEntry;
        floorIndex = towerData.floorIndex;
        placeIndex = towerData.placeIndex;

        BuildingPlace place = null;
        if (BuildingData.BuildingType == BuildingType.Room)
            place = CityManager.Instance.BuiltFloors[towerData.floorIndex].roomBuildingPlaces[towerData.placeIndex];
        if (BuildingData.BuildingType == BuildingType.Hall)
            place = CityManager.Instance.BuiltFloors[towerData.floorIndex].hallBuildingPlace;
        else if (BuildingData.BuildingType == BuildingType.FloorFrame)
            place = CityManager.Instance.BuiltFloors[towerData.floorIndex].floorBuildingPlace;

        GetAllNeighborBuildings();

        if (placeIndex % 2 == 0)
            buildingPosition = BuildingPosition.Corner;
        else
            buildingPosition = BuildingPosition.Straight;
    }

    protected override BuildingConstruction GetConstruction()
    {
        BuildingConstruction construction = null;
        if (LevelData is TowerBuildingLevelData levelData) {
            if (buildingData.ConnectionType == ConnectionType.None) {
                if (buildingPosition == BuildingPosition.Straight) {
                    if (levelData.ConstructionStraight)
                        construction = levelData.ConstructionStraight;
                }
                else if (buildingPosition == BuildingPosition.Corner) {
                    if (levelData.ConstructionCorner)
                        construction = levelData.ConstructionCorner;
                }
            }
            else if (buildingData.ConnectionType == ConnectionType.Horizontal) {
                if (buildingPosition == BuildingPosition.Straight) {
                    if (leftConnectedBuilding && rightConnectedBuilding && levelData.ConstructionStraightLeftRight)
                        construction = levelData.ConstructionStraightLeftRight;
                    else if (leftConnectedBuilding && levelData.ConstructionStraightLeft)
                        construction = levelData.ConstructionStraightLeft;
                    else if (rightConnectedBuilding && levelData.ConstructionStraightRight)
                        construction = levelData.ConstructionStraightRight;
                    else if (!leftConnectedBuilding && !rightConnectedBuilding && levelData.ConstructionStraight)
                        construction = levelData.ConstructionStraight;
                }
                else if (buildingPosition == BuildingPosition.Corner) {
                    if (leftConnectedBuilding && rightConnectedBuilding && levelData.ConstructionCornerLeftRight)
                        construction = levelData.ConstructionCornerLeftRight;
                    else if (leftConnectedBuilding && levelData.ConstructionCornerLeft)
                        construction = levelData.ConstructionCornerLeft;
                    else if (rightConnectedBuilding && levelData.ConstructionCornerRight)
                        construction = levelData.ConstructionCornerRight;
                    else if (!leftConnectedBuilding && !rightConnectedBuilding && levelData.ConstructionCorner)
                        construction = levelData.ConstructionCorner;
                }
            }
            else if (buildingData.ConnectionType == ConnectionType.Vertical) {
                if (buildingPosition == BuildingPosition.Straight) {
                    if (upConnectedBuilding && downConnectedBuilding && levelData.ConstructionStraightUpDown)
                        construction = levelData.ConstructionStraightUpDown;
                    else if (upConnectedBuilding && levelData.ConstructionStraightUp)
                        construction = levelData.ConstructionStraightUp;
                    else if (downConnectedBuilding && levelData.ConstructionStraightDown)
                        construction = levelData.ConstructionStraightDown;
                    else if (!upConnectedBuilding && !downConnectedBuilding && levelData.ConstructionStraight)
                        construction = levelData.ConstructionStraight;
                }
                else if (buildingPosition == BuildingPosition.Corner) {
                    if (upConnectedBuilding && downConnectedBuilding && levelData.ConstructionCornerUpDown)
                        construction = levelData.ConstructionCornerUpDown;
                    else if (upConnectedBuilding && levelData.ConstructionCornerUp)
                        construction = levelData.ConstructionCornerUp;
                    else if (downConnectedBuilding && levelData.ConstructionCornerDown)
                        construction = levelData.ConstructionCornerDown;
                    else if (!upConnectedBuilding && !downConnectedBuilding && levelData.ConstructionCorner)
                        construction = levelData.ConstructionCorner;
                }
            }
        }
        else {
            Debug.LogError("LevelData is not TowerBuildingLevelData");
        }

        return construction;
    }

    private TowerBuilding GetNeighborBuilding(Side side)
    {
        int horizontalIndexOffset = side == Side.Left ? 1 : side == Side.Right ? -1 : 0;
        int verticalIndexOffset = side == Side.Up ? 1 : side == Side.Down ? -1 : 0;
        int sideIndex = (placeIndex + horizontalIndexOffset + CityManager.roomsCountPerFloor) % CityManager.roomsCountPerFloor;
        int verticalIndex = floorIndex + verticalIndexOffset;

        if (verticalIndex < CityManager.Instance.BuiltFloors.Count && verticalIndex >= 0) {
            Building building = CityManager.Instance.BuiltFloors[verticalIndex].roomBuildingPlaces[sideIndex].placedBuilding;
            return building as TowerBuilding;
        }
        return null;
    }

    private void GetAllNeighborBuildings()
    {
        leftNeighborBuilding = GetNeighborBuilding(Side.Left);
        rightNeighborBuilding = GetNeighborBuilding(Side.Right);
        upNeighborBuilding = GetNeighborBuilding(Side.Up);
        downNeighborBuilding = GetNeighborBuilding(Side.Down);
    }

    private TowerBuilding CheckConnectionPossibility(TowerBuilding target, ConnectionType requiredConnection)
    {
        if (!target) return null;
        if (target.buildingData.BuildingId != buildingData.BuildingId) return null;
        if (buildingData.ConnectionType != requiredConnection) return null;

        return target;
    }

    public bool ConnectedWith(TowerBuilding target)
    {
        if (!target) {
            Debug.Log("buildingToCheck == NULL");
            return false;
        }

        TowerBuilding start = this;
        TowerBuilding current = start;
        var visited = new HashSet<TowerBuilding>();
        visited.Add(current);
        if (buildingData.ConnectionType == ConnectionType.Horizontal) {
            TowerBuilding[] directions = { leftNeighborBuilding, rightNeighborBuilding };
            foreach (var direction in directions) {
                current = direction;
                while (current && current.BuildingData.BuildingId == buildingData.BuildingId) {
                    if (!visited.Add(current)) return false;
                    if (current == target) return true;
                    current = (direction == leftNeighborBuilding) ? current.leftNeighborBuilding : current.rightNeighborBuilding;
                }
            }
        }
        else if (buildingData.ConnectionType == ConnectionType.Vertical) {
            TowerBuilding[] directions = { upNeighborBuilding, downNeighborBuilding };
            foreach (var direction in directions) {
                current = direction;
                while (current && current.buildingData.BuildingId == buildingData.BuildingId) {
                    if (!visited.Add(current)) return false;
                    if (current == target) return true;
                    current = (direction == upNeighborBuilding) ? current.upNeighborBuilding : current.downNeighborBuilding;
                }
            }
        }
        return false;
    }

    private void OnConstructionPlaced(Building building)
    {
        if (building is not TowerBuilding) return;

        GetAllNeighborBuildings();
    }
}
