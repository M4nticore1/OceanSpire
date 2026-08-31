using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
        if (mask.HasFlag(NeighborMask.Left) && GetNeighborBuilding(Direction.Left) != null) {
            yield return GetNeighborBuilding(Direction.Left);
        }
        if (mask.HasFlag(NeighborMask.Right) && GetNeighborBuilding(Direction.Right) != null) {
            yield return GetNeighborBuilding(Direction.Right);
        }
        if (mask.HasFlag(NeighborMask.Up) && GetNeighborBuilding(Direction.Up) != null) {
            yield return GetNeighborBuilding(Direction.Up);
        }
        if (mask.HasFlag(NeighborMask.Down) && GetNeighborBuilding(Direction.Down) != null) {
            yield return GetNeighborBuilding(Direction.Down);
        }
    }

    public IEnumerable<TowerBuilding> ConnectedBuildingsEnumerable()
    {
        if (GetConnectedBuilding(Direction.Left) != null) {
            yield return GetConnectedBuilding(Direction.Left);
        }
        if (GetConnectedBuilding(Direction.Right) != null) {
            yield return GetConnectedBuilding(Direction.Right);
        }
        if (GetConnectedBuilding(Direction.Up) != null) {
            yield return GetConnectedBuilding(Direction.Up);
        }
        if (GetConnectedBuilding(Direction.Down) != null) {
            yield return GetConnectedBuilding(Direction.Down);
        }
    }

    protected override void OnInit(BuildingData buildingData)
    {
        var towerData = buildingData as TowerBuildingData;
        FloorIndex = towerData.FloorIndex;
        PlaceIndex = towerData.PlaceIndex;

        UpdateBuildingPlace(FloorIndex, PlaceIndex);
        UpdatePositionType();
        UpdateNeighborBuildings();

        base.OnInit(buildingData);

        UpdateConnectedBuildings();
    }

    protected override void OnDemolish()
    {
        base.OnDemolish();

        if (BuildingPlace != null) {
            BuildingPlace.RemovePlacedBuilding();
        }

        InvokeBuildingDemolished();
    }

    protected override BuildingConstruction GetConstructionToSpawn()
    {
        BuildingConstruction construction = null;

        if (constructionComponent.GetUnderConstruction()) {
            var levelData = UpgradeComponent.IsUnderUpgrade ? NextLevelDefinition as TowerBuildingLevelData : LevelDefinition as TowerBuildingLevelData;

            if (levelData != null) {
                if (BuildingPosition == BuildingPosition.Straight) {
                    construction = levelData.ConstructionStraightFrame;
                }
                else if (BuildingPosition == BuildingPosition.Corner) {
                    construction = levelData.ConstructionCornerFrame;
                }
            }
            else {
                Debug.LogError($"[{nameof(TowerBuilding)}] Level Data is not TowerBuildingLevelData at {name}!");
            }
        }
        else if (LevelDefinition is TowerBuildingLevelData levelData) {
            if (ConstructionComponent.GetUnderConstruction()) {
                if (BuildingPosition == BuildingPosition.Straight) {
                    construction = levelData.ConstructionStraightFrame;
                }
                else if (BuildingPosition == BuildingPosition.Corner) {
                    construction = levelData.ConstructionCornerFrame;
                }
            }
            else if (buildingData.ConnectionType == ConnectionType.None) {
                if (BuildingPosition == BuildingPosition.Straight) {
                    if (levelData.ConstructionStraight != null)
                        construction = levelData.ConstructionStraight;
                }
                else if (BuildingPosition == BuildingPosition.Corner) {
                    if (levelData.ConstructionCorner != null)
                        construction = levelData.ConstructionCorner;
                }
            }
            else if (buildingData.ConnectionType == ConnectionType.Horizontal) {
                if (BuildingPosition == BuildingPosition.Straight) {
                    if (GetConnectedBuilding(Direction.Left) != null && GetConnectedBuilding(Direction.Right) != null && levelData.ConstructionStraightLeftRight != null)
                        construction = levelData.ConstructionStraightLeftRight;
                    else if (GetConnectedBuilding(Direction.Left) != null && levelData.ConstructionStraightLeft != null)
                        construction = levelData.ConstructionStraightLeft;
                    else if (GetConnectedBuilding(Direction.Right) != null && levelData.ConstructionStraightRight != null)
                        construction = levelData.ConstructionStraightRight;
                    else if (GetConnectedBuilding(Direction.Left) == null && GetConnectedBuilding(Direction.Right) == null && levelData.ConstructionStraight != null)
                        construction = levelData.ConstructionStraight;
                }
                else if (BuildingPosition == BuildingPosition.Corner) {
                    if (GetConnectedBuilding(Direction.Left) != null && GetConnectedBuilding(Direction.Right) != null && levelData.ConstructionCornerLeftRight != null)
                        construction = levelData.ConstructionCornerLeftRight;
                    else if (GetConnectedBuilding(Direction.Left) != null && levelData.ConstructionCornerLeft != null)
                        construction = levelData.ConstructionCornerLeft;
                    else if (GetConnectedBuilding(Direction.Right) != null && levelData.ConstructionCornerRight != null)
                        construction = levelData.ConstructionCornerRight;
                    else if (GetConnectedBuilding(Direction.Left) == null && GetConnectedBuilding(Direction.Right) == null && levelData.ConstructionCorner != null)
                        construction = levelData.ConstructionCorner;
                }
            }
            else if (buildingData.ConnectionType == ConnectionType.Vertical) {
                if (BuildingPosition == BuildingPosition.Straight) {
                    if (GetConnectedBuilding(Direction.Up) != null && GetConnectedBuilding(Direction.Down) != null && levelData.ConstructionStraightUpDown != null)
                        construction = levelData.ConstructionStraightUpDown;
                    else if (GetConnectedBuilding(Direction.Up) != null && levelData.ConstructionStraightUp != null)
                        construction = levelData.ConstructionStraightUp;
                    else if (GetConnectedBuilding(Direction.Down) != null && levelData.ConstructionStraightDown != null)
                        construction = levelData.ConstructionStraightDown;
                    else if (GetConnectedBuilding(Direction.Up) == null && GetConnectedBuilding(Direction.Down) == null && levelData.ConstructionStraight != null)
                        construction = levelData.ConstructionStraight;
                }
                else if (BuildingPosition == BuildingPosition.Corner) {
                    if (GetConnectedBuilding(Direction.Up) != null && GetConnectedBuilding(Direction.Down) != null && levelData.ConstructionCornerUpDown != null)
                        construction = levelData.ConstructionCornerUpDown;
                    else if (GetConnectedBuilding(Direction.Up) != null && levelData.ConstructionCornerUp != null)
                        construction = levelData.ConstructionCornerUp;
                    else if (GetConnectedBuilding(Direction.Down) != null && levelData.ConstructionCornerDown != null)
                        construction = levelData.ConstructionCornerDown;
                    else if (GetConnectedBuilding(Direction.Up) == null && GetConnectedBuilding(Direction.Down) == null && levelData.ConstructionCorner != null)
                        construction = levelData.ConstructionCorner;
                }
            }
        }
        else {
            Debug.LogError($"[{nameof(TowerBuilding)}] LevelData is not TowerBuildingLevelData at {name}!");
        }

        return construction;
    }

    protected override void HandleConstructionRefresh()
    {
        base.HandleConstructionRefresh();

        UpdateConnectedBuildings();
    }

    protected override void OnLevelChange()
    {
        base.OnLevelChange();

        UpdateConnectedBuildings();
    }

    public bool ShouldBuild(BuildingPlace buildingPlace)
    {
        if (buildingPlace == null) return false;
        if (!BuildingType.ShouldBuild(buildingPlace)) return false;

        foreach (var module in BuildingModules) {
            if (module == null) continue;
            if (!module.ShouldBuild(buildingPlace)) return false;
        }

        return true;
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
            if (direction == null) continue;

            if (!visited.Add(direction))
                continue;
            if (direction.NetworkWith(target, visited))
                return true;
        }
        return false;
    }

    public bool ConnectedWith(TowerBuilding target)
    {
        return connectedBuildings.Values.ToArray().Contains(target);
    }

    public bool ShouldConnectTo(TowerBuilding target)
    {
        if (target == null) return false;
        if (connectedBuildings.ContainsValue(target)) return false;
        if (target.buildingData.BuildingId != buildingData.BuildingId) return false;
        if (target.levelComponent.Level != levelComponent.Level) return false;
        if (target.constructionComponent.GetUnderConstruction()) return false;

        return true;
    }

    public bool ShouldUnconnectFrom(TowerBuilding target)
    {
        if (target == null) return false;
        if (!connectedBuildings.ContainsValue(target)) return false;
        if (target.buildingData.BuildingId != buildingData.BuildingId) return true;
        if (target.levelComponent.Level != levelComponent.Level) return true;
        if (target.constructionComponent.GetUnderConstruction()) return true;

        return false;
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
                if (connected == null) continue;
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

        if (Definition.BuildingType == BuildingTypeEnum.Room) {
            place = BuildingsManager.Instance.GetRoomPlace(floorIndex, placeIndex);
            transform.SetParent(place.transform);
        }
        else if (Definition.BuildingType == BuildingTypeEnum.Hall) {
            place = floors[floorIndex].HallBuildingPlace;
            transform.SetParent(place.transform);
        }
        else if (Definition.BuildingType == BuildingTypeEnum.FloorFrame) {
            place = BuildingsManager.Instance.GetFloorFrameBuilding(floorIndex - 1)?.FloorBuildingPlace;
            transform.SetParent(place != null ? null : BuildingsManager.Instance.FirstFloorBuildingTransform);
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
            if (building == null) continue;

            TrySetNeighborWith(dir, building);
            building.TrySetNeighborWith(GetInverseDirection(dir), this);
        }
    }

    private void UpdateConnectedBuildings()
    {
        if (constructionComponent.GetUnderConstruction()) return;

        foreach (Direction dir in Enum.GetValues(typeof(Direction))) {
            var building = GetNeighborBuilding(dir);
            if (building == null) continue;
            if (!building.IsInited) continue;

            UpdateConencted(dir, building);
            building.UpdateConencted(GetInverseDirection(dir), this);
            building.RunUpdateConstructionCoroutine();
        }
    }

    private void TrySetNeighborWith(Direction dir, TowerBuilding neighbor)
    {
        if (!ShouldSetNeighbor(neighbor)) return;

        SetNeighbor(dir, neighbor);
    }

    private void SetNeighbor(Direction dir, TowerBuilding neighbor)
    {
        neighborBuildings[dir] = neighbor;
    }

    private void UpdateConencted(Direction dir, TowerBuilding target)
    {
        if (ShouldConnectTo(target)) {
            ConnectTo(dir, target);
        }
        else if (ShouldUnconnectFrom(target)) {
            UnconnectFrom(dir);
        }
    }

    private bool TryConnectTo(Direction dir, TowerBuilding target)
    {
        if (!ShouldConnectTo(target)) return false;

        ConnectTo(dir, target);
        return true;
    }

    private bool TryUnconnectFrom(Direction dir, TowerBuilding target)
    {
        if (!ShouldUnconnectFrom(target)) return false;

        UnconnectFrom(dir);
        return true;
    }

    private void ConnectTo(Direction dir, TowerBuilding target)
    {
        connectedBuildings[dir] = target;
    }

    private void UnconnectFrom(Direction dir)
    {
        connectedBuildings[dir] = null;
    }

    private void SetBuildingPlace(BuildingPlace place)
    {
        BuildingPlace = place;

        if (BuildingPlace != null) {
            BuildingPlace.TrySetPlaceBuilding(this);
        }
    }

    private void SetBuildingPosition(BuildingPosition position)
    {
        BuildingPosition = position;
    }

    private void InvokeBuildingDemolished()
    {
        foreach (var building in neighborBuildings.Values.ToArray()) {
            if (building == null) continue;

            building.OnNeighborBuildingDemolished(this);
        }

        foreach (var building in connectedBuildings.Values.ToArray()) {
            if (building == null) continue;

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
        RunUpdateConstructionCoroutine();
    }

    private bool ShouldSetNeighbor(TowerBuilding neighbor)
    {
        if (neighbor == null) return false;
        if (neighbor.IsDemolished) return false;

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

    private TowerBuilding GetNeighborBuilding(Direction dir)
    {
        neighborBuildings.TryGetValue(dir, out var building);

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