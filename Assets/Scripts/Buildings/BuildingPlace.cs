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
    private BuildingsManager buildingsManager;

    [SerializeField] private BuildingType buildingType = BuildingType.Room;
    public BuildingType BuildingType => buildingType;

    public int floorIndex { get; private set; } = 0;
    [SerializeField] private int placeIndex = 0;
    public int PlaceIndex => placeIndex;
    public int emptyBuildingPlacesAbove { get; set; } = 0;
    public int emptyBuildingPlacesBelow { get; set; } = 0;

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

    public void Init(int newFloorindex)
    {
        buildingsManager = FindAnyObjectByType<BuildingsManager>();

        floorIndex = newFloorindex;
        AssignNeighborPlaces();
        HideBuildingPlace();

        EventBus.onStartedPlacingBuilding += OnBuildingStartPlacing;
        Building.onBuildingInited += OnBuildingInited;
        EventBus.onStopPlacingBuildingButtonClicked += OnStopPlacingBuildingButtonClicked;
    }

    public void HandleBuildingInited(TowerBuilding building)
    {
        SetPlacedBuilding(building);
    }

    public void HandleBuildingDemolished()
    {
        RemovePlacedBuilding();
    }

    private void AssignNeighborPlaces()
    {
        leftPlace = GetNeighborPlace(Side.Left);
        rightPlace = GetNeighborPlace(Side.Right);
        upPlace = GetNeighborPlace(Side.Up);
        downPlace = GetNeighborPlace(Side.Down);
    }

    private BuildingPlace GetNeighborPlace(Side side)
    {
        int horizontalIndexOffset = side == Side.Left ? 1 : side == Side.Right ? -1 : 0;
        int verticalIndexOffset = side == Side.Up ? 1 : side == Side.Down ? -1 : 0;
        int sideIndex = (placeIndex + horizontalIndexOffset + BuildingsManager.RoomsCountPerFloor) % BuildingsManager.RoomsCountPerFloor;
        int verticalIndex = floorIndex + verticalIndexOffset;

        if (verticalIndex < buildingsManager.BuiltFloors.Count && verticalIndex >= 0) {
            BuildingPlace place = buildingsManager.BuiltFloors[verticalIndex].RoomBuildingPlaces[sideIndex];
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
        if (towerBuilding && building.GetComponent<FloorFrameModule>() && floorIndex == towerBuilding.floorIndex - 1) {
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

    private void SetPlacedBuilding(TowerBuilding building)
    {
        placedBuilding = building;
        AssignFrameActivity();
    }

    private void RemovePlacedBuilding()
    {
        placedBuilding = null;
        AssignFrameActivity();
    }

    private void AssignFrameActivity()
    {
        if (!buildingFrame) return;

        buildingFrame.SetActive(placedBuilding == null);
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
        TowerBuilding building = ConstructionManager.Instance.buildingToPlace as TowerBuilding;
        TowerBuilding spawnedBuilding = BuildingFactory.CreateBuilding(BuildingDataFactory.CreateBuildingData(building, floorIndex, placeIndex));

        SetPlacedBuilding(spawnedBuilding);
        onClicked?.Invoke(spawnedBuilding);
    }

    public bool CanClick()
    {
        Building buildingToPlace = ConstructionManager.Instance.buildingToPlace;
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
