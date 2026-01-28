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

[AddComponentMenu("")]
public class TowerBuilding : Building
{
    protected BuildingPosition buildingPosition { get; private set; } = BuildingPosition.Straight;
    public int floorIndex => buildingPlace ? buildingPlace.floorIndex : 0;
    public int placeIndex => buildingPlace ? buildingPlace.BuildingPlaceIndex : 0;

    public TowerBuilding leftNeighborBuilding = null;
    public TowerBuilding rightNeighborBuilding = null;
    public TowerBuilding upNeighborBuilding = null;
    public TowerBuilding downNeighborBuilding = null;

    public TowerBuilding leftConnectedBuilding => CheckConnectionPossibility(leftNeighborBuilding, ConnectionType.Horizontal);
    public TowerBuilding rightConnectedBuilding => CheckConnectionPossibility(rightNeighborBuilding, ConnectionType.Horizontal);
    public TowerBuilding upConnectedBuilding => CheckConnectionPossibility(upNeighborBuilding, ConnectionType.Vertical);
    public TowerBuilding downConnectedBuilding => CheckConnectionPossibility(downNeighborBuilding, ConnectionType.Vertical);

    public IEnumerable NeighborBuildings(NeighborMask mask)
    {
        if (mask.HasFlag(NeighborMask.Left))
            yield return leftNeighborBuilding;

        if (mask.HasFlag(NeighborMask.Right))
            yield return rightNeighborBuilding;

        if (mask.HasFlag(NeighborMask.Up))
            yield return upNeighborBuilding;

        if (mask.HasFlag(NeighborMask.Down))
            yield return downNeighborBuilding;
    }

    public override void InitializeBuilding(BuildingPlace buildingPlace, bool isUnderConstruction, int levelIndex, int interiorIndex = -1)
    {
        constructionComponent = GetComponent<ConstructionComponent>();
        storageComponent = GetComponent<StorageBuildingComponent>();
        productionComponent = GetComponent<ProductionBuilding>();

        this.buildingPlace = buildingPlace;
        this.LevelIndex = levelIndex;

        if (storageComponent)
            storageComponent.Initialize();

        leftNeighborBuilding = GetNeighborBuilding(Side.Left);
        rightNeighborBuilding = GetNeighborBuilding(Side.Right);
        upNeighborBuilding = GetNeighborBuilding(Side.Up);
        downNeighborBuilding = GetNeighborBuilding(Side.Down);

        if (placeIndex % 2 == 0)
            buildingPosition = BuildingPosition.Corner;
        else
            buildingPosition = BuildingPosition.Straight;

        constructionComponent.InitializeConstruction(isUnderConstruction, levelIndex);

        isInitialized = true;
    }

    protected TowerBuilding GetNeighborBuilding(Side side)
    {
        int floorIndex = this.floorIndex < CityManager.firstBuildCityFloorIndex ? CityManager.firstBuildCityFloorIndex : this.floorIndex;
        int placeIndex = this.floorIndex < CityManager.firstBuildCityFloorIndex && this.placeIndex < CityManager.firstBuildCityBuildingPlace ? CityManager.firstBuildCityBuildingPlace : this.placeIndex;

        int horizontalIndexOffset = side == Side.Left ? 1 : side == Side.Right ? -1 : 0;
        int verticalIndexOffset = side == Side.Up ? 1 : side == Side.Down ? -1 : 0;
        int sideIndex = (placeIndex + horizontalIndexOffset + CityManager.roomsCountPerFloor) % CityManager.roomsCountPerFloor;
        int verticalIndex = floorIndex + verticalIndexOffset;

        if (verticalIndex < CityManager.Instance.builtFloors.Count && verticalIndex >= 0) {
            Building building = CityManager.Instance.builtFloors[verticalIndex].roomBuildingPlaces[sideIndex].placedBuilding;
            return building as TowerBuilding;
        }
        return null;
    }

    protected TowerBuilding CheckConnectionPossibility(TowerBuilding target, ConnectionType requiredConnection)
    {
        if (!target) return null;
        if (buildingData.ConnectionType != requiredConnection) return null;
        if (target.buildingData.BuildingId != buildingData.BuildingId) return null;

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

    //private TowerBuilding GetNeightboorBuilding(Side side)
    //{
    //    int horizontalIndexOffset = side == Side.Left ? 1 : side == Side.Right ? -1 : 0;
    //    int verticalIndexOffset = side == Side.Up ? 1 : side == Side.Down ? -1 : 0;
    //    int sideIndex = (GetplaceIndex + horizontalIndexOffset + CityManager.roomsCountPerFloor) % CityManager.roomsCountPerFloor;
    //    int verticalIndex = GetfloorIndex + verticalIndexOffset;
    //    if (verticalIndex < cityManager.builtFloors.Count && verticalIndex >= 0)
    //        return cityManager.builtFloors[verticalIndex].roomBuildingPlaces[sideIndex].placedBuilding as TowerBuilding;

    //    return null;
    //}
}
