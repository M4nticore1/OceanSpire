using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

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

public class TowerBuilding : Building, IConnectedBuildingsListener
{
    public BuildingPlace buildingPlace { get; private set; }

    public BuildingPosition buildingPosition { get; private set; }
    public int floorIndex { get; private set; }
    public int placeIndex { get; private set; }

    public TowerBuilding leftBuilding;
    public TowerBuilding rightBuilding;
    public TowerBuilding upBuilding;
    public TowerBuilding downBuilding;

    public TowerBuilding LeftConnectedBuilding => leftBuilding && ConnectedWith(leftBuilding) ? leftBuilding : null;
    public TowerBuilding RightConnectedBuilding => rightBuilding && ConnectedWith(rightBuilding) ? rightBuilding : null;
    public TowerBuilding UpConnectedBuilding => upBuilding && ConnectedWith(upBuilding) ? upBuilding : null;
    public TowerBuilding DownConnectedBuilding => downBuilding && ConnectedWith(downBuilding) ? downBuilding : null;

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

        AssignNeighborBuildings();

        if (placeIndex % 2 == 0) {
            SetBuildingPosition(BuildingPosition.Corner);
        }
        else {
            SetBuildingPosition(BuildingPosition.Straight);
        }

        ApplyTransform();
        InvokeBuildingInited();
    }

    protected override BuildingConstruction GetConstructionToSpawn()
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

    protected override void OnChangedConstruction()
    {
        base.OnChangedConstruction();

        //foreach (TowerBuilding towerBuilding in NeighborBuildings(NeighborMask.All)) {
        //    if (!DirectlyConnectedWith(towerBuilding)) continue;

        //    foreach (var module in towerBuilding.GetComponents<BuildingModule>()) {
        //        module.HandleChangedConstruction();
        //    }
        //}
    }

    protected override void InvokeBuildingInited()
    {
        base.InvokeBuildingInited();

        foreach (var building in ConnectedBuildings()) {
            building.HandleConnectedBuildingInited(this);
        }
    }

    protected override void InvokeBuildingDemolished()
    {
        base.InvokeBuildingDemolished();

        buildingPlace.HandleBuildingDemolished();

        foreach (var building in ConnectedBuildings()) {
            building.HandleConnectedBuildingDemolished(this);
        }
    }

    public IEnumerable<TowerBuilding> NeighborBuildings(NeighborMask mask)
    {
        if (mask.HasFlag(NeighborMask.Left) && leftBuilding) {
            yield return leftBuilding;
        }
        if (mask.HasFlag(NeighborMask.Right) && rightBuilding) {
            yield return rightBuilding;
        }
        if (mask.HasFlag(NeighborMask.Up) && upBuilding) {
            yield return upBuilding;
        }
        if (mask.HasFlag(NeighborMask.Down) && downBuilding) {
            yield return downBuilding;
        }
    }

    public IEnumerable<TowerBuilding> ConnectedBuildings()
    {
        NeighborMask mask = GetNeighborMaskByConnectionType(buildingData.ConnectionType);
        foreach (var building in NeighborBuildings(mask)) {
            if (!building) continue;
            if (!CanConnectedWith(building)) continue;
            yield return building;
        }
    }

    public void HandleConnectedBuildingInited(Building building)
    {
        AssignNeighborBuildings();
        UpdateConstruction();

        foreach (var module in GetComponents<IConnectedBuildingsListener>()) {
            if ((Component)module == this) continue;

            module.HandleConnectedBuildingInited(building);
        }
    }

    public void HandleConnectedBuildingDemolished(Building building)
    {
        AssignNeighborBuildings();
        UpdateConstruction();

        foreach (var module in GetComponents<IConnectedBuildingsListener>()) {
            if ((Component)module == this) continue;

            module.HandleConnectedBuildingDemolished(building);
        }
    }

    public bool NeighborWith(TowerBuilding target)
    {
        if (buildingData.ConnectionType == ConnectionType.Horizontal) {
            if (math.abs(target.floorIndex - floorIndex) == 1) return true;
        }
        else if (buildingData.ConnectionType == ConnectionType.Vertical) {
            if ((target.placeIndex - placeIndex) % BuildingsManager.RoomsCountPerFloor == 1) return true;
        }
        return false;
    }   

    private bool CanConnectedWith(TowerBuilding building)
    {
        if (building.buildingData.BuildingId != buildingData.BuildingId) return false;
        if (building.levelComponent.LevelIndex != levelComponent.LevelIndex) return false;
        //if (!NeighborWith(building)) return false;
        return true;
    }

    public bool ConnectedWith(TowerBuilding target)
    {
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

    private void AssignNeighborBuildings()
    {
        if (buildingData.BuildingType == BuildingType.Room) {
            int roomsCount = BuildingsManager.RoomsCountPerFloor;
            int leftIndex = (placeIndex + 1) % roomsCount;
            leftBuilding = buildingsManager.BuiltFloors[floorIndex].RoomBuildingPlaces[leftIndex].PlacedBuilding;

            int rightIndex = (roomsCount + placeIndex - 1) % roomsCount;
            rightBuilding = buildingsManager.BuiltFloors[floorIndex].RoomBuildingPlaces[rightIndex].PlacedBuilding;
        }

        int floorCount = buildingsManager.BuiltFloors.Count;
        int upIndex = floorIndex + 1;
        if (upIndex < floorCount) {
            upBuilding = buildingsManager.BuiltFloors[upIndex].RoomBuildingPlaces[placeIndex].PlacedBuilding;
        }
        else {
            upBuilding = null;
        }

        int downIndex = floorIndex - 1;
        if (downIndex >= 0) {
            downBuilding = buildingsManager.BuiltFloors[downIndex].RoomBuildingPlaces[placeIndex].PlacedBuilding;
        }
        else {
            downBuilding = null;
        }
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
            transform.position = buildingPlace.transform.position;
        }
        else {
            transform.SetParent(buildingPlace.transform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }

    private NeighborMask GetNeighborMaskByConnectionType(ConnectionType connectionType)
    {
        if (connectionType == ConnectionType.Horizontal) {
            return NeighborMask.Horizontal;
        }
        else if (connectionType == ConnectionType.Vertical) {
            return NeighborMask.Vertical;
        }
        else {
            return NeighborMask.None;
        }
    }
}
