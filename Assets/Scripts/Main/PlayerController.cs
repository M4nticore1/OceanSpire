using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    // Main
    private GameManager gameManager = null;
    private CityManager cityManager = null;
    private UIManager uiManager = null;

    // Camera Movement
    [SerializeField] private Camera mainCamera = null;
    [SerializeField] private GameObject cameraHolder = null;

    private Vector3 сameraHolderStartPosition = Vector3.zero;
    private Quaternion сameraHolderStartRotation = new Quaternion(0f, 0f, 0f, 0);

    [HideInInspector] public Vector3 cameraVerticalPosition { get; private set; } = Vector3.zero;
    [HideInInspector] public Vector3 cameraHorizontalPosition { get; private set; } = Vector3.zero;

    //private Vector3 cameraHorizontalRotation = Vector3.zero;
    private Vector3 cameraHolderRotation = Vector3.zero;

    private Vector2 cameraMoveSensitivity = new Vector2(6.0f, 1.0f);
    private const float cameraStopMoveSpeed = 6.0f;

    [HideInInspector] public Vector2 dragMoveVelocity = Vector2.zero;
    private Vector2 cameraMoveVelocity = Vector2.zero;
    private const float cameraHeightBoundaryPadding = 10.0f;
    private const float cameraHeightReturnSpeed = 5.0f;

    private float cameraMoveMultiplier = 1.0f;
    [HideInInspector] public float cameraYawRotateAlpha = 0.0f;
    private float moveStateValue = 0;
    private int moveStateIndex = 0;

    // Camera Arm
    private float currentCameraArmLength = 0.0f;
    private const float minCameraArmLength = 12.0f;
    private const float maxCameraArmLength = 100.0f;

    private const float nearCameraArmBoundaryPadding = 5.0f;
    private const float farCameraArmBoundaryPadding = 20.0f;
    private const float cameraArmReturnSpeed = 5.0f;

    private float cameraArmMoveMultiplier = 1.0f;
    private float currentCameraDistance = 0.0f;

    private const int cameraMovingDistance = 20;
    private const int cameraDistanceToShowBuildingStats = 15;
    private const int cameraHeightOffsetToShowBuildingStats = 0;

    // Camera Shake
    private const float cameraShakeAmplitude = 1f;
    private const float cameraShakeSpeed = 0.5f;
    private Vector3 currentCameraShakeForce = Vector3.zero;
    private Vector3 currentCameraShakeRotation = Vector3.zero;

    // Input System
    private bool isFirstTouchPressed = false;
    private bool isSecondTouchPressed = false;

    private PlayerInput playerInput = null;
    [SerializeField] private InputActionAsset mainInputActionsAsset = null;
    private InputActionMap touchInputActionMap = null;

    private InputAction firstTouchPressAction = null;
    private InputAction firstTouchPositionAction = null;
    private InputAction firstTouchMoveAction = null;
    private InputAction secondTouchPressAction = null;
    private InputAction secondTouchPositionAction = null;
    private InputAction secondTouchMoveAction = null;

    private Vector2 firstTouchStartPosition = Vector2.zero;
    private Vector2 firstTouchCurrentPosition = Vector2.zero;
    private Vector2 firstTouchMoveInput = Vector2.zero;

    private Vector2 secondTouchStartPosition = Vector2.zero;
    private Vector2 secondTouchCurrentPosition = Vector2.zero;
    private Vector2 secondTouchMoveInput = Vector2.zero;

    private float currentTouchPitchDistance = 0.0f;
    private float startTouchPitchDistance = 0.0f;
    private float touchPitchInput = 0.0f;
    private float touchPitchLastInput = 0.0f;
    private float touchPitchVelocity = 0.0f;
    private const float touchPitchSensitivity = 0.1f;
    private const float pitchStopSpeed = 25.0f;

    // Building
    private bool isBuildingToPlaceSelected = false;
    [HideInInspector] public Building buildingToPlace = null;

    private SelectComponent selectedComponent = null;
    //private bool isSelectedBuilding = false;

    // Raycast
    private GraphicRaycaster graphicRaycaster = null;
    private EventSystem eventSystem = null;
    [SerializeField] private LayerMask clickableLayers;

    private float lastSaveDataTime = 0.0f;

    public bool isInitialized { get; private set; } = false;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        cityManager = FindAnyObjectByType<CityManager>();
        uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager) graphicRaycaster = uiManager.gameObject.GetComponent<GraphicRaycaster>();
        else Debug.LogError("uiManager is NULL");
        SetInputSystem();
    }

    private void Start()
    {
        Load(GameManager.saveData);
    }

    public void Load(SaveData saveData)
    {
        LoadLocalization();

        сameraHolderStartPosition = cameraHolder.transform.position;
        сameraHolderStartRotation = cameraHolder.transform.rotation;
        cameraHolderRotation = сameraHolderStartRotation.eulerAngles;

        currentCameraArmLength = -mainCamera.transform.localPosition.z;

        if (SaveSystem.saveData == null) {
            cameraYawRotateAlpha = 0.5f; }
        else {
            cameraYawRotateAlpha = saveData.cameraYawRotation; }

        cameraYawRotateAlpha = 0.52f;

        moveStateValue = 1f / CityManager.roomsCountPerFloor;

        SaveData();

        isInitialized = true;
    }

    private void LoadLocalization()
    {
        LocalizationSystem.LoadLocalizations();
        LocalizationSystem.SetLocalization(Settings.currentLanguageKey);
    }

    private void Update()
    {
        Tick();
    }

    public void Tick()
    {
        if (isInitialized)
        {
            OnTouchPresing();
            MoveCamera();
            SetCameraArmLength();
            CameraShake();

            mainCamera.transform.localRotation = Quaternion.Euler(currentCameraShakeRotation);

            int placeIndex;
            if (moveStateIndex <= 5)
                placeIndex = (5 - moveStateIndex) % 6;
            else
                placeIndex = 13 - moveStateIndex;

            Building buildingToShowStats = cityManager.GetBuildingByIndex(CityManager.GetFloorIndexByHeight(cameraHolder.transform.position.y + cameraHeightOffsetToShowBuildingStats), placeIndex);

            if (currentCameraDistance <= cameraDistanceToShowBuildingStats)
            {
                if (buildingToShowStats)
                    uiManager.OpenBuildingStatsPanel(buildingToShowStats);
                else
                    uiManager.CloseBuildingStatsPanel();
            }
            else
                uiManager.CloseBuildingStatsPanel();

            if (Time.time >= lastSaveDataTime + GameManager.autoSaveFrequency)
            {
                SaveData();
                lastSaveDataTime = Time.time;
            }
        }
    }

    private void OnEnable()
    {
        touchInputActionMap.Enable();

        firstTouchPressAction.performed += OnFirstTouchStarted;
        firstTouchPressAction.canceled += OnFirstTouchEnded;

        secondTouchPressAction.performed += OnSecondTouchStarted;
        secondTouchPressAction.canceled += OnSecondTouchEnded;

        BuildingWidget.OnBuildStartPlacing += OnBuildingStartPlacing;
        UIManager.OnBuildStopPlacing += StopPlacingBuilding;
    }

    private void OnDisable()
    {
        touchInputActionMap.Disable();

        firstTouchPressAction.performed -= OnFirstTouchStarted;
        firstTouchPressAction.canceled -= OnFirstTouchEnded;

        secondTouchPressAction.performed -= OnSecondTouchStarted;
        secondTouchPressAction.canceled -= OnSecondTouchEnded;

        BuildingWidget.OnBuildStartPlacing -= OnBuildingStartPlacing;
        UIManager.OnBuildStopPlacing -= StopPlacingBuilding;
    }

    private void SetInputSystem()
    {
        if (mainInputActionsAsset != null)
        {
            touchInputActionMap = mainInputActionsAsset.FindActionMap("Gameplay");

            if (touchInputActionMap != null)
            {
                firstTouchPressAction = touchInputActionMap.FindAction("FirstTouchPress");
                firstTouchPositionAction = touchInputActionMap.FindAction("FirstTouchPosition");
                firstTouchMoveAction = touchInputActionMap.FindAction("FirstTouchMove");

                secondTouchPressAction = touchInputActionMap.FindAction("SecondTouchPress");
                secondTouchPositionAction = touchInputActionMap.FindAction("SecondTouchPosition");
                secondTouchMoveAction = touchInputActionMap.FindAction("SecondTouchMove");

                //mouseScrollAction = touchInputActionMap.FindAction("MouseScroll");
            }
            else
                Debug.Log("void PlayerController : SetInputSystem() touchInputActionMap is NULL");
        }
        else
            Debug.Log("void PlayerController : SetInputSystem() inputActions is NULL");
    }

    // Camera
    private void MoveCamera()
    {
        if (cameraHolder && cityManager)
        {
            if (cameraHolder.transform.position.y > cityManager.cityHeight)
                cameraMoveMultiplier = math.pow(((cityManager.cityHeight - cameraHolder.transform.position.y) + cameraHeightBoundaryPadding) / cameraHeightBoundaryPadding, 2.0f);
            else if (cameraHolder.transform.position.y < 0.0f)
                cameraMoveMultiplier = ((0 - math.abs(cameraHolder.transform.position.y)) + cameraHeightBoundaryPadding) / cameraHeightBoundaryPadding;

            Vector3 currentCameraMoveVelocity = new Vector3(0, cameraMoveVelocity.y, 0) * Time.deltaTime * cameraMoveMultiplier;
            cameraVerticalPosition -= currentCameraMoveVelocity;

            // Camera horizontal move
            float shiftedAlpha = cameraYawRotateAlpha - (1f - (moveStateValue / 2f));
            shiftedAlpha = Mathf.Repeat(shiftedAlpha, 1f);
            moveStateIndex = (int)(shiftedAlpha / moveStateValue);

            int positionHalfLenght = cameraMovingDistance / 2;

            float position = shiftedAlpha % moveStateValue * CityManager.roomsCountPerFloor * cameraMovingDistance - positionHalfLenght;
            float cameraSensitivityMultiplier = moveStateIndex % 2 == 0 ? 0.5f : 1f;

            if (moveStateIndex == 0)
                cameraHorizontalPosition = new Vector3(-position, 0, -positionHalfLenght);
            else if (moveStateIndex == 1)
                cameraHorizontalPosition = new Vector3(-positionHalfLenght, 0, -positionHalfLenght);
            else if (moveStateIndex == 2)
                cameraHorizontalPosition = new Vector3(-positionHalfLenght, 0, position);
            else if (moveStateIndex == 3)
                cameraHorizontalPosition = new Vector3(-positionHalfLenght, 0, positionHalfLenght);
            else if (moveStateIndex == 4)
                cameraHorizontalPosition = new Vector3(position, 0, positionHalfLenght);
            else if (moveStateIndex == 5)
                cameraHorizontalPosition = new Vector3(positionHalfLenght, 0, positionHalfLenght);
            else if (moveStateIndex == 6)
                cameraHorizontalPosition = new Vector3(positionHalfLenght, 0, -position);
            else if (moveStateIndex == 7)
                cameraHorizontalPosition = new Vector3(positionHalfLenght, 0, -positionHalfLenght);

            cameraYawRotateAlpha += cameraMoveVelocity.x * cameraSensitivityMultiplier / 360f * Time.deltaTime;
            cameraYawRotateAlpha = Mathf.Repeat(cameraYawRotateAlpha, 1f);

            cameraHolderRotation.y = cameraYawRotateAlpha * 360f;

            if (!isFirstTouchPressed)
            {
                Vector3 clampPosition = Vector3.zero;

                if (cameraVerticalPosition.y > cityManager.cityHeight)
                {
                    clampPosition.y = cityManager.cityHeight;
                    cameraVerticalPosition = math.lerp(cameraVerticalPosition, clampPosition, cameraHeightReturnSpeed * Time.deltaTime);
                }
                else if (cameraVerticalPosition.y < 0.0f)
                {
                    cameraVerticalPosition = math.lerp(cameraVerticalPosition, clampPosition, cameraHeightReturnSpeed * Time.deltaTime);
                    clampPosition.y = 0;
                }
            }

            cameraHolder.transform.rotation = Quaternion.Euler(cameraHolderRotation);
            cameraHolder.transform.position = cameraHorizontalPosition + cameraVerticalPosition;
        }
        else
        {
            if (!cameraHolder)
                Debug.LogError("cameraHolder is NULL");
            if (!cityManager)
                Debug.LogError("cityManager is NULL");
        }
    }

    private void SetCameraArmLength()
    {
        if (isFirstTouchPressed && isSecondTouchPressed)
        {
            currentTouchPitchDistance = Vector2.Distance(firstTouchCurrentPosition, secondTouchCurrentPosition);
            touchPitchInput = currentTouchPitchDistance - startTouchPitchDistance;

            touchPitchVelocity = touchPitchInput - touchPitchLastInput;
            touchPitchLastInput = touchPitchInput;

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

            currentCameraArmLength -= touchPitchVelocity * touchPitchSensitivity;

            if (currentCameraArmLength > maxCameraArmLength)
                currentCameraArmLength = math.lerp(currentCameraArmLength, maxCameraArmLength, cameraArmReturnSpeed * Time.deltaTime);
            else if (currentCameraArmLength < minCameraArmLength)
                currentCameraArmLength = math.lerp(currentCameraArmLength, minCameraArmLength, cameraArmReturnSpeed * Time.deltaTime);

            touchPitchLastInput = 0;
        }

        mainCamera.transform.localPosition = new Vector3(mainCamera.transform.localPosition.x, mainCamera.transform.localPosition.y, -currentCameraArmLength);
        currentCameraDistance = Vector3.Distance(cameraHolder.transform.position, mainCamera.transform.position);
    }

    private void CameraShake()
    {
        currentCameraShakeForce = new Vector3(math.sin(Time.time * cameraShakeSpeed) * cameraShakeAmplitude, math.cos(Time.time * cameraShakeSpeed / 2) * cameraShakeAmplitude, 0);

        currentCameraShakeRotation = math.lerp(currentCameraShakeRotation, currentCameraShakeForce, cameraShakeSpeed * Time.deltaTime);
    }

    private void OnFirstTouchStarted(InputAction.CallbackContext context)
    {
        if (!isFirstTouchPressed)
        {
            firstTouchStartPosition = Touchscreen.current.touches[0].position.ReadValue();

            startTouchPitchDistance = Vector2.Distance(firstTouchStartPosition, secondTouchCurrentPosition);

            isFirstTouchPressed = true;
        }
    }

    private void OnFirstTouchEnded(InputAction.CallbackContext context)
    {
        if (isFirstTouchPressed)
        {
            if (!isSecondTouchPressed)
            {
                if (firstTouchStartPosition == firstTouchCurrentPosition)
                {
                    PointerEventData pointerEventData = new PointerEventData(eventSystem);
                    pointerEventData.position = firstTouchCurrentPosition;
                    List<RaycastResult> results = new List<RaycastResult>();
                    graphicRaycaster.Raycast(pointerEventData, results);

                    if (results.Count == 0)
                    {
                        Ray ray = mainCamera.ScreenPointToRay(firstTouchCurrentPosition);

                        RaycastHit hit;

                        if (Physics.Raycast(ray, out hit, 500))
                        {
                            if (isBuildingToPlaceSelected && buildingToPlace)
                            {
                                BuildingPlace hittedBuildingPlace = hit.collider.gameObject.GetComponent<BuildingPlace>();

                                if (hittedBuildingPlace && !hittedBuildingPlace.placedBuilding)
                                {
                                    PlaceBuilding(hittedBuildingPlace);
                                }
                            }
                            else
                            {
                                LootContainer hittedLootContainer = hit.collider.gameObject.GetComponent<LootContainer>();
                                Resident hittedResident = hit.collider.gameObject.GetComponent<Resident>();
                                Building hittedBuilding = null;

                                if (hit.collider && hit.collider.gameObject.transform.parent && hit.collider.gameObject.transform.parent.GetComponent<Building>())
                                    hittedBuilding = hit.collider.gameObject.transform.parent.GetComponent<Building>();

                                if (hittedBuilding)
                                {
                                    ProductionBuildingComponent hittedProductionBuilding = hittedBuilding.GetComponent<ProductionBuildingComponent>();

                                    if (hittedProductionBuilding && hittedProductionBuilding.isReadyToCollect)
                                    {
                                        CollectItems(hittedProductionBuilding.TakeProducedItem());
                                        DeselectComponent();
                                    }
                                    else
                                    {
                                        if (selectedComponent != hittedBuilding)
                                            SelectComponent(hittedBuilding.selectComponent);
                                        else
                                            DeselectComponent();
                                    }
                                }
                                else if (hittedResident)
                                {
                                    SelectComponent(hittedResident.selectComponent);
                                }
                                else if (hittedLootContainer)
                                {
                                    List<ItemInstance> takedItems = hittedLootContainer.TakeItems();
                                    CollectItems(takedItems);

                                    DeselectComponent();
                                }
                                else
                                {
                                    DeselectComponent();
                                }
                            }
                        }
                    }
                }
            }

            firstTouchCurrentPosition = Vector2.zero;
            firstTouchMoveInput = Vector2.zero;

            isFirstTouchPressed = false;
        }
    }

    private void OnSecondTouchStarted(InputAction.CallbackContext context)
    {
        if (!isSecondTouchPressed)
        {
            secondTouchStartPosition = Touchscreen.current.touches[1].position.ReadValue();
            firstTouchCurrentPosition = firstTouchPositionAction.ReadValue<Vector2>();

            startTouchPitchDistance = Vector2.Distance(firstTouchCurrentPosition, secondTouchStartPosition);

            isSecondTouchPressed = true;
        }
    }

    private void OnSecondTouchEnded(InputAction.CallbackContext context)
    {
        if (isSecondTouchPressed)
        {
            secondTouchCurrentPosition = Vector2.zero;
            secondTouchMoveInput = Vector2.zero;

            isSecondTouchPressed = false;
        }
    }
    
    private void OnTouchPresing()
    {
        if (isFirstTouchPressed || isSecondTouchPressed)
        {
            if (isFirstTouchPressed)
            {
                firstTouchCurrentPosition = firstTouchPositionAction.ReadValue<Vector2>();
                firstTouchMoveInput = firstTouchMoveAction.ReadValue<Vector2>();
            }
            else
            {
                firstTouchCurrentPosition = Vector3.zero;
                firstTouchMoveInput = Vector3.zero;
            }

            if (isSecondTouchPressed)
            {
                secondTouchCurrentPosition = secondTouchPositionAction.ReadValue<Vector2>();
                secondTouchMoveInput = secondTouchMoveAction.ReadValue<Vector2>();
            }
            else
            {
                secondTouchCurrentPosition = Vector3.zero;
                secondTouchMoveInput = Vector3.zero;
            }

            dragMoveVelocity.x = firstTouchMoveInput.x + secondTouchMoveInput.x;
            dragMoveVelocity.y = firstTouchMoveInput.y + secondTouchMoveInput.y;

            cameraMoveVelocity.x = dragMoveVelocity.x * cameraMoveSensitivity.x;
            cameraMoveVelocity.y = dragMoveVelocity.y * cameraMoveSensitivity.y;
        }
        else if (!isFirstTouchPressed && !isSecondTouchPressed)
        {
            cameraMoveVelocity = Vector2.Lerp(cameraMoveVelocity, Vector2.zero, cameraStopMoveSpeed * Time.deltaTime);
        }
    }

    private void OnBuildingStartPlacing(Building building)
    {
        //Building building = newConstructionToPlace.GetComponent<Building>();

        //if (isBuildingToPlaceSelected)
        //{
        //    StopPlacingBuilding(buildingToPlace.constructionComponent);
        //}

        //isBuildingToPlaceSelected = true;
        //buildingToPlace = building;

        //cityManager.ShowBuildingPlacesByType(building);
        //uiManager.CloseBuildingManagementMenu();
        //uiManager.OnBuildingPlacingStarted();
    }

    private void PlaceBuilding(BuildingPlace buildingPlace)
    {
        cityManager.PlaceBuilding(buildingToPlace, buildingPlace, 0, true);
    }

    public void StopPlacingBuilding()
    {
        if (buildingToPlace)
        {
            //cityManager.HideBuildingPlacesByType(buildingToPlace.BuildingData.BuildingType);

            isBuildingToPlaceSelected = false;
            buildingToPlace = null;
        }
    }

    private void CollectItems(ItemInstance item)
    {
        int id = item.ItemData.ItemId;
        if (cityManager.items[id].Amount < cityManager.totalStorageCapacity[id].Amount)
        {
            cityManager.AddItem(item);
        }
    }

    private void CollectItems(List<ItemInstance> items)
    {
        cityManager.AddItems(items);
    }

    private void SelectComponent(SelectComponent selectComponent)
    {
        DeselectComponent();

        selectedComponent = selectComponent;
        selectedComponent.Select();

        Building building = selectComponent.GetComponent<Building>();
        Entity entity = selectComponent.GetComponent<Entity>();
        Boat boat = selectComponent.GetComponent<Boat>();
        if (building)
        {
            if (building.constructionComponent.isRuined)
            {
                uiManager.OpenRepairBuildingMenu(building);
            }
            else
            {
                uiManager.OpenBuildingManagementMenu(building);
            }
        }
        else if (entity)
        {

        }
        else if (boat)
        {

        }
    }

    public void DeselectComponent()
    {
        if (selectedComponent)
        {
            selectedComponent.Deselect();

            if (!uiManager.isBuildingResourcesMenuOpened)
            {
                selectedComponent = null;

                uiManager.CloseBuildingManagementMenu();
            }
            else
            {
                uiManager.CloseBuildingActionMenu();
            }
        }
    }

    private void SaveData()
    {
       SaveSystem.SaveData(this, cityManager);
    }
}
