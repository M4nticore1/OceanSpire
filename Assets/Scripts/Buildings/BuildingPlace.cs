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

    public int floorIndex { get; private set; } = 0;
    [SerializeField] private int placeIndex = 0;
    public int PlaceIndex => placeIndex;
    public int emptyBuildingPlacesAbove { get; set; } = 0;
    public int emptyBuildingPlacesBelow { get; set; } = 0;

    //public bool isBuildingPlaced = false;
    public TowerBuilding placedBuilding = null;

    [SerializeField] private GameObject buildingZone = null;
    [SerializeField] private GameObject buildingFrame = null;
    [SerializeField] private MeshRenderer buildingZoneMeshRenderer = null;
    [SerializeField] private BoxCollider boxCollider = null;

    private MaterialPropertyBlock materialPropertyBlock = null;
    private MaterialPropertyBlock outlineMaterialPropertyBlock = null;

    private Color buildingPlaceValidColor = new Color(0.2f, 1, 0.2f, 1);
    private Color buildingPlaceWarningColor = new Color(1, 1, 0, 1);
    private Color buildingPlaceInvalidColor = new Color(1, 0, 0, 1);

    private Color buildingPlaceValidOutlineColor = new Color(0.035f, 1, 0, 1);
    private Color buildingPlaceWarningOutlineColor = new Color(1, 1, 0, 1);
    private Color buildingPlaceInvalidOutlineColor = new Color(1, 0, 0, 1);

    public BuildingPlace leftPlace { get; private set; }
    public BuildingPlace rightPlace { get; private set; }
    public BuildingPlace upPlace { get; private set; }
    public BuildingPlace downPlace { get; private set; }

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

    private void Awake()
    {
        buildingZoneMeshRenderer = buildingZone.GetComponent<MeshRenderer>();

        materialPropertyBlock = new MaterialPropertyBlock();
        outlineMaterialPropertyBlock = new MaterialPropertyBlock();
    }

    public void Init(int newFloorindex)
    {
        floorIndex = newFloorindex;
        ApplyNeighborPlace();
        HideBuildingPlace();

        EventBus.onBuildingStartedPlacing += OnBuildingStartPlacing;
        EventBus.onBuildingFinishedPlacing += OnBuildingFinishPlacing;
        EventBus.onBuildingInitialized += OnBuildingInitialized;
    }

    private BuildingPlace GetNeighborPlace(Side side)
    {
        int horizontalIndexOffset = side == Side.Left ? 1 : side == Side.Right ? -1 : 0;
        int verticalIndexOffset = side == Side.Up ? 1 : side == Side.Down ? -1 : 0;
        int sideIndex = (placeIndex + horizontalIndexOffset + CityManager.roomsCountPerFloor) % CityManager.roomsCountPerFloor;
        int verticalIndex = floorIndex + verticalIndexOffset;

        if (verticalIndex < CityManager.Instance.BuiltFloors.Count && verticalIndex >= 0) {
            return CityManager.Instance.BuiltFloors[verticalIndex].roomBuildingPlaces[sideIndex];
        }
        return null;
    }

    private void ApplyNeighborPlace()
    {
        leftPlace = GetNeighborPlace(Side.Left);
        rightPlace = GetNeighborPlace(Side.Right);
        upPlace = GetNeighborPlace(Side.Up);
        downPlace = GetNeighborPlace(Side.Down);
    }

    private void OnBuildingStartPlacing(Building building)
    {
        if (placedBuilding) return;
        if (building.BuildingData.BuildingType != buildingType) return;

        ShowBuildingPlace(BuildingPlaceState.Valid);
    }

    private void OnBuildingFinishPlacing(Building building)
    {
        if (placedBuilding) return;
        if (building.BuildingData.BuildingType != buildingType) return;

        HideBuildingPlace();
    }

    private void OnBuildingInitialized(Building building)
    {
        TowerBuilding towerBuilding = building as TowerBuilding;
        if (towerBuilding.floorIndex != floorIndex || towerBuilding.placeIndex != placeIndex || towerBuilding.BuildingData.BuildingType != buildingType) return;

        SetPlacedBuilding(towerBuilding);
    }

    private void SetPlacedBuilding(TowerBuilding building)
    {
        placedBuilding = building;
        if (buildingFrame)
            buildingFrame.SetActive(false);
    }

    private void RemoveBuildingPlaced()
    {
        placedBuilding = null;
        if (buildingFrame)
            buildingFrame.SetActive(true);
    }

    private void ShowBuildingPlace(BuildingPlaceState buildingPlaceState)
    {
        if (buildingZone)
            buildingZone.SetActive(true);
        if (boxCollider)
            boxCollider.enabled = true;

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
        if (buildingZone)
            buildingZone.SetActive(false);
        if (boxCollider)
            boxCollider.enabled = false;
    }

    //public void SetColliderSize(Vector3 NewColliderSize)
    //{
    //    boxCollider.size = NewColliderSize;
    //    boxCollider.center = new Vector3(0, NewColliderSize.y / 2, 0);
    //}

    // Events
    public void Click()
    {
        TowerBuilding building = ConstructionManager.Instance.buildingToPlace as TowerBuilding;
        int id = building.BuildingData.BuildingId;
        TowerBuildingEntry data = new TowerBuildingEntry(floorIndex, placeIndex);

        BuildingFactory.CreateBuilding(id, data);
    }

    public bool CanClick()
    {
        Building building = ConstructionManager.Instance.buildingToPlace;
        if (!building) {
            Debug.Log("building is not valid.");
            return false;
        }

        TowerBuilding towerBuilding = building as TowerBuilding;
        if (!towerBuilding) {
            Debug.Log("buildingToPlace is not valid.");
            return false;
        }

        return true;
    }
}
