using System;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingPlaceState
{
    Valid,
    Warning,
    Invalid
}

public class BuildingPlace : MonoBehaviour, IClickable
{
    [SerializeField] private BuildingType buildingType = BuildingType.Room;
    public BuildingType BuildingType => buildingType;

    public int FloorIndex = 0;

    [SerializeField] private int placeIndex = 0;
    public int PlaceIndex => placeIndex;

    [SerializeField] private TowerBuilding placedBuilding;
    public TowerBuilding PlacedBuilding => placedBuilding;

    [SerializeField] private GameObject buildingZone;
    [SerializeField] private GameObject buildingFrame;
    [SerializeField] private BoxCollider boxCollider;

    private Dictionary<Direction, BuildingPlace> neighborBuildingPlaces = new();
    public IReadOnlyDictionary<Direction, BuildingPlace> NeighborBuildingPlaces => neighborBuildingPlaces;

    public bool IsClickable { get; private set; } = true;

    public event Action OnClicked;

    public static event Action<Building> OnBuildingPlaceClicked;

    private void OnEnable()
    {
        EventBus.OnConstructionStarted += OnBuildingStartPlacing;
        EventBus.OnConstructionStopped += OnStopPlacingBuildingButtonClicked;
        Building.OnBuildingInited += OnBuildingInited;
    }

    private void OnDisable()
    {
        EventBus.OnConstructionStarted -= OnBuildingStartPlacing;
        EventBus.OnConstructionStopped -= OnStopPlacingBuildingButtonClicked;
        Building.OnBuildingInited -= OnBuildingInited;
    }

    private void Start()
    {
        HideBuildingPlace();
        UpdateFrameActivity();
    }

    public void Init(int newFloorindex)
    {
        FloorIndex = newFloorindex;
        UpdateNeighborPlaces();
        HideBuildingPlace();
        UpdatePlaceActive();
    }

    public void TrySetPlaceBuilding(TowerBuilding building)
    {
        if (!CanPlaceBuilding(building)) return;

        SetPlacedBuilding(building);
    }

    public void RemovePlacedBuilding()
    {
        SetPlacedBuilding(null);
    }

    private void SetPlacedBuilding(TowerBuilding building)
    {
        placedBuilding = building;
        UpdateFrameActivity();
    }

    public bool CanPlaceBuilding(TowerBuilding building)
    {
        return building && !placedBuilding;
    }

    public void Click()
    {
        var buildingPrefab = ConstructionManager.Instance.BuildingToPlace;
        if (!buildingPrefab) {
            Debug.Log("BuildingToPlace is not valid");
            return;
        }

        var towerBuildingPrefab = buildingPrefab as TowerBuilding;
        if (!towerBuildingPrefab) {
            Debug.Log("towerBuildingPrefab is not valid");
            return;
        }

        var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        var buildingData = new TowerBuildingData()
        {
            Id = towerBuildingPrefab.BuildingData.BuildingId,
            Level = towerBuildingPrefab.LevelComponent.Level,
            Upgrade = UpgradeData.Create(towerBuildingPrefab.UpgradeComponent),

            Construction = new ConstructionData()
            {
                IsUnderConstruction = true,
                ConstructionStartTime = currentTime,
                ConstructionFinishTime = currentTime + buildingPrefab.LevelData.UpgradeTime
            },

            FloorIndex = FloorIndex,
            PlaceIndex = placeIndex,
        };

        var spawnedBuilding = BuildingFactory.CreateBuilding(towerBuildingPrefab, transform, buildingData);
        SetPlacedBuilding(spawnedBuilding);

        OnClicked?.Invoke();
        OnBuildingPlaceClicked?.Invoke(spawnedBuilding);
    }

    public void SetClickable(bool value)
    {
        IsClickable = value;
    }

    public bool ShouldClick()
    {
        return true;
    }

    public IEnumerable<BuildingPlace> GetNeighborPlaces(NeighborMask mask)
    {
        if (mask.HasFlag(NeighborMask.Left)) {
            var place = neighborBuildingPlaces[Direction.Left];
            if (place) {
                yield return place;
            }
        }
        if (mask.HasFlag(NeighborMask.Right)) {
            var place = neighborBuildingPlaces[Direction.Right];
            if (place) {
                yield return place;
            }
        }
        if (mask.HasFlag(NeighborMask.Up)) {
            var place = neighborBuildingPlaces[Direction.Up];
            if (place) {
                yield return place;
            }
        }
        if (mask.HasFlag(NeighborMask.Down)) {
            var place = neighborBuildingPlaces[Direction.Down];
            if (place) {
                yield return place;
            }
        }
    }

    private void UpdateNeighborPlaces()
    {
        neighborBuildingPlaces[Direction.Left] = CalculateNeighborPlace(Direction.Left);
        neighborBuildingPlaces[Direction.Right] = CalculateNeighborPlace(Direction.Right);
        neighborBuildingPlaces[Direction.Up] = CalculateNeighborPlace(Direction.Up);
        neighborBuildingPlaces[Direction.Down] = CalculateNeighborPlace(Direction.Down);
    }

    private BuildingPlace CalculateNeighborPlace(Direction side)
    {
        int horizontalIndexOffset = side == Direction.Left ? 1 : side == Direction.Right ? -1 : 0;
        int verticalIndexOffset = side == Direction.Up ? 1 : side == Direction.Down ? -1 : 0;
        int sideIndex = (placeIndex + horizontalIndexOffset + BuildingsManager.RoomsCountPerFloor) % BuildingsManager.RoomsCountPerFloor;
        int verticalIndex = FloorIndex + verticalIndexOffset;

        if (verticalIndex >= BuildingsManager.Instance.BuiltFloors.Count) return null;
        if (verticalIndex < 0) return null;

        var place = BuildingsManager.Instance.BuiltFloors[verticalIndex].RoomBuildingPlaces[sideIndex];
        return place;
    }

    private void OnBuildingStartPlacing(Building building)
    {
        if (!ShouldShow(building)) return;

        if (building.BuildingData.BuildingType != buildingType) {
            HideBuildingPlace();
        }
        else {
            ShowBuildingPlace(BuildingPlaceState.Valid);
        }
    }

    private void OnBuildingInited(Building building)
    {
        TowerBuilding towerBuilding = building as TowerBuilding;
        if (towerBuilding && building.GetComponent<FloorFrameModule>() && FloorIndex == towerBuilding.FloorIndex - 1) {
            UpdateNeighborPlaces();
        }

        if (placedBuilding && building != placedBuilding) return;
        if (building.BuildingData.BuildingType != buildingType) return;

        HideBuildingPlace();
    }

    private void OnStopPlacingBuildingButtonClicked()
    {
        HideBuildingPlace();
    }

    private void UpdatePlaceActive()
    {
        if (FloorIndex != 0) return;
        if (placeIndex != BuildingsManager.FirstBuildingPlace) return;

        gameObject.SetActive(false);
    }

    private void UpdateFrameActivity()
    {
        if (!buildingFrame) return;

        buildingFrame.SetActive(!placedBuilding);
    }

    private void ShowBuildingPlace(BuildingPlaceState buildingPlaceState)
    {
        if (buildingType == BuildingType.FloorFrame && BuildingsManager.Instance.BuiltFloors.Count >= BuildingsManager.Instance.MaxFloorsCount) return;

        if (buildingZone) {
            buildingZone.SetActive(true);
        }
        
        if (boxCollider) {
            boxCollider.enabled = true;
        }
    }

    private void HideBuildingPlace()
    {
        if (buildingZone) {
            buildingZone.SetActive(false);
        }

        if (boxCollider) {
            boxCollider.enabled = false;
        }
    }

    private bool ShouldShow(Building building)
    {
        var towerBuilding = building as TowerBuilding;
        if (!towerBuilding) return false;

        if (placedBuilding) return false;
        if (buildingType != BuildingType.Room) return true;

        List<Building> path;
        var targetBuilding = BuildingsManager.Instance.EntranceBuildingPlace;

        var left = neighborBuildingPlaces[Direction.Left];
        if (left && PathFinder.TryFindTowerPath(left, targetBuilding, out path)) return true;

        var right = neighborBuildingPlaces[Direction.Right];
        if (right && PathFinder.TryFindTowerPath(right, targetBuilding, out path)) return true;

        if (building.BuildingData.ConnectionType == ConnectionType.Vertical) {
            var up = neighborBuildingPlaces[Direction.Up];
            if (up && PathFinder.TryFindTowerPath(up, targetBuilding, out path)) return true;

            var down = neighborBuildingPlaces[Direction.Down];
            if (down && PathFinder.TryFindTowerPath(down, targetBuilding, out path)) return true;
        }

        return false;
    }
}
