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

    public TowerBuildingEntry(int floorIndex, int placeIndex) : base()
    {
        this.floorIndex = floorIndex;
        this.placeIndex = placeIndex;
    }
}

public class TowerBuilding : Building
{
    public BuildingPlace buildingPlace { get; private set; } = null;

    public BuildingPosition buildingPosition { get; private set; } = BuildingPosition.Straight;
    public int floorIndex { get; private set; }
    public int placeIndex { get; private set; }

    public TowerBuilding leftBuilding => buildingPlace.leftPlace.PlacedBuilding;
    public TowerBuilding rightBuilding => buildingPlace.rightPlace.PlacedBuilding;
    public TowerBuilding upBuilding => buildingPlace.upPlace?.PlacedBuilding;
    public TowerBuilding downBuilding => buildingPlace.downPlace?.PlacedBuilding;

    public TowerBuilding leftConnectedBuilding => CheckConnectionPossibility(leftBuilding, ConnectionType.Horizontal);
    public TowerBuilding rightConnectedBuilding => CheckConnectionPossibility(rightBuilding, ConnectionType.Horizontal);
    public TowerBuilding upConnectedBuilding => CheckConnectionPossibility(upBuilding, ConnectionType.Vertical);
    public TowerBuilding downConnectedBuilding => CheckConnectionPossibility(downBuilding, ConnectionType.Vertical);

    public IEnumerable NeighborBuildings(NeighborMask mask)
    {
        if (mask.HasFlag(NeighborMask.Left)) {
            yield return leftBuilding;
        }
        if (mask.HasFlag(NeighborMask.Right)) {
            yield return rightBuilding;
        }
        if (mask.HasFlag(NeighborMask.Up)) {
            yield return upBuilding;
        }
        if (mask.HasFlag(NeighborMask.Down)) {
            yield return downBuilding;
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

    protected override void OnInit(BuildingEntry data)
    {
        TowerBuildingEntry towerData = data as TowerBuildingEntry;
        floorIndex = towerData.floorIndex;
        placeIndex = towerData.placeIndex;

        List<FloorFrameModule> floors = CityManager.Instance.BuiltFloors;
        BuildingPlace place = null;

        if (BuildingData.BuildingType == BuildingType.Room) {
            place = floors[towerData.floorIndex].roomBuildingPlaces[towerData.placeIndex];
        }
        else if (BuildingData.BuildingType == BuildingType.Hall) {
            place = floors[towerData.floorIndex].hallBuildingPlace;
        }
        else if (BuildingData.BuildingType == BuildingType.FloorFrame) {
            int index = towerData.floorIndex - 1;
            place = floors.Count > index && index >= 0 ? floors[index].floorBuildingPlace : null;
        }

        SetBuildingPlace(place);

        if (placeIndex % 2 == 0) {
            SetBuildingPosition(BuildingPosition.Corner);
        }
        else {
            SetBuildingPosition(BuildingPosition.Straight);
        }

        ApplyTransform();
    }

    private void SetBuildingPlace(BuildingPlace place)
    {
        buildingPlace = place;
    }

    private void SetBuildingPosition(BuildingPosition position)
    {
        buildingPosition = position;
    }

    private void ApplyTransform()
    {
        if (!buildingPlace) return;
        if (buildingData.BuildingType != BuildingType.Room && buildingData.BuildingType != BuildingType.Hall) return;

        transform.SetParent(buildingPlace.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
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

    private TowerBuilding CheckConnectionPossibility(TowerBuilding target, ConnectionType requiredConnection)
    {
        if (!target) return null;
        if (target.buildingData.BuildingId != buildingData.BuildingId) return null;
        if (requiredConnection != buildingData.ConnectionType) return null;

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
            TowerBuilding[] directions = { leftBuilding, rightBuilding };
            foreach (var direction in directions) {
                current = direction;
                while (current && current.BuildingData.BuildingId == buildingData.BuildingId) {
                    if (!visited.Add(current)) return false;
                    if (current == target) return true;
                    current = (direction == leftBuilding) ? current.leftBuilding : current.rightBuilding;
                }
            }
        }
        else if (buildingData.ConnectionType == ConnectionType.Vertical) {
            TowerBuilding[] directions = { upBuilding, downBuilding };
            foreach (var direction in directions) {
                current = direction;
                while (current && current.buildingData.BuildingId == buildingData.BuildingId) {
                    if (!visited.Add(current)) return false;
                    if (current == target) return true;
                    current = (direction == upBuilding) ? current.upBuilding : current.downBuilding;
                }
            }
        }
        return false;
    }

    private void OnConstructionPlaced(Building building)
    {
        if (building is not TowerBuilding) return;
    }
}
