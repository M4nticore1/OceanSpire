using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum BuildingPlaceState
{
    Valid,
    Warning,
    Invalid
}

public class BuildingPlace : MonoBehaviour, IClickable
{
    [SerializeField] private BuildingTypeEnum buildingType = BuildingTypeEnum.Room;
    public BuildingTypeEnum BuildingType => buildingType;

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

    private bool isClickable = true;
    public bool IsClickable { get { return isClickable; } set { isClickable = value; } }

    private BuildingsManager buildingsManager => BuildingsManager.Instance;

    public event Action OnClicked;

    public static event Action<Building> OnBuildingPlaceClicked;

    private void OnEnable()
    {
        EventBus.OnConstructionStarted += HandleBuildingPlacingStarted;
        EventBus.OnConstructionStopped += HandleBuildingPlacingFinished;

        Building.OnBuildingInited += HandleBuildingInited;
        Building.OnBuildingDemolished += HandleBuildingDemolished;
    }

    private void OnDisable()
    {
        EventBus.OnConstructionStarted -= HandleBuildingPlacingStarted;
        EventBus.OnConstructionStopped -= HandleBuildingPlacingFinished;

        Building.OnBuildingInited -= HandleBuildingInited;
        Building.OnBuildingDemolished -= HandleBuildingDemolished;
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
        UpdateEntrancePlaceActive();
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

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var buildingData = new TowerBuildingData()
        {
            Id = towerBuildingPrefab.Definition.BuildingId,
            Level = towerBuildingPrefab.LevelComponent.Level,
            Upgrade = UpgradeData.Default(),

            Construction = new ConstructionData()
            {
                ConstructionStartTime = currentTime,
                ConstructionFinishTime = currentTime + buildingPrefab.LevelDefinition.UpgradeTime
            },

            FloorIndex = FloorIndex,
            PlaceIndex = placeIndex,
        };

        var spawnedBuilding = BuildingFactory.CreateBuilding(towerBuildingPrefab, transform, buildingData);
        SetPlacedBuilding(spawnedBuilding);

        OnClicked?.Invoke();
        OnBuildingPlaceClicked?.Invoke(spawnedBuilding);
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

        if (verticalIndex >= buildingsManager.BuiltFloors.Count) return null;
        if (verticalIndex < 0) return null;

        var place = buildingsManager.BuiltFloors[verticalIndex].RoomBuildingPlaces[sideIndex];
        return place;
    }

    private void HandleBuildingPlacingStarted(Building building)
    {
        UpdatePlaceShown(building);
    }

    private void HandleBuildingPlacingFinished()
    {
        HideBuildingPlace();
    }

    private void HandleBuildingInited(Building building)
    {
        TowerBuilding towerBuilding = building as TowerBuilding;
        if (towerBuilding && building.GetComponent<FloorFrameModule>() && FloorIndex == towerBuilding.FloorIndex - 1) {
            UpdateNeighborPlaces();
        }

        if (placedBuilding && building != placedBuilding) return;
        if (building.Definition.BuildingType != buildingType) return;

        HideBuildingPlace();
    }

    private void HandleBuildingDemolished(Building building)
    {
        UpdatePlaceShown(building);
    }

    private void UpdateEntrancePlaceActive()
    {
        if (buildingsManager.GetEntranceBuildingPlace() == this) {
            gameObject.SetActive(false);
        }
    }

    private void UpdateFrameActivity()
    {
        if (buildingFrame != null) {
            buildingFrame.SetActive(!placedBuilding);
        }
    }

    private void UpdatePlaceShown(Building building)
    {
        if (ShouldShow(building)) {
            ShowBuildingPlace(BuildingPlaceState.Valid);
        }
        else {
            HideBuildingPlace();
        }
    }

    private void ShowBuildingPlace(BuildingPlaceState buildingPlaceState)
    {
        var maxFloorsCount = buildingsManager.MaxFloorsCount;
        if (buildingType == BuildingTypeEnum.FloorFrame && maxFloorsCount > 0 && buildingsManager.BuiltFloors.Count >= maxFloorsCount) return;

        if (buildingZone != null) {
            buildingZone.SetActive(true);
        }
        
        if (boxCollider != null) {
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
        if (building == null) return false;
        if (placedBuilding != null) return false;

        var towerBuilding = building as TowerBuilding;
        if (towerBuilding == null) return false;
        if (!towerBuilding.ShouldBuild(this)) return false;

        if (building.Definition.BuildingType != buildingType) return false;

        return true;
    }
}
