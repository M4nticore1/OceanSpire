using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // Main
    [SerializeField] private PlayerUIManager uiManager = null;

    // Camera Movement
    [SerializeField] private Camera mainCamera = null;
    [SerializeField] private GameObject cameraHolder = null;

    public Vector3 cameraVerticalPosition { get; private set; } = Vector3.zero;
    public Vector3 cameraHorizontalPosition { get; private set; } = Vector3.zero;

    private bool isCameraMoving => isKeyboardMoveButtonPressed || isMouseMoveButtonPressed || isTouchscreenMoveButtonPressed;
    private Vector2 cameraMoveVelocity => keyboardCameraMove * keyboardCameraMoveSensitivity + mouseCameraMove * mouseCameraMoveSensitivity + touchscreenCameraMove * touchscreenCameraMoveSensitivity;
    private Vector2 keyboardCameraMoveInput => cameraMoveKeyboardIA.ReadValue<Vector2>();
    private Vector2 mouseCameraMoveInput => cameraMoveMouseIA.ReadValue<Vector2>();
    private Vector2 touchscreenCameraMoveInput => cameraMoveTouchscreenIA.ReadValue<Vector2>();

    private bool isKeyboardMoveButtonPressed => cameraMoveKeyboardButtonIA.IsPressed();
    private bool isMouseMoveButtonPressed => cameraMoveMouseButtonIA.IsPressed();
    private bool isTouchscreenMoveButtonPressed => cameraMoveTouchscreenButtonIA.IsPressed();

    private Vector2 keyboardCameraMove;
    private Vector2 mouseCameraMove;
    private Vector2 touchscreenCameraMove;

    private const float keyboardCameraMoveSensitivity = 100f;
    private const float mouseCameraMoveSensitivity = 10f;
    private const float touchscreenCameraMoveSensitivity = 3f;
    private const float cameraStopMoveSpeed = 6.0f;

    private const float cameraVerticalBoundaryPadding = 10.0f;
    private const float cameraVerticalReturnSpeed = 5.0f;

    private float cameraVerticalReturnMultiplier = 1.0f;
    public float cameraYawRotateAlpha { get; private set; } = 0.0f;
    private float moveStateValue = 0;
    private int moveStateIndex = 0;

    // Camera Arm
    private float currentCameraArmLength = 0.0f;
    private const float minCameraArmLength = 25.0f;
    private const float maxCameraArmLength = 100.0f;

    private const float nearCameraArmBoundaryPadding = 10.0f;
    private const float farCameraArmBoundaryPadding = 20.0f;
    private const float cameraArmReturnSpeed = 4.0f;

    private float cameraArmMoveMultiplier = 1.0f;
    private float currentCameraDistance = 0.0f;

    private const int cameraMovingDistance = 24;
    private const int cameraDistanceToShowBuildingStats = 15;
    private const int cameraHeightOffsetToShowBuildingStats = 0;

    private float currentCameraZoomVelocity = 0f;
    private const float cameraZoomIntensity = 6f;

    // Camera Shake
    private const float cameraShakeAmplitude = 1f;
    private const float cameraShakeSpeed = 0.5f;
    private Vector3 currentCameraShakeForce = Vector3.zero;
    private Vector3 currentCameraShakeRotation = Vector3.zero;

    // Input System
    private bool isPrimaryInteractionPressed = false;
    private bool isSecondaryInteractionPressed = false;

    [SerializeField] private InputActionAsset mainInputActionsAsset = null;
    private InputActionMap touchInputActionMap = null;

    private InputAction mousePositionIA = null;

    private InputAction primaryInteractionPressIA = null;
    private InputAction primaryInteractionPositionIA = null;
    private InputAction primaryInteractionDeltaIA = null;
    private InputAction secondaryInteractionPressIA = null;
    private InputAction secondaryInteractionPositionIA = null;
    private InputAction secondaryInteractionDeltaIA = null;

    private InputAction cameraMoveKeyboardButtonIA = null;
    private InputAction cameraMoveMouseButtonIA = null;
    private InputAction cameraMoveTouchscreenButtonIA = null;

    private InputAction cameraMoveKeyboardIA = null;
    private InputAction cameraMoveMouseIA = null;
    private InputAction cameraMoveTouchscreenIA = null;
    private InputAction cameraZoomIA = null;

    private Vector2 primaryInteractionStartPosition = Vector2.zero;
    private Vector2 primaryInteractionPosition = Vector2.zero;
    private Vector2 primaryInteractionDelta => primaryInteractionDeltaIA.ReadValue<Vector2>();

    private Vector2 secondaryInteractionStartPosition = Vector2.zero;
    private Vector2 secondaryInteractionPosition = Vector2.zero;
    private Vector2 secondaryInteractionDelta = Vector2.zero;

    private float interactionsPitch = 0.0f;
    private float startInteractionsPitch = 0.0f;
    private float touchPitchInput = 0.0f;
    private float touchPitchLastInput = 0.0f;
    private float touchPitchVelocity = 0.0f;
    private const float touchPitchSensitivity = 0.1f;
    private const float pitchStopSpeed = 25.0f;

    // Building
    [HideInInspector] public Building buildingToPlace = null;

    public ISelectable selectedObject = null;

    // Raycast
    private EventSystem eventSystem = null;
    private GraphicRaycaster graphicRaycaster = null;
    [SerializeField] private LayerMask clickableLayers;

    public bool isInitialized { get; private set; } = false;

    public void Initialize()
    {
        graphicRaycaster = uiManager.GetComponent<GraphicRaycaster>();
        SetInputSystem();
    }

    private void OnEnable()
    {
        // Gameplay Events
        BuildingWidget.OnStartPlacingConstruction += OnBuildingStartPlacing;
        GameManager.OnConstructionPlaced += OnConstructionPlaced;
        PlayerUIManager.OnBuildStopPlacing += OnConstructionPlaced;

        touchInputActionMap.Enable();

        // Primary Interaction
        primaryInteractionPressIA.performed += OnPrimaryInteractionStarted;
        primaryInteractionPressIA.canceled += OnPrimaryInteractionEnded;

        // Secondary Interaction
        secondaryInteractionPressIA.performed += OnSecondaryTouchStarted;
        secondaryInteractionPressIA.canceled += OnSecondaryTouchEnded;

        secondaryInteractionPositionIA.performed += OnSecondaryInteractionPosition;
        secondaryInteractionPositionIA.canceled += OnSecondaryInteractionPosition;

        secondaryInteractionDeltaIA.performed += OnSecondaryInteractionDelta;
        secondaryInteractionDeltaIA.canceled += OnSecondaryInteractionDelta;

        // Camera
        cameraZoomIA.performed += ZoomCamera;
    }

    private void OnDisable()
    {

        BuildingWidget.OnStartPlacingConstruction -= OnBuildingStartPlacing;
        GameManager.OnConstructionPlaced -= OnConstructionPlaced;
        PlayerUIManager.OnBuildStopPlacing -= OnConstructionPlaced;

        touchInputActionMap.Disable();

        // Primary Interaction
        primaryInteractionPressIA.performed -= OnPrimaryInteractionStarted;
        primaryInteractionPressIA.canceled -= OnPrimaryInteractionEnded;

        // Secondary Interaction
        secondaryInteractionPressIA.performed -= OnSecondaryTouchStarted;
        secondaryInteractionPressIA.canceled -= OnSecondaryTouchEnded;

        secondaryInteractionPositionIA.performed -= OnSecondaryInteractionPosition;
        secondaryInteractionPositionIA.canceled -= OnSecondaryInteractionPosition;

        secondaryInteractionDeltaIA.performed -= OnSecondaryInteractionDelta;
        secondaryInteractionDeltaIA.canceled -= OnSecondaryInteractionDelta;

        // Camera
        cameraZoomIA.performed -= ZoomCamera;
    }

    private void SetInputSystem()
    {
        if (mainInputActionsAsset != null)
        {
            touchInputActionMap = mainInputActionsAsset.FindActionMap("Gameplay");

            if (touchInputActionMap != null)
            {
                mousePositionIA = touchInputActionMap.FindAction("MousePosition");

                primaryInteractionPressIA = touchInputActionMap.FindAction("PrimaryInteractionPress");
                primaryInteractionPositionIA = touchInputActionMap.FindAction("PrimaryInteractionPosition");
                primaryInteractionDeltaIA = touchInputActionMap.FindAction("PrimaryInteractionDelta");

                secondaryInteractionPressIA = touchInputActionMap.FindAction("SecondaryInteractionPress");
                secondaryInteractionPositionIA = touchInputActionMap.FindAction("SecondaryInteractionPosition");
                secondaryInteractionDeltaIA = touchInputActionMap.FindAction("SecondaryInteractionDelta");

                cameraMoveKeyboardIA = touchInputActionMap.FindAction("CameraMoveKeyboard");
                cameraMoveMouseIA = touchInputActionMap.FindAction("CameraMoveMouse");
                cameraMoveTouchscreenIA = touchInputActionMap.FindAction("CameraMoveTouchScreen");
                cameraZoomIA = touchInputActionMap.FindAction("CameraZoom");

                cameraMoveKeyboardButtonIA = touchInputActionMap.FindAction("CameraMoveKeyboardButton");
                cameraMoveMouseButtonIA = touchInputActionMap.FindAction("CameraMoveMouseButton");
                cameraMoveTouchscreenButtonIA = touchInputActionMap.FindAction("CameraMoveTouchscreenButton");
            }
            else
                Debug.Log("void PlayerController : SetInputSystem() touchInputActionMap is NULL");
        }
        else
            Debug.Log("void PlayerController : SetInputSystem() inputActions is NULL");
    }

    public void Load(SaveData saveData)
    {
        uiManager.InitializeUIManager(this);

        currentCameraArmLength = -mainCamera.transform.localPosition.z;

        if (saveData == null) {
            cameraYawRotateAlpha = 0.5f; }
        else {
            cameraYawRotateAlpha = saveData.cameraYawRotation; }

        cameraYawRotateAlpha = 0.52f;

        moveStateValue = 1f / GameManager.roomsCountPerFloor;

        isInitialized = true;
    }

    public void Tick()
    {
        if (isInitialized)
        {
            CameraMovement();
            CameraZoom();
            CameraShake();

            Vector3 localPosition = mainCamera.transform.localPosition;
            mainCamera.transform.localPosition = new Vector3(localPosition.x, localPosition.y, -currentCameraArmLength);
            mainCamera.transform.localRotation = Quaternion.Euler(currentCameraShakeRotation);

            int placeIndex;
            if (moveStateIndex <= 5)
                placeIndex = (5 - moveStateIndex) % 6;
            else
                placeIndex = 13 - moveStateIndex;

            Building buildingToShowStats = GameManager.Instance.GetBuildingByIndex(GameManager.GetFloorIndexByHeight(cameraHolder.transform.position.y + cameraHeightOffsetToShowBuildingStats), placeIndex);

            if (currentCameraDistance <= cameraDistanceToShowBuildingStats) {
                if (buildingToShowStats)
                    uiManager.OpenBuildingStatsPanel(buildingToShowStats);
                else
                    uiManager.CloseBuildingStatsPanel();
            }
            else
                uiManager.CloseBuildingStatsPanel();
        }
    }

    private void CameraMovement()
    {
        if (!cameraHolder) {
            Debug.LogError("cameraHolder is NULL");
            return;
        }

        if (isKeyboardMoveButtonPressed)
            keyboardCameraMove = keyboardCameraMoveInput;
        else
            keyboardCameraMove = Vector2.Lerp(keyboardCameraMove, Vector2.zero, cameraStopMoveSpeed * Time.deltaTime);
        if (isMouseMoveButtonPressed)
            mouseCameraMove = mouseCameraMoveInput;
        else
            mouseCameraMove = Vector2.Lerp(mouseCameraMove, Vector2.zero, cameraStopMoveSpeed * Time.deltaTime);
        if (isTouchscreenMoveButtonPressed)
            touchscreenCameraMove = touchscreenCameraMoveInput;
        else
            touchscreenCameraMove = Vector2.Lerp(touchscreenCameraMove, Vector2.zero, cameraStopMoveSpeed * Time.deltaTime);

        // Keybord Moving
        if (!isCameraMoving) {
            if (cameraHolder.transform.position.y > GameManager.Instance.cityHeight || cameraHolder.transform.position.y < 0.0f) {
                Vector3 cameraPosition = cameraHolder.transform.position;
                float targetHeight = cameraHolder.transform.position.y < 0f ? 0f : GameManager.Instance.cityHeight;
                cameraHolder.transform.position = math.lerp(cameraHolder.transform.position, new Vector3(cameraPosition.x, targetHeight, cameraPosition.z), cameraVerticalReturnSpeed * Time.deltaTime);
            }
        }

        // Return Vertical Position
        if (cameraHolder.transform.position.y > GameManager.Instance.cityHeight)
            cameraVerticalReturnMultiplier = cameraHolder.transform.position.y - GameManager.Instance.cityHeight /*math.pow(((cityManager.cityHeight - cameraHolder.transform.position.y) + cameraHeightBoundaryPadding) / cameraHeightBoundaryPadding, 2.0f)*/;
        else if (cameraHolder.transform.position.y < 0.0f)
            cameraVerticalReturnMultiplier = -cameraHolder.transform.position.y /*((0 - math.abs(cameraHolder.transform.position.y)) + cameraHeightBoundaryPadding) / cameraHeightBoundaryPadding*/;
        else
            cameraVerticalReturnMultiplier = 0f;

        // Add Move
        float multiplier = 1f;
        float cameraHeight = math.abs(cameraHolder.transform.position.y);
        if (cameraHolder.transform.position.y > GameManager.Instance.cityHeight && cameraMoveVelocity.y > 0f)
            multiplier = 1f - math.clamp((cameraHeight - GameManager.Instance.cityHeight) / cameraVerticalBoundaryPadding, 0f, 1f);
        else if (cameraHolder.transform.position.y < 0f && cameraMoveVelocity.y < 0f)
            multiplier = 1f - math.clamp(cameraHeight / cameraVerticalBoundaryPadding, 0f, 1f);

        cameraHolder.transform.position += new Vector3(0, cameraMoveVelocity.y, 0) * multiplier * Time.deltaTime;
        Vector3 eulers = cameraHolder.transform.eulerAngles;
        eulers.y += cameraMoveVelocity.x * Time.deltaTime;
        cameraHolder.transform.eulerAngles = eulers;

        // Square Move
        float alpha = 1f - cameraHolder.transform.eulerAngles.y / 360f + 0.125f;
        Vector2 pos = SquareLoop(alpha, 16f, 0.5f);
        cameraHolder.transform.position = new Vector3(pos.x, cameraHolder.transform.position.y, pos.y);
    }

    Vector2 SquareLoop(float t, float fullSize, float corner)
    {
        t = Mathf.Repeat(t, 1f);
        float halfSize = fullSize / 2;

        float seg = 1f / 4f;

        if (t < seg) // Bottom → Right
        {
            float k = t / seg;
            return new Vector2(Mathf.Lerp(-halfSize, halfSize, Smooth(k, corner)), -halfSize);
        }
        else if (t < seg * 2f) // Right → Top
        {
            float k = (t - seg) / seg;
            return new Vector2(halfSize, Mathf.Lerp(-halfSize, halfSize, Smooth(k, corner)));
        }
        else if (t < seg * 3f) // Top → Left
        {
            float k = (t - seg * 2f) / seg;
            return new Vector2(Mathf.Lerp(halfSize, -halfSize, Smooth(k, corner)), halfSize);
        }
        else // Left → Bottom
        {
            float k = (t - seg * 3f) / seg;
            return new Vector2(-halfSize, Mathf.Lerp(halfSize, -halfSize, Smooth(k, corner)));
        }
    }

    float Smooth(float x, float corner)
    {
        return Mathf.SmoothStep(0f, 1f, Mathf.Lerp(x, x * x * (3 - 2 * x), corner));
    }

    private void ZoomCamera(InputAction.CallbackContext context)
    {
        currentCameraZoomVelocity = context.ReadValue<float>();
        Vector3 localPostion = math.abs(mainCamera.transform.localPosition);
        float multiplier = 1f;

        if (currentCameraArmLength < minCameraArmLength && currentCameraZoomVelocity > 0)
            multiplier = 1f - math.clamp((minCameraArmLength - currentCameraArmLength) / nearCameraArmBoundaryPadding, 0f, 1f);
        else if (currentCameraArmLength > maxCameraArmLength && currentCameraZoomVelocity < 0)
            multiplier = 1f - math.clamp((currentCameraArmLength - maxCameraArmLength) / farCameraArmBoundaryPadding, 0f, 1f);

        currentCameraArmLength -= cameraZoomIntensity * currentCameraZoomVelocity * multiplier;
    }

    private void CameraZoom()
    {
        if (isPrimaryInteractionPressed && isSecondaryInteractionPressed)
        {
            currentCameraZoomVelocity = touchPitchVelocity * cameraZoomIntensity;

            if (currentCameraArmLength > maxCameraArmLength && touchPitchVelocity < 0)
                cameraArmMoveMultiplier = 1.0f - ((currentCameraArmLength - maxCameraArmLength) / farCameraArmBoundaryPadding);
            else if (currentCameraArmLength < minCameraArmLength && touchPitchVelocity > 0)
                cameraArmMoveMultiplier = 1.0f - (math.abs(minCameraArmLength - currentCameraArmLength) / nearCameraArmBoundaryPadding);
            else
                cameraArmMoveMultiplier = 1;

            cameraArmMoveMultiplier = math.clamp(cameraArmMoveMultiplier, 0, 1);

            currentCameraArmLength -= touchPitchVelocity * touchPitchSensitivity * cameraArmMoveMultiplier;
            currentCameraArmLength = math.clamp(currentCameraArmLength, minCameraArmLength - nearCameraArmBoundaryPadding, maxCameraArmLength + farCameraArmBoundaryPadding);
        }
        else
        {
            touchPitchVelocity = math.lerp(touchPitchVelocity, 0, pitchStopSpeed * Time.deltaTime);
            currentCameraArmLength -= touchPitchVelocity * touchPitchSensitivity * cameraArmMoveMultiplier;

            float targetLength = currentCameraArmLength > maxCameraArmLength ? maxCameraArmLength : currentCameraArmLength < minCameraArmLength ? minCameraArmLength : currentCameraArmLength;
            if (currentCameraArmLength != targetLength)
            {
                currentCameraArmLength = math.lerp(currentCameraArmLength, targetLength, cameraArmReturnSpeed * Time.deltaTime);
            }

            currentCameraDistance = Vector3.Distance(cameraHolder.transform.position, mainCamera.transform.position);
        }
    }

    private void CameraShake()
    {
        currentCameraShakeForce = new Vector3(math.sin(Time.time * cameraShakeSpeed) * cameraShakeAmplitude, math.cos(Time.time * cameraShakeSpeed / 2) * cameraShakeAmplitude, 0);

        currentCameraShakeRotation = math.lerp(currentCameraShakeRotation, currentCameraShakeForce, cameraShakeSpeed * Time.deltaTime);
    }

    private void OnPrimaryInteractionStarted(InputAction.CallbackContext context)
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            primaryInteractionStartPosition = primaryInteractionPositionIA.ReadValue<Vector2>();
        else
            primaryInteractionStartPosition = mousePositionIA.ReadValue<Vector2>();

        startInteractionsPitch = Vector2.Distance(primaryInteractionStartPosition, secondaryInteractionPosition);
        isPrimaryInteractionPressed = true;
    }

    private void OnPrimaryInteractionEnded(InputAction.CallbackContext context)
    {
        if (!isSecondaryInteractionPressed) {
            // Get Pointer Position
            var device = context.control.device;

            if (device is Touchscreen)
                primaryInteractionPosition = primaryInteractionPositionIA.ReadValue<Vector2>();
            else if (device is Mouse)
                primaryInteractionPosition = mousePositionIA.ReadValue<Vector2>();

            // Main
            if (primaryInteractionStartPosition == primaryInteractionPosition) {
                PointerEventData pointerEventData = new PointerEventData(eventSystem);
                pointerEventData.position = primaryInteractionPosition;
                List<RaycastResult> results = new List<RaycastResult>();
                graphicRaycaster.Raycast(pointerEventData, results);

                if (results.Count == 0) {
                    Ray ray = mainCamera.ScreenPointToRay(primaryInteractionPosition);

                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit)) {
                        GameObject hitted = hit.collider.gameObject;

                        if (buildingToPlace) {
                            BuildingPlace hittedBuildingPlace = hitted.GetComponent<BuildingPlace>();

                            if (hittedBuildingPlace && !hittedBuildingPlace.placedBuilding) {
                                PlaceConstruction(hittedBuildingPlace);
                            }
                        }
                        else {
                            LootContainer hittedLootContainer = hitted.GetComponent<LootContainer>();
                            Resident hittedResident = hitted.GetComponent<Resident>();

                            Transform parent = hitted.transform.parent;
                            Building hittedBuilding = parent ? parent.GetComponent<Building>() : null;
                            BuildingConstruction buildingConstruction = parent ? parent.GetComponent<BuildingConstruction>() : null;
                            Boat hittedBoat = parent ? parent.GetComponent<Boat>() : null;

                            if (!hittedBuilding && buildingConstruction)
                                hittedBuilding = buildingConstruction.transform.parent.GetComponent<Building>();

                            ISelectable selected = null;
                            if (hittedBuilding) {
                                ProductionBuilding hittedProductionBuilding = hittedBuilding.GetComponent<ProductionBuilding>();

                                if (hittedProductionBuilding && hittedProductionBuilding.isReadyToCollect) {
                                    CollectItems(hittedProductionBuilding.TakeProducedItem());
                                    Deselect();
                                }
                                else {
                                    selected = hittedBuilding.GetComponent<ISelectable>();
                                }
                            }
                            else if (hittedResident) {
                                selected = hittedResident.GetComponent<ISelectable>();
                            }
                            else if (hittedBoat) {
                                selected = hittedBoat.GetComponent<ISelectable>();
                            }
                            else if (hittedLootContainer) {
                                List<ItemInstance> takedItems = hittedLootContainer.TakeItems();
                                CollectItems(takedItems);
                            }

                            if (selected != null) {
                                if (selected == selectedObject) {
                                    Deselect();
                                }
                                else {
                                    if (selectedObject != null)
                                        Deselect();
                                    Select(selected);
                                }
                            }
                            else {
                                Deselect();
                            }

                        }
                    }
                }
            }
        }

        isPrimaryInteractionPressed = false;
    }

    private void OnSecondaryTouchStarted(InputAction.CallbackContext context)
    {
        secondaryInteractionStartPosition = secondaryInteractionPositionIA.ReadValue<Vector2>();
        startInteractionsPitch = Vector2.Distance(primaryInteractionPosition, secondaryInteractionStartPosition);
        isSecondaryInteractionPressed = true;
    }

    private void OnSecondaryTouchEnded(InputAction.CallbackContext context)
    {
        secondaryInteractionPosition = Vector2.zero;
        secondaryInteractionDelta = Vector2.zero;
        isSecondaryInteractionPressed = false;
    }

    private void OnSecondaryInteractionPosition(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        secondaryInteractionPosition = value;

        interactionsPitch = Vector2.Distance(primaryInteractionPosition, secondaryInteractionPosition);
        touchPitchInput = interactionsPitch - startInteractionsPitch;
        touchPitchVelocity = touchPitchInput - touchPitchLastInput;
        touchPitchLastInput = touchPitchInput;
    }

    private void OnSecondaryInteractionDelta(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        secondaryInteractionDelta = value;
    }

    //private void OnTouchPresing()
    //{
    //    if (isFirstTouchPressed || isSecondTouchPressed)
    //    {
    //        if (isFirstTouchPressed)
    //        {
    //            firstTouchCurrentPosition = primaryInteractionPositionIA.ReadValue<Vector2>();
    //            firstTouchMoveInput = primaryInteractionDeltaIA.ReadValue<Vector2>();
    //        }
    //        else
    //        {
    //            firstTouchCurrentPosition = Vector3.zero;
    //            firstTouchMoveInput = Vector3.zero;
    //        }

    //        if (isSecondTouchPressed)
    //        {
    //            secondTouchCurrentPosition = secondaryInteractionPositionIA.ReadValue<Vector2>();
    //            secondTouchMoveInput = secondaryInteractionDeltaIA.ReadValue<Vector2>();
    //        }
    //        else
    //        {
    //            secondTouchCurrentPosition = Vector3.zero;
    //            secondTouchMoveInput = Vector3.zero;
    //        }

    //        dragMoveVelocity.x = firstTouchMoveInput.x + secondTouchMoveInput.x;
    //        dragMoveVelocity.y = firstTouchMoveInput.y + secondTouchMoveInput.y;

    //        cameraMoveVelocity.x = dragMoveVelocity.x * cameraMoveSensitivity.x;
    //        cameraMoveVelocity.y = dragMoveVelocity.y * cameraMoveSensitivity.y;
    //    }
    //    //else if (!isFirstTouchPressed && !isSecondTouchPressed)
    //    //{
    //    //    cameraMoveVelocity = Vector2.Lerp(cameraMoveVelocity, Vector2.zero, cameraStopMoveSpeed * Time.deltaTime);
    //    //}
    //}

    private void OnBuildingStartPlacing(ConstructionComponent construction)
    {
        Building building = construction.GetComponent<Building>();

        //if (isBuildingToPlaceSelected)
        //{
        //    StopPlacingBuilding(buildingToPlace.constructionComponent);
        //}

        //isBuildingToPlaceSelected = true;
        buildingToPlace = building;

        //cityManager.ShowBuildingPlacesByType(building);
        //uiManager.CloseBuildingManagementMenu();
        //uiManager.OnBuildingPlacingStarted();
    }

    private void PlaceConstruction(BuildingPlace buildingPlace)
    {
        GameManager.Instance.PlaceBuilding(buildingToPlace, buildingPlace, 0, true);
    }

    public void OnConstructionPlaced()
    {
        if (buildingToPlace)
        {
            //cityManager.HideBuildingPlacesByType(buildingToPlace.BuildingData.BuildingType);

            buildingToPlace = null;
        }
    }

    private void CollectItems(ItemInstance item)
    {
        int id = item.ItemData.ItemId;
        if (GameManager.Instance.items[id].Amount < GameManager.Instance.totalStorageCapacity[id].Amount) {
            GameManager.Instance.AddItem(item);
        }
    }

    private void CollectItems(List<ItemInstance> items)
    {
        GameManager.Instance.AddItems(items);
    }

    private void Select(ISelectable selectComponent)
    {
        selectedObject = selectComponent;
        selectedObject.Select();
        uiManager.OpenContextMenu(selectedObject);
    }

    public void Deselect()
    {
        if (selectedObject == null) return;

        selectedObject.Deselect();

        if (!uiManager.isBuildingResourcesMenuOpened) {
            selectedObject = null;

            uiManager.CloseContextMenu();
        }
        else {
            uiManager.CloseBuildingActionMenu();
        }
    }
}
