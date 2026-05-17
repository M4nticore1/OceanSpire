using System;
using System.Collections;
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

    public BuildingPlace leftPlace { get; private set; }
    public BuildingPlace rightPlace { get; private set; }
    public BuildingPlace upPlace { get; private set; }
    public BuildingPlace downPlace { get; private set; }

    private bool isShowed = false;

    public static event Action<Building> onClicked;

    public IEnumerable NeighborPlaces(NeighborMask mask)
    {
        if (mask.HasFlag(NeighborMask.Left)) {
            yield return leftPlace;
        }
        if (mask.HasFlag(NeighborMask.Right)) {
            yield return rightPlace;
        }
        if (mask.HasFlag(NeighborMask.Up)) {
            yield return upPlace;
        }
        if (mask.HasFlag(NeighborMask.Down)) {
            yield return downPlace;
        }
    }

    private void OnEnable()
    {
        EventBus.OnConstructionStarted += OnBuildingStartPlacing;
        Building.onBuildingInited += OnBuildingInited;
        EventBus.OnConstructionStopped += OnStopPlacingBuildingButtonClicked;
    }

    private void OnDisable()
    {
        EventBus.OnConstructionStarted -= OnBuildingStartPlacing;
        Building.onBuildingInited -= OnBuildingInited;
        EventBus.OnConstructionStopped -= OnStopPlacingBuildingButtonClicked;
    }

    private void Start()
    {
        HideBuildingPlace();
        UpdateFrameActivity();
    }

    public void Init(int newFloorindex)
    {
        FloorIndex = newFloorindex;
        AssignNeighborPlaces();
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

    private void AssignNeighborPlaces()
    {
        leftPlace = GetNeighborPlace(Direction.Left);
        rightPlace = GetNeighborPlace(Direction.Right);
        upPlace = GetNeighborPlace(Direction.Up);
        downPlace = GetNeighborPlace(Direction.Down);
    }

    private BuildingPlace GetNeighborPlace(Direction side)
    {
        int horizontalIndexOffset = side == Direction.Left ? 1 : side == Direction.Right ? -1 : 0;
        int verticalIndexOffset = side == Direction.Up ? 1 : side == Direction.Down ? -1 : 0;
        int sideIndex = (placeIndex + horizontalIndexOffset + BuildingsManager.RoomsCountPerFloor) % BuildingsManager.RoomsCountPerFloor;
        int verticalIndex = FloorIndex + verticalIndexOffset;

        if (verticalIndex < BuildingsManager.Instance.BuiltFloors.Count && verticalIndex >= 0) {
            BuildingPlace place = BuildingsManager.Instance.BuiltFloors[verticalIndex].RoomBuildingPlaces[sideIndex];
            return place;
        }
        return null;
    }

    private void OnBuildingStartPlacing(Building building)
    {
        if (placedBuilding) return;

        if (building.BuildingData.BuildingType != buildingType) {
            if (isShowed) {
                HideBuildingPlace();
            }
            return;
        }

        ShowBuildingPlace(BuildingPlaceState.Valid);
    }

    private void OnBuildingInited(Building building)
    {
        TowerBuilding towerBuilding = building as TowerBuilding;
        if (towerBuilding && building.GetComponent<FloorFrameModule>() && FloorIndex == towerBuilding.FloorIndex - 1) {
            AssignNeighborPlaces();
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

        isShowed = true;

        //Color mainColor = Color.black;
        //Color outlineColor = Color.black;

        //if (buildingPlaceState == BuildingPlaceState.Valid)
        //{
        //    mainColor = buildingPlaceValidColor;
        //    outlineColor = buildingPlaceValidOutlineColor;
        //}
        //else if (buildingPlaceState == BuildingPlaceState.Warning)
        //{
        //    mainColor = buildingPlaceWarningColor;
        //    outlineColor = buildingPlaceWarningOutlineColor;
        //}
        //else if (buildingPlaceState == BuildingPlaceState.Invalid)
        //{
        //    mainColor = buildingPlaceInvalidColor;
        //    outlineColor = buildingPlaceInvalidOutlineColor;
        //}

        //if (materialPropertyBlock != null)
        //    materialPropertyBlock.SetColor("_BaseColor", mainColor);
        //if (buildingZoneMeshRenderer)
        //buildingZoneMeshRenderer.SetPropertyBlock(materialPropertyBlock, 0);

        //if (outlineMaterialPropertyBlock != null)
        //    outlineMaterialPropertyBlock.SetColor("_OutlineColor", outlineColor);
        //if (buildingZoneMeshRenderer)
        //    buildingZoneMeshRenderer.SetPropertyBlock(outlineMaterialPropertyBlock, 1);
    }

    private void HideBuildingPlace()
    {
        if (buildingZone) {
            buildingZone.SetActive(false);
        }

        if (boxCollider) {
            boxCollider.enabled = false;
        }

        isShowed = false;
    }

    // Events
    public void Click()
    {
        TowerBuilding building = ConstructionManager.Instance.BuildingToPlace as TowerBuilding;

        TowerBuildingData buildingData = TowerBuildingData.Create(building);
        buildingData.InstanceId = InstancesManager.Instance.GetNextInstanceId();
        buildingData.FloorIndex = FloorIndex;
        buildingData.PlaceIndex = placeIndex;
        buildingData.Construction.ConstructionTime = 0f;
        buildingData.Construction.IsUnderConstruction = true;

        TowerBuilding spawnedBuilding = BuildingFactory.CreateBuilding(building, transform, buildingData);
        SetPlacedBuilding(spawnedBuilding);

        onClicked?.Invoke(spawnedBuilding);
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
}
