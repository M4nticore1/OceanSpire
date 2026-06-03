using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BuildingPosition
{
    Straight,
    Corner
}

public enum Direction
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

public class TowerBuilding : Building
{
    public BuildingPlace BuildingPlace { get; private set; }

    public BuildingPosition BuildingPosition { get; private set; }
    public int FloorIndex;
    public int PlaceIndex;

    private Dictionary<Direction, TowerBuilding> neighborBuildings = new();
    public IReadOnlyDictionary<Direction, TowerBuilding> NeighborBuildings => neighborBuildings;

    private Dictionary<Direction, TowerBuilding> connectedBuildings = new();
    public IReadOnlyDictionary<Direction, TowerBuilding> ConnectedBuildings => connectedBuildings;

    public IEnumerable<TowerBuilding> NeighborBuildingsEnumerable(NeighborMask mask)
    {
        if (mask.HasFlag(NeighborMask.Left) && GetNeighborBuilding(Direction.Left)) {
            yield return GetNeighborBuilding(Direction.Left);
        }
        if (mask.HasFlag(NeighborMask.Right) && GetNeighborBuilding(Direction.Right)) {
            yield return GetNeighborBuilding(Direction.Right);
        }
        if (mask.HasFlag(NeighborMask.Up) && GetNeighborBuilding(Direction.Up)) {
            yield return GetNeighborBuilding(Direction.Up);
        }
        if (mask.HasFlag(NeighborMask.Down) && GetNeighborBuilding(Direction.Down)) {
            yield return GetNeighborBuilding(Direction.Down);
        }
    }

    public IEnumerable<TowerBuilding> ConnectedBuildingsEnumerable()
    {
        if (GetConnectedBuilding(Direction.Left)) {
            yield return GetConnectedBuilding(Direction.Left);
        }
        if (GetConnectedBuilding(Direction.Right)) {
            yield return GetConnectedBuilding(Direction.Right);
        }
        if (GetConnectedBuilding(Direction.Up)) {
            yield return GetConnectedBuilding(Direction.Up);
        }
        if (GetConnectedBuilding(Direction.Down)) {
            yield return GetConnectedBuilding(Direction.Down);
        }
    }

    protected override void OnInit(BuildingData buildingData)
    {
        base.OnInit(buildingData);

        var towerData = buildingData as TowerBuildingData;
        FloorIndex = towerData.FloorIndex;
        PlaceIndex = towerData.PlaceIndex;

        UpdateBuildingPlace(FloorIndex, PlaceIndex);
        UpdateNeighborBuildings();
        UpdateConnectedBuildings();
        UpdatePositionType();
        UpdateConstruction();
    }

    protected override void OnDemolish()
    {
        if (BuildingPlace) {
            BuildingPlace.RemovePlacedBuilding();
        }

        InvokeBuildingDemolished();
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
                    if (GetConnectedBuilding(Direction.Left) && GetConnectedBuilding(Direction.Right) && levelData.ConstructionStraightLeftRight)
                        construction = levelData.ConstructionStraightLeftRight;
                    else if (GetConnectedBuilding(Direction.Left) && levelData.ConstructionStraightLeft)
                        construction = levelData.ConstructionStraightLeft;
                    else if (GetConnectedBuilding(Direction.Right) && levelData.ConstructionStraightRight)
                        construction = levelData.ConstructionStraightRight;
                    else if (!GetConnectedBuilding(Direction.Left) && !GetConnectedBuilding(Direction.Right) && levelData.ConstructionStraight)
                        construction = levelData.ConstructionStraight;
                }
                else if (BuildingPosition == BuildingPosition.Corner) {
                    if (GetConnectedBuilding(Direction.Left) && GetConnectedBuilding(Direction.Right) && levelData.ConstructionCornerLeftRight)
                        construction = levelData.ConstructionCornerLeftRight;
                    else if (GetConnectedBuilding(Direction.Left) && levelData.ConstructionCornerLeft)
                        construction = levelData.ConstructionCornerLeft;
                    else if (GetConnectedBuilding(Direction.Right) && levelData.ConstructionCornerRight)
                        construction = levelData.ConstructionCornerRight;
                    else if (!GetConnectedBuilding(Direction.Left) && !GetConnectedBuilding(Direction.Right) && levelData.ConstructionCorner)
                        construction = levelData.ConstructionCorner;
                }
            }
            else if (buildingData.ConnectionType == ConnectionType.Vertical) {
                if (BuildingPosition == BuildingPosition.Straight) {
                    if (GetConnectedBuilding(Direction.Up) && GetConnectedBuilding(Direction.Down) && levelData.ConstructionStraightUpDown)
                        construction = levelData.ConstructionStraightUpDown;
                    else if (GetConnectedBuilding(Direction.Up) && levelData.ConstructionStraightUp)
                        construction = levelData.ConstructionStraightUp;
                    else if (GetConnectedBuilding(Direction.Down) && levelData.ConstructionStraightDown)
                        construction = levelData.ConstructionStraightDown;
                    else if (!GetConnectedBuilding(Direction.Up) && !GetConnectedBuilding(Direction.Down) && levelData.ConstructionStraight)
                        construction = levelData.ConstructionStraight;
                }
                else if (BuildingPosition == BuildingPosition.Corner) {
                    if (GetConnectedBuilding(Direction.Up) && GetConnectedBuilding(Direction.Down) && levelData.ConstructionCornerUpDown)
                        construction = levelData.ConstructionCornerUpDown;
                    else if (GetConnectedBuilding(Direction.Up) && levelData.ConstructionCornerUp)
                        construction = levelData.ConstructionCornerUp;
                    else if (GetConnectedBuilding(Direction.Down) && levelData.ConstructionCornerDown)
                        construction = levelData.ConstructionCornerDown;
                    else if (!GetConnectedBuilding(Direction.Up) && !GetConnectedBuilding(Direction.Down) && levelData.ConstructionCorner)
                        construction = levelData.ConstructionCorner;
                }
            }
        }
        else {
            Debug.LogError("LevelData is not TowerBuildingLevelData");
        }

        return construction;
    }

    protected override void OnConstructionFinish()
    {
        base.OnConstructionFinish();

        UpdateConnectedBuildings();
    }

    public bool NetworkWith(TowerBuilding target, HashSet<TowerBuilding> visited = null)
    {
        if (this == target)
            return true;

        if (visited == null) {
            visited = new HashSet<TowerBuilding>();
        }
        visited.Add(this);

        foreach (var direction in connectedBuildings.Values) {
            if (!direction) continue;

            if (!visited.Add(direction))
                continue;
            if (direction.NetworkWith(target, visited))
                return true;
        }
        return false;
    }

    public bool ShouldConnectTo(TowerBuilding target)
    {
        if (!target) return false;
        if (target.buildingData.BuildingId != buildingData.BuildingId) return false;
        if (target.levelComponent.Level != levelComponent.Level) return false;
        if (target.constructionComponent.IsUnderConstruction) return false;

        return true;
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

            foreach (var connected in building.connectedBuildings.Values) {
                if (!connected) continue;
                if (connected.IsDemolished) continue;
                if (visited.Contains(connected)) continue;

                visited.Add(connected);
                queue.Enqueue(connected);
                network.Add(connected);
            }
        }

        return network;
    }

    private void UpdateBuildingPlace(int floorIndex, int placeIndex)
    {
        IReadOnlyList<FloorFrameModule> floors = BuildingsManager.Instance.BuiltFloors;
        BuildingPlace place = null;

        if (BuildingData.BuildingType == BuildingType.Room) {
            place = BuildingsManager.Instance.GetRoomPlace(floorIndex, placeIndex);
            transform.SetParent(place.transform);
        }
        else if (BuildingData.BuildingType == BuildingType.Hall) {
            place = floors[floorIndex].HallBuildingPlace;
            transform.SetParent(place.transform);
        }
        else if (BuildingData.BuildingType == BuildingType.FloorFrame) {
            place = BuildingsManager.Instance.GetFloorFrameBuilding(floorIndex - 1)?.FloorBuildingPlace;
            transform.SetParent(place ? null : BuildingsManager.Instance.FirstFloorBuildingTransform);
        }

        SetBuildingPlace(place);
    }

    private void UpdatePositionType()
    {
        if (PlaceIndex % 2 == 0) {
            SetBuildingPosition(BuildingPosition.Corner);
        }
        else {
            SetBuildingPosition(BuildingPosition.Straight);
        }
    }

    private void UpdateNeighborBuildings()
    {
        neighborBuildings.Clear();

        foreach (Direction dir in Enum.GetValues(typeof(Direction))) {
            var building = CalculateNeighbor(dir);
            if (!building) continue;

            TrySetNeighborWith(dir, building);
            building.TrySetNeighborWith(GetInverseDirection(dir), this);
        }
    }

    private void UpdateConnectedBuildings()
    {
        if (constructionComponent.IsUnderConstruction) return;

        connectedBuildings.Clear();

        foreach (Direction dir in Enum.GetValues(typeof(Direction))) {
            var building = GetNeighborBuilding(dir);
            if (!building) continue;

            TryConnectTo(dir, building);
            InvokeBuildingConnected();

            building.TryConnectTo(GetInverseDirection(dir), this);
            building.UpdateConstruction();
        }
    }

    private void TrySetNeighborWith(Direction dir, TowerBuilding target)
    {
        if (!target) return;
        if (!ShouldSetNeighborWith(target)) return;

        neighborBuildings[dir] = target;
    }

    private void TryConnectTo(Direction dir, TowerBuilding target)
    {
        if (!ShouldConnectTo(target)) return;

        ConnectTo(dir, target);
    }

    private void ConnectTo(Direction dir, TowerBuilding target)
    {
        connectedBuildings[dir] = target;
    }

    private void SetBuildingPlace(BuildingPlace place)
    {
        BuildingPlace = place;

        if (BuildingPlace) {
            BuildingPlace.TrySetPlaceBuilding(this);
        }
    }

    private void SetBuildingPosition(BuildingPosition position)
    {
        BuildingPosition = position;
    }

    private void InvokeBuildingConnected()
    {
        foreach (var module in GetComponents<IBuildingListener>()) {
            module.OnBuildingConnected(this);
        }
    }

    private void InvokeBuildingDemolished()
    {
        foreach (var building in neighborBuildings.Values.ToArray()) {
            if (!building) continue;

            building.OnNeighborBuildingDemolished(this);
        }

        foreach (var building in connectedBuildings.Values.ToArray()) {
            if (!building) continue;

            building.OnConnectedBuildingDemolished(this);
        }
    }

    private void OnNeighborBuildingDemolished(TowerBuilding building)
    {
        UpdateNeighborBuildings();
    }

    private void OnConnectedBuildingDemolished(TowerBuilding building)
    {
        UpdateConnectedBuildings();
        UpdateConstruction();

        foreach (var module in GetComponents<IBuildingListener>()) {
            if ((Component)module == this) continue;

            module.OnConnectedBuildingDemolished(building);
        }
    }

    private bool ShouldSetNeighborWith(TowerBuilding target)
    {
        if (!target) return false;
        if (target.IsDemolished) return false;

        return true;
    }

    private TowerBuilding CalculateNeighbor(Direction dir)
    {
        var floors = BuildingsManager.Instance.BuiltFloors;

        int floor = FloorIndex;
        int place = PlaceIndex;

        var (df, dp) = GetDelta(dir);

        int newFloor = floor + df;
        if (newFloor < 0 || newFloor >= floors.Count)
            return null;

        var floorData = floors[newFloor];

        int roomsCount = BuildingsManager.RoomsCountPerFloor;
        int newPlace = (place + dp + roomsCount) % roomsCount;

        return floorData.RoomBuildingPlaces[newPlace].PlacedBuilding;
    }

    private (int floorDelta, int placeDelta) GetDelta(Direction dir)
    {
        return dir switch
        {
            Direction.Left => (0, +1),
            Direction.Right => (0, -1),
            Direction.Up => (+1, 0),
            Direction.Down => (-1, 0),
            _ => (0, 0)
        };
    }

    private Direction GetInverseDirection(Direction dir)
    {
        switch (dir) {
            case Direction.Left:
                return Direction.Right;
            case Direction.Right:
                return Direction.Left;
            case Direction.Up:
                return Direction.Down;
            case Direction.Down:
                return Direction.Up;
        }
        
        return Direction.Left;
    }

    private TowerBuilding GetNeighborBuilding(Direction value)
    {
        TowerBuilding building = null;
        neighborBuildings.TryGetValue(value, out building);

        return building;
    }

    private TowerBuilding GetConnectedBuilding(Direction value)
    {
        TowerBuilding building = null;
        connectedBuildings.TryGetValue(value, out building);

        return building;
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