using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerEntry
{
    public Vector3 cameraRotation;
}

public class PlayerController : MonoBehaviour
{
    private float cameraVerticalReturnMultiplier = 1.0f;
    public float cameraYawRotateAlpha { get; private set; } = 0.0f;
    private float moveStateValue = 0;
    private int moveStateIndex = 0;

    // Camera Arm

    private const int cameraMovingDistance = 24;
    private const int cameraHeightOffsetToShowBuildingStats = 0;
    private const int cameraDistanceToShowBuildingStats = 25;
    private bool isCameraEnteredBuildingStatsDistance = false;
    private Building buildingToShowStats = null;

    private float interactionsPitch = 0.0f;
    private float startInteractionsPitch = 0.0f;
    private float touchPitchInput = 0.0f;
    private float touchPitchLastInput = 0.0f;

    // Raycast
    private EventSystem eventSystem = null;
    [SerializeField] private LayerMask clickableLayers;

    public bool isInitialized { get; private set; } = false;

    private void Update()
    {

    }

    public void Init()
    {
        
    }

    public void Load(PlayerEntry saveData)
    {
        //currentCameraArmLength = -mainCamera.transform.localPosition.z;

        //if (saveData == null) {
        //    cameraYawRotateAlpha = 0.52f; }
        //else {
        //    cameraYawRotateAlpha = saveData.playerData.cameraRotation.y / 360; }

        //moveStateValue = 1f / CityManager.roomsCountPerFloor;

        //isInitialized = true;
    }

    //private void ShowStatsMenu()
    //{
    //    RaycastHit hit;
    //    Vector3 direction = new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z).normalized;
    //    //Vector3 direction = Quaternion.AngleAxis(5, mainCamera.transform.right) * mainCamera.transform.forward;
    //    if (Physics.Raycast(mainCamera.transform.position, direction, out hit, cameraDistanceToShowBuildingStats)) {
    //        Building building = hit.transform.parent?.GetComponent<Building>();
    //        if (building != buildingToShowStats) {
    //            buildingToShowStats = building;

    //            EventBus.InvokeCameraEnteredStatsMenuDistance(buildingToShowStats);
    //            isCameraEnteredBuildingStatsDistance = true;
    //        }
    //    }
    //    else if (isCameraEnteredBuildingStatsDistance) {
    //        EventBus.InvokeCameraExitedStatsMenuDistance();
    //        buildingToShowStats = null;
    //        isCameraEnteredBuildingStatsDistance = false;
    //    }
    //}

    //// Place Building
    //private void PlaceBuilding(BuildingPlace buildingPlace)
    //{
    //    EventBus.InvokeBuildingPlacePressed(buildingPlace);
    //}
}
