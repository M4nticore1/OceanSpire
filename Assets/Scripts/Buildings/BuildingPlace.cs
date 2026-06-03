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
        var buildingPrefab = ConstructionManager.Instance.BuildingToPlace as TowerBuilding;

        TowerBuildingData buildingData = new TowerBuildingData()
        {
            Id = buildingPrefab.BuildingData.BuildingId,
            InstanceId = InstancesManager.Instance.GetNextInstanceId(),
            Level = LevelData.Create(buildingPrefab.LevelComponent),
            Upgrade = UpgradeData.Create(buildingPrefab.UpgradeComponent),
            Construction = new ConstructionData()
            {
                IsUnderConstruction = true,
            },
            Crafting = CraftingModuleData.Create(buildingPrefab.GetComponent<CraftingModule>()),
            FloorIndex = FloorIndex,
            PlaceIndex = placeIndex,
        };

        var spawnedBuilding = BuildingFactory.CreateBuilding(buildingPrefab, transform, buildingData);
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
        Building buildingToPlace = ConstructionManager.Instance.BuildingToPlace;
        if (!buildingToPlace) {
            Debug.Log("buildingToPlace is not valid.");
            return false;
        }

        TowerBuilding towerBuilding = buildingToPlace as TowerBuilding;
        if (!towerBuilding) {
            Debug.Log("buildingToPlace is not TowerBuilding.");
            return false;
        }

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
        if (placeIndex != BuildingsManager.FirstBuildCityBuildingPlace) return;

        gameObject.SetActive(false);
    }

    private void UpdateFrameActivity()
    {
        if (!buildingFrame) return;

        buildingFrame.SetActive(!placedBuilding);
    }

    private void ShowBuildingPlace(BuildingPlaceState buildingPlaceState)
    {
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

        var targetBuilding = BuildingsManager.Instance.GetRoomPlace(0, BuildingsManager.FirstBuildCityBuildingPlace);

        var path = new List<Building>();
        if (neighborBuildingPlaces[Direction.Left])
            if (PathFinder.TryFindBuildingPath(neighborBuildingPlaces[Direction.Left], targetBuilding.placedBuilding, ref path)) return true;

        if (neighborBuildingPlaces[Direction.Right])
            if (PathFinder.TryFindBuildingPath(neighborBuildingPlaces[Direction.Right], targetBuilding.placedBuilding, ref path)) return true;

        if (building.GetComponent<ElevatorModule>()) {
            var up = neighborBuildingPlaces[Direction.Up];
            if (up && up.placedBuilding && up.placedBuilding.ShouldConnectTo(towerBuilding)) return true;

            var down = neighborBuildingPlaces[Direction.Down];
            if (down && down.placedBuilding && down.placedBuilding.ShouldConnectTo(towerBuilding)) return true;
        }

        return false;
    }
}
