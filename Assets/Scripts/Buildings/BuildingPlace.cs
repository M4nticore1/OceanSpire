using UnityEngine;

public enum BuildingPlaceState
{
    Valid,
    Warning,
    Invalid
}

public class BuildingPlace : MonoBehaviour
{
    [SerializeField] private BuildingType buildingType = BuildingType.Room;
    public BuildingType BuildingType => buildingType;

    public int floorIndex = 0;
    [SerializeField] private int buildingPlaceIndex = 0;
    public int BuildingPlaceIndex => buildingPlaceIndex;
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

    private void Awake()
    {
        buildingZoneMeshRenderer = buildingZone.GetComponent<MeshRenderer>();

        materialPropertyBlock = new MaterialPropertyBlock();
        outlineMaterialPropertyBlock = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        if (placedBuilding)
        {

        }
    }

    private void OnDisable()
    {
        
    }

    public void InitializeBuildingPlace(int newFloorindex)
    {
        floorIndex = newFloorindex;
    }

    //public void DestroyBuilding()
    //{
    //    if (placedBuilding)
    //    {
    //        boxCollider.enabled = true;

    //        //isBuildingPlaced = false;
    //        placedBuilding = null;

    //        if (buildingFrame)
    //            buildingFrame.SetActive(true);
    //    }
    //}

    public void SetPlacedBuilding(TowerBuilding building)
    {
        placedBuilding = building;
        if (buildingFrame)
            buildingFrame.SetActive(false);
    }

    public void RemoveBuildingPlaced()
    {
        placedBuilding = null;
        if (buildingFrame)
            buildingFrame.SetActive(true);
    }

    public void ShowBuildingPlace(BuildingPlaceState buildingPlaceState)
    {
        if (buildingZone)
            buildingZone.SetActive(true);
        if (boxCollider)
            boxCollider.enabled = true;

        Color mainColor = Color.black;
        Color outlineColor = Color.black;

        if (buildingPlaceState == BuildingPlaceState.Valid)
        {
            mainColor = buildingPlaceValidColor;
            outlineColor = buildingPlaceValidOutlineColor;
        }
        else if (buildingPlaceState == BuildingPlaceState.Warning)
        {
            mainColor = buildingPlaceWarningColor;
            outlineColor = buildingPlaceWarningOutlineColor;
        }
        else if (buildingPlaceState == BuildingPlaceState.Invalid)
        {
            mainColor = buildingPlaceInvalidColor;
            outlineColor = buildingPlaceInvalidOutlineColor;
        }

        if (materialPropertyBlock != null)
            materialPropertyBlock.SetColor("_BaseColor", mainColor);
        if (buildingZoneMeshRenderer)
        buildingZoneMeshRenderer.SetPropertyBlock(materialPropertyBlock, 0);

        if (outlineMaterialPropertyBlock != null)
            outlineMaterialPropertyBlock.SetColor("_OutlineColor", outlineColor);
        if (buildingZoneMeshRenderer)
            buildingZoneMeshRenderer.SetPropertyBlock(outlineMaterialPropertyBlock, 1);
    }

    public void HideBuildingPlace()
    {
        if (buildingZone)
            buildingZone.SetActive(false);
        if (boxCollider)
            boxCollider.enabled = false;
    }

    public void SetColliderSize(Vector3 NewColliderSize)
    {
        boxCollider.size = NewColliderSize;
        boxCollider.center = new Vector3(0, NewColliderSize.y / 2, 0);
    }
}
