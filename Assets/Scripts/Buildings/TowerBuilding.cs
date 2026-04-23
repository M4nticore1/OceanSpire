using System;
using System.Collections.Generic;
using Unity.Mathematics;
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
public class TowerBuildingData : BuildingData
{
    public int FloorIndex { get; private set; }
    public int PlaceIndex { get; private set; }

    public TowerBuildingData(int id, int instanceId, int level, ConstructionData constructionData, int floorIndex, int placeIndex) : base(id, instanceId, level, constructionData)
    {
        FloorIndex = floorIndex;
        PlaceIndex = placeIndex;
    }
}

public class TowerBuilding : Building, INeighborBuildingsListener
{
    public BuildingPlace BuildingPlace { get; private set; }

    public BuildingPosition BuildingPosition { get; private set; }
    public int FloorIndex { get; private set; }
    public int PlaceIndex { get; private set; }

    public TowerBuilding LeftBuilding { get; private set; }
    public TowerBuilding RightBuilding { get; private set; }
    public TowerBuilding UpBuilding { get; private set; }
    public TowerBuilding DownBuilding { get; private set; }

    public TowerBuilding LeftConnectedBuilding => LeftBuilding && ConnectedWith(LeftBuilding) ? LeftBuilding : null;
    public TowerBuilding RightConnectedBuilding => RightBuilding && ConnectedWith(RightBuilding) ? RightBuilding : null;
    public TowerBuilding UpConnectedBuilding => UpBuilding && ConnectedWith(UpBuilding) ? UpBuilding : null;
    public TowerBuilding DownConnectedBuilding => DownBuilding && ConnectedWith(DownBuilding) ? DownBuilding : null;

    protected override void OnInit(BuildingData data)
    {
        TowerBuildingData towerData = data as TowerBuildingData;
        FloorIndex = towerData.FloorIndex;
        PlaceIndex = towerData.PlaceIndex;

        List<FloorFrameModule> floors = BuildingsManager.instance.BuiltFloors;
        BuildingPlace place = null;

        if (BuildingData.BuildingType == BuildingType.Room) {
            place = floors[towerData.FloorIndex].RoomBuildingPlaces[towerData.PlaceIndex];
        }
        else if (BuildingData.BuildingType == BuildingType.Hall) {
            place = floors[towerData.FloorIndex].HallBuildingPlace;
        }
        else if (BuildingData.BuildingType == BuildingType.FloorFrame) {
            int index = towerData.FloorIndex - 1;
            place = floors.Count > index && index >= 0 ? floors[index].FloorBuildingPlace : null;
        }

        if (place) {
            SetBuildingPlace(place);
        }

        AssignNeighborBuildings();

        if (PlaceIndex % 2 == 0) {
            SetBuildingPosition(BuildingPosition.Corner);
        }
        else {
            SetBuildingPosition(BuildingPosition.Straight);
        }

        ApplyTransform();
    }

    protected override BuildingConstruction GetConstructionToSpawn()
    {
        BuildingConstruction construction = null;

        if (LevelData is TowerBuildingLevelData levelData) {
            if (ConstructionComponent.IsUnderConstruction) {
                if (BuildingPosition == BuildingPosition.Straight) {
                    construction = levelData.ConstructionStraightFrame;
                }
                else if (BuildingPosition == BuildingPosition.Corner) {
                    construction = levelData.ConstructionCornerFrame;
                }
            }
            else if (buildingData.ConnectionType == ConnectionType.None) {
                if (BuildingPosition == BuildingPosition.Straight) {
                    if (levelData.ConstructionStraight)
                        construction = levelData.ConstructionStraight;
                }
                else if (BuildingPosition == BuildingPosition.Corner) {
                    if (levelData.ConstructionCorner)
                        construction = levelData.ConstructionCorner;
                }
            }
            else if (buildingData.ConnectionType == ConnectionType.Horizontal) {
                if (BuildingPosition == BuildingPosition.Straight) {
                    if (LeftConnectedBuilding && RightConnectedBuilding && levelData.ConstructionStraightLeftRight)
                        construction = levelData.ConstructionStraightLeftRight;
                    else if (LeftConnectedBuilding && levelData.ConstructionStraightLeft)
                        construction = levelData.ConstructionStraightLeft;
                    else if (RightConnectedBuilding && levelData.ConstructionStraightRight)
                        construction = levelData.ConstructionStraightRight;
                    else if (!LeftConnectedBuilding && !RightConnectedBuilding && levelData.ConstructionStraight)
                        construction = levelData.ConstructionStraight;
                }
                else if (BuildingPosition == BuildingPosition.Corner) {
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
                if (BuildingPosition == BuildingPosition.Straight) {
                    if (UpConnectedBuilding && DownConnectedBuilding && levelData.ConstructionStraightUpDown)
                        construction = levelData.ConstructionStraightUpDown;
                    else if (UpConnectedBuilding && levelData.ConstructionStraightUp)
                        construction = levelData.ConstructionStraightUp;
                    else if (DownConnectedBuilding && levelData.ConstructionStraightDown)
                        construction = levelData.ConstructionStraightDown;
                    else if (!UpConnectedBuilding && !DownConnectedBuilding && levelData.ConstructionStraight)
                        construction = levelData.ConstructionStraight;
                }
                else if (BuildingPosition == BuildingPosition.Corner) {
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

    protected override void InvokeBuildingInited()
    {
        base.InvokeBuildingInited();

        foreach (var building in NeighborBuildings(NeighborMask.All)) {
            building.HandleNeighborBuildingInited(this);
        }
    }

    protected override void InvokeBuildingDemolished()
    {
        base.InvokeBuildingDemolished();

        BuildingPlace.SetPlacedBuilding(null);

        foreach (var building in ConnectedBuildings()) {
            building.HandleNeighborBuildingDemolished(this);
        }
    }

    public IEnumerable<TowerBuilding> NeighborBuildings(NeighborMask mask)
    {
        if (mask.HasFlag(NeighborMask.Left) && LeftBuilding) {
            yield return LeftBuilding;
        }
        if (mask.HasFlag(NeighborMask.Right) && RightBuilding) {
            yield return RightBuilding;
        }
        if (mask.HasFlag(NeighborMask.Up) && UpBuilding) {
            yield return UpBuilding;
        }
        if (mask.HasFlag(NeighborMask.Down) && DownBuilding) {
            yield return DownBuilding;
        }
    }

    public IEnumerable<TowerBuilding> ConnectedBuildings()
    {
        NeighborMask mask = GetNeighborMaskByConnectionType(buildingData.ConnectionType);
        foreach (var building in NeighborBuildings(mask)) {
            if (!building) continue;
            if (!ConnectedWith(building)) continue;
            yield return building;
        }
    }

    public void HandleNeighborBuildingInited(TowerBuilding building)
    {
        AssignNeighborBuildings();

        if (ConnectedWith(building)) {
            UpdateConstruction();
        }

        foreach (var module in GetComponents<INeighborBuildingsListener>()) {
            if ((Component)module == this) continue;

            module.HandleNeighborBuildingInited(building);
        }
    }

    public void HandleNeighborBuildingDemolished(TowerBuilding building)
    {
        AssignNeighborBuildings();
        UpdateConstruction();

        foreach (var module in GetComponents<INeighborBuildingsListener>()) {
            if ((Component)module == this) continue;

            module.HandleNeighborBuildingDemolished(building);
        }
    }

    public List<TowerBuilding> GetNetworkBuildings()
    {
        List<TowerBuilding> network = new List<TowerBuilding>();
        Queue<TowerBuilding> queue = new Queue<TowerBuilding>();
        HashSet<TowerBuilding> visited = new HashSet<TowerBuilding>();

        queue.Enqueue(this);
        visited.Add(this);

        while (queue.Count > 0) {
            TowerBuilding building = queue.Dequeue();

            foreach (var connected in building.ConnectedBuildings()) {
                if (!visited.Contains(connected)) {
                    visited.Add(connected);
                    queue.Enqueue(connected);
                    network.Add(connected);
                }
            }
        }

        return network;
    }

    public bool NeighborWith(TowerBuilding target)
    {
        bool horizontal = math.abs(target.FloorIndex - FloorIndex) == 1;
        bool vertical = (target.PlaceIndex - PlaceIndex) % BuildingsManager.RoomsCountPerFloor == 1;
        return horizontal || vertical;
    }   

    public bool ConnectedWith(TowerBuilding building)
    {
        if (building.buildingData.BuildingId != buildingData.BuildingId) return false;
        if (building.levelComponent.level != levelComponent.level) return false;
        foreach (var neighborBuilding in NeighborBuildings(GetNeighborMaskByConnectionType(BuildingData.ConnectionType))) {
            if (neighborBuilding == building)
                return true;
        }
        return false;
    }

    public bool NetworkWith(TowerBuilding target, HashSet<TowerBuilding> visited = null)
    {
        if (this == target)
            return true;

        if (visited == null) {
            visited = new HashSet<TowerBuilding>();
        }
        visited.Add(this);

        foreach (var direction in ConnectedBuildings()) {
            if (!visited.Add(direction))
                continue;
            if (direction.NetworkWith(target, visited))
                return true;
        }
        return false;
    }

    private void AssignNeighborBuildings()
    {
        if (buildingData.BuildingType == BuildingType.Room) {
            int roomsCount = BuildingsManager.RoomsCountPerFloor;
            int leftIndex = (PlaceIndex + 1) % roomsCount;
            LeftBuilding = BuildingsManager.instance.BuiltFloors[FloorIndex].RoomBuildingPlaces[leftIndex].PlacedBuilding;

            int rightIndex = (roomsCount + PlaceIndex - 1) % roomsCount;
            RightBuilding = BuildingsManager.instance.BuiltFloors[FloorIndex].RoomBuildingPlaces[rightIndex].PlacedBuilding;
        }

        int floorCount = BuildingsManager.instance.BuiltFloors.Count;
        int upIndex = FloorIndex + 1;

        if (upIndex < floorCount) {
            UpBuilding = BuildingsManager.instance.BuiltFloors[upIndex].RoomBuildingPlaces[PlaceIndex].PlacedBuilding;
        }
        else {
            UpBuilding = null;
        }

        int downIndex = FloorIndex - 1;

        if (downIndex >= 0) {
            DownBuilding = BuildingsManager.instance.BuiltFloors[downIndex].RoomBuildingPlaces[PlaceIndex].PlacedBuilding;
        }
        else {
            DownBuilding = null;
        }
    }

    private void SetBuildingPlace(BuildingPlace place)
    {
        BuildingPlace = place;
        BuildingPlace.SetPlacedBuilding(this);
    }

    private void SetBuildingPosition(BuildingPosition position)
    {
        BuildingPosition = position;
    }

    private void ApplyTransform()
    {
        if (!BuildingPlace) return;

        if (GetComponent<FloorFrameModule>()) {
            transform.position = BuildingPlace.transform.position;
        }
        else {
            transform.SetParent(BuildingPlace.transform);
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
