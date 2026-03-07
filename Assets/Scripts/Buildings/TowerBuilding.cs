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
    public BuildingPlace buildingPlace = null;

    public BuildingPosition buildingPosition { get; private set; } = BuildingPosition.Straight;
    public int floorIndex { get; private set; }
    public int placeIndex { get; private set; }

    public TowerBuilding LeftBuilding => buildingPlace?.leftPlace?.PlacedBuilding;
    public TowerBuilding RightBuilding => buildingPlace?.rightPlace?.PlacedBuilding;
    public TowerBuilding UpBuilding => buildingPlace?.upPlace?.PlacedBuilding;
    public TowerBuilding DownBuilding => buildingPlace?.downPlace?.PlacedBuilding;

    public TowerBuilding LeftConnectedBuilding => CheckConnectionPossibility(LeftBuilding, ConnectionType.Horizontal);
    public TowerBuilding RightConnectedBuilding => CheckConnectionPossibility(RightBuilding, ConnectionType.Horizontal);
    public TowerBuilding UpConnectedBuilding => CheckConnectionPossibility(UpBuilding, ConnectionType.Vertical);
    public TowerBuilding DownConnectedBuilding => CheckConnectionPossibility(DownBuilding, ConnectionType.Vertical);

    protected override void OnInit(BuildingEntry data)
    {
        TowerBuildingEntry towerData = data as TowerBuildingEntry;
        floorIndex = towerData.floorIndex;
        placeIndex = towerData.placeIndex;

        List<FloorFrameModule> floors = buildingsManager.BuiltFloors;
        BuildingPlace place = null;

        if (BuildingData.BuildingType == BuildingType.Room) {
            place = floors[towerData.floorIndex].RoomBuildingPlaces[towerData.placeIndex];
        }
        else if (BuildingData.BuildingType == BuildingType.Hall) {
            place = floors[towerData.floorIndex].HallBuildingPlace;
        }
        else if (BuildingData.BuildingType == BuildingType.FloorFrame) {
            int index = towerData.floorIndex - 1;
            place = floors.Count > index && index >= 0 ? floors[index].FloorBuildingPlace : null;
        }

        if (place) {
            SetBuildingPlace(place);
        }

        if (placeIndex % 2 == 0) {
            SetBuildingPosition(BuildingPosition.Corner);
        }
        else {
            SetBuildingPosition(BuildingPosition.Straight);
        }

        ApplyTransform();
        InvokeBuildingPlaced();
    }

    public override void Demolish()
    {
        base.Demolish();

        buildingPlace.HandleBuildingDemolished();
        InvokeBuildingDemolished();
    }

    public IEnumerable NeighborBuildings(NeighborMask mask)
    {
        if (mask.HasFlag(NeighborMask.Left)) {
            yield return LeftBuilding;
        }
        if (mask.HasFlag(NeighborMask.Right)) {
            yield return RightBuilding;
        }
        if (mask.HasFlag(NeighborMask.Up)) {
            yield return UpBuilding;
        }
        if (mask.HasFlag(NeighborMask.Down)) {
            yield return DownBuilding;
        }
    }

    public void HandleConnectedBuildingPlaced(Building building)
    {
        UpdateConstruction();
    }

    public void HandleConnectedBuildingDemolished(Building building)
    {
        UpdateConstruction();
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
            TowerBuilding[] directions = { LeftBuilding, RightBuilding };
            foreach (var direction in directions) {
                current = direction;
                while (current && current.BuildingData.BuildingId == buildingData.BuildingId) {
                    if (!visited.Add(current)) return false;
                    if (current == target) return true;
                    current = (direction == LeftBuilding) ? current.LeftBuilding : current.RightBuilding;
                }
            }
        }
        else if (buildingData.ConnectionType == ConnectionType.Vertical) {
            TowerBuilding[] directions = { UpBuilding, DownBuilding };
            foreach (var direction in directions) {
                current = direction;
                while (current && current.buildingData.BuildingId == buildingData.BuildingId) {
                    if (!visited.Add(current)) return false;
                    if (current == target) return true;
                    current = (direction == UpBuilding) ? current.UpBuilding : current.DownBuilding;
                }
            }
        }
        return false;
    }

    private void SetBuildingPlace(BuildingPlace place)
    {
        buildingPlace = place;
        buildingPlace.HandleBuildingInited(this);
    }

    private void SetBuildingPosition(BuildingPosition position)
    {
        buildingPosition = position;
    }

    private void ApplyTransform()
    {
        if (!buildingPlace) return;

        if (GetComponent<FloorFrameModule>()) {
            transform.SetParent(buildingPlace.transform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        else {
            transform.position = buildingPlace.transform.position;
        }
    }

    private void InvokeBuildingPlaced()
    {
        foreach (TowerBuilding building in NeighborBuildings(NeighborMask.All)) {
            building?.HandleConnectedBuildingPlaced(this);
        }
    }

    private void InvokeBuildingDemolished()
    {
        foreach (TowerBuilding building in NeighborBuildings(NeighborMask.All)) {
            building?.HandleConnectedBuildingDemolished(this);
        }
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
                    if (LeftConnectedBuilding && RightConnectedBuilding && levelData.ConstructionStraightLeftRight)
                        construction = levelData.ConstructionStraightLeftRight;
                    else if (LeftConnectedBuilding && levelData.ConstructionStraightLeft)
                        construction = levelData.ConstructionStraightLeft;
                    else if (RightConnectedBuilding && levelData.ConstructionStraightRight)
                        construction = levelData.ConstructionStraightRight;
                    else if (!LeftConnectedBuilding && !RightConnectedBuilding && levelData.ConstructionStraight)
                        construction = levelData.ConstructionStraight;
                }
                else if (buildingPosition == BuildingPosition.Corner) {
                    if (LeftConnectedBuilding && RightConnectedBuilding && levelData.ConstructionCornerLeftRight)
                        construction = levelData.ConstructionCornerLeftRight;
                    else if (LeftConnectedBuilding && levelData.ConstructionCornerLeft)
                        construction = levelData.ConstructionCornerLeft;
                    else if (RightConnectedBuilding && levelData.ConstructionCornerRight)
                        construction = levelData.ConstructionCornerRight;
                    else if (!LeftConnectedBuilding && !RightConnectedBuilding && levelData.ConstructionCorner)
                        construction = levelData.ConstructionCorner;
                }
            }
            else if (buildingData.ConnectionType == ConnectionType.Vertical) {
                if (buildingPosition == BuildingPosition.Straight) {
                    if (UpConnectedBuilding && DownConnectedBuilding && levelData.ConstructionStraightUpDown)
                        construction = levelData.ConstructionStraightUpDown;
                    else if (UpConnectedBuilding && levelData.ConstructionStraightUp)
                        construction = levelData.ConstructionStraightUp;
                    else if (DownConnectedBuilding && levelData.ConstructionStraightDown)
                        construction = levelData.ConstructionStraightDown;
                    else if (!UpConnectedBuilding && !DownConnectedBuilding && levelData.ConstructionStraight)
                        construction = levelData.ConstructionStraight;
                }
                else if (buildingPosition == BuildingPosition.Corner) {
                    if (UpConnectedBuilding && DownConnectedBuilding && levelData.ConstructionCornerUpDown)
                        construction = levelData.ConstructionCornerUpDown;
                    else if (UpConnectedBuilding && levelData.ConstructionCornerUp)
                        construction = levelData.ConstructionCornerUp;
                    else if (DownConnectedBuilding && levelData.ConstructionCornerDown)
                        construction = levelData.ConstructionCornerDown;
                    else if (!UpConnectedBuilding && !DownConnectedBuilding && levelData.ConstructionCorner)
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
}
