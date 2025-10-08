using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // Main
    private GameManager gameManager = null;
    private CityManager cityManager = null;
    private UIManager UIManager = null;

    // Camera Movement
    [SerializeField] private Camera mainCamera = null;
    [SerializeField] private GameObject cameraHolder = null;

    private Vector3 ñameraHolderStartPosition = Vector3.zero;
    private Quaternion ñameraHolderStartRotation = new Quaternion(0f, 0f, 0f, 0);

    [HideInInspector] public Vector3 cameraVerticalPosition = Vector3.zero;
    [HideInInspector] public Vector3 cameraHorizontalPosition = Vector3.zero;

    private Vector3 cameraHorizontalRotation = Vector3.zero;
    private Vector3 cameraVerticalRotation = Vector3.zero;

    private Vector2 CameraMoveSensitivity = new Vector2(6.0f, 1.0f);
    private const float cameraStopMoveSpeed = 9.0f;

    private Vector2 cameraMoveVelocity = Vector2.zero;
    private const float cameraHeightBoundaryPadding = 10.0f;
    private const float cameraHeightReturnSpeed = 5.0f;

    private float cameraMoveMultiplier = 1.0f;
    [HideInInspector] public float cameraYawRotateAlpha = 0.0f;
    float moveStateValue = 0;

    private const int cameraMovingDistance = 8;

    // Camera Arm
    private float startCameraArmLength = 0.0f;
    private float currentCameraArmLength = 0.0f;
    private const float minCameraArmLength = 20.0f;
    private const float maxCameraArmLength = 100.0f;

    private const float nearCameraArmBoundaryPadding = 5.0f;
    private const float farCameraArmBoundaryPadding = 20.0f;
    private const float cameraArmReturnSpeed = 5.0f;

    private float cameraArmMoveMultiplier = 1.0f;

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
    private float touchPitchSensitivity = 0.1f;
    private float pitchStopSpeed = 25.0f;

    // Building
    private bool isBuildingToPlaceSelected = false;
    [HideInInspector] public Building buildingToPlace = null;

    private Building selectedBuilding = null;
    private bool isSelectedBuilding = false;

    // Raycast
    private GraphicRaycaster graphicRaycaster = null;
    private EventSystem eventSystem = null;
    [SerializeField] private LayerMask clickableLayers;

    private float lastSaveDataTime = 0.0f;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        gameManager = FindAnyObjectByType<GameManager>();
        cityManager = FindAnyObjectByType<CityManager>();
        UIManager = FindAnyObjectByType<UIManager>();
        graphicRaycaster = UIManager.gameObject.GetComponent<GraphicRaycaster>();

        UIManager.InitializeUIManager();

        SetInputSystem();
    }

    private void Start()
    {
        LoadData();

        ñameraHolderStartPosition = cameraHolder.transform.position;
        ñameraHolderStartRotation = cameraHolder.transform.rotation;
        cameraVerticalRotation = new Vector3(cameraHolder.transform.rotation.eulerAngles.x, 0f, 0f);

        currentCameraArmLength = -mainCamera.transform.localPosition.z;
        startCameraArmLength = -currentCameraArmLength;

        if (!gameManager.hasSavedData)
            cameraYawRotateAlpha = 0.5f;

        moveStateValue = 1f / CityManager.roomsCountPerFloor;
    }

    private void Update()
    {
        OnTouchPresing();
        MoveCamera();
        SetCameraArmLength();

        if (Time.time >= lastSaveDataTime + GameManager.autoSaveFrequency)
        {
            SaveData();
            lastSaveDataTime = Time.time;
        }
    }

    private void OnEnable()
    {
        firstTouchPressAction.performed += OnFirstTouchStarted;
        firstTouchPressAction.canceled += OnFirstTouchEnded;
        firstTouchPressAction.Enable();

        secondTouchPressAction.performed += OnSecondTouchStarted;
        secondTouchPressAction.canceled += OnSecondTouchEnded;
        secondTouchPressAction.Enable();

        firstTouchMoveAction?.Enable();
        firstTouchPositionAction?.Enable();

        secondTouchMoveAction?.Enable();
        secondTouchPositionAction?.Enable();

        CityManager.OnStorageCapacityUpdated += UpdateUIStorageItems;
        Building.OnBuildingStartConstructing += StopPlacingBuilding;
    }

    private void OnDisable()
    {
        if (firstTouchPressAction != null)
        {
            firstTouchPressAction.performed -= OnFirstTouchStarted;
            firstTouchPressAction.canceled -= OnFirstTouchEnded;
            firstTouchPressAction.Disable();
        }
        else
            Debug.Log("void PlayerController : OnEnable() touchPressAction is NULL");

        firstTouchMoveAction?.Disable();
        firstTouchPositionAction?.Disable();

        secondTouchMoveAction?.Disable();
        secondTouchPositionAction?.Disable();

        CityManager.OnStorageCapacityUpdated -= UpdateUIStorageItems;
    }

    private void SetInputSystem()
    {
        playerInput = GetComponent<PlayerInput>();

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
            }
            else
                Debug.Log("void PlayerController : SetInputSystem() touchInputActionMap is NULL");
        }
        else
            Debug.Log("void PlayerController : SetInputSystem() inputActions is NULL");
    }

    private void MoveCamera()
    {
        if (cameraHolder.transform.position.y > cityManager.cityHeight)
            cameraMoveMultiplier = math.pow(((cityManager.cityHeight - cameraHolder.transform.position.y) + cameraHeightBoundaryPadding) / cameraHeightBoundaryPadding, 2.0f);
        else if (cameraHolder.transform.position.y < 0.0f)
            cameraMoveMultiplier = ((0 - math.abs(cameraHolder.transform.position.y)) + cameraHeightBoundaryPadding) / cameraHeightBoundaryPadding;

        Vector3 currentCameraMoveVelocity = new Vector3(0, cameraMoveVelocity.y, 0) * Time.deltaTime * cameraMoveMultiplier;
        cameraVerticalPosition -= currentCameraMoveVelocity;

        // Camera horizontal move
        float shiftedAlpha = cameraYawRotateAlpha - (1 - (moveStateValue / 2));
        shiftedAlpha = Mathf.Repeat(shiftedAlpha, 1f);
        int moveStateIndex = (int)(shiftedAlpha / moveStateValue);

        cameraHorizontalRotation.y = cameraYawRotateAlpha * 360f;

        int positionHalfLenght = cameraMovingDistance / 2;

        float position = shiftedAlpha % moveStateValue * CityManager.roomsCountPerFloor * cameraMovingDistance - positionHalfLenght;
        float cameraSensitivityMultiplier = moveStateIndex % 2 == 0 ? 0.5f : 1f;

        if (moveStateIndex == 0)
            cameraHorizontalPosition = new Vector3(-position, 0, -positionHalfLenght);
        if (moveStateIndex == 1)
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

        int sign = cameraMoveVelocity.x > 0 ? 1 : cameraMoveVelocity.x < 0 ? -1 : 0;
        cameraYawRotateAlpha += cameraMoveVelocity.x * cameraSensitivityMultiplier / 360f * Time.deltaTime;
        cameraYawRotateAlpha = Mathf.Repeat(cameraYawRotateAlpha, 1f);

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

        cameraHolder.transform.rotation = Quaternion.Euler(cameraHorizontalRotation + cameraVerticalRotation);
        cameraHolder.transform.position = cameraHorizontalPosition + cameraVerticalPosition;
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
                        int clickableMask = LayerMask.GetMask("Clickable");
                        int UIMask = LayerMask.GetMask("UI");
                        Ray ray = mainCamera.ScreenPointToRay(firstTouchCurrentPosition);

                        RaycastHit hit;

                        if (Physics.Raycast(ray, out hit, 500, clickableLayers))
                        {
                            if (isBuildingToPlaceSelected && buildingToPlace)
                            {
                                BuildingPlace hittedBuildingPlace = hit.collider.gameObject.GetComponent<BuildingPlace>();

                                if (hittedBuildingPlace && !hittedBuildingPlace.isBuildingPlaced)
                                {
                                    PlaceBuilding(hittedBuildingPlace);
                                }
                            }
                            else
                            {
                                LootContainer hittedLootContainer = hit.collider.gameObject.GetComponent<LootContainer>();
                                Building hittedBuilding = null;

                                if (hit.collider && hit.collider.gameObject.transform.parent && hit.collider.gameObject.transform.parent.GetComponent<Building>())
                                    hittedBuilding = hit.collider.gameObject.transform.parent.GetComponent<Building>();

                                if (hittedBuilding)
                                {
                                    ProductionBuildingComponent hittedProductionBuilding = hittedBuilding.GetComponent<ProductionBuildingComponent>();

                                    if (hittedProductionBuilding && hittedProductionBuilding.isReadyToCollect)
                                    {
                                        TakeItem(hittedProductionBuilding.TakeProducedItem());
                                    }
                                    else
                                    {
                                        if (!isSelectedBuilding || selectedBuilding != hittedBuilding)
                                            SelectBuilding(hittedBuilding);
                                        else
                                            UnselectBuilding();
                                    }
                                }
                                else if (hittedLootContainer)
                                {
                                    TakeItems(hittedLootContainer.GetContainedLoot());
                                    hittedLootContainer.TakeItems();

                                    UnselectBuilding();
                                }
                                else
                                {
                                    UnselectBuilding();
                                }
                            }
                        }
                        else
                        {
                            //UnselectBuilding();
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
            startCameraArmLength = mainCamera.transform.localPosition.z;

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

            cameraMoveVelocity.x = (firstTouchMoveInput.x + secondTouchMoveInput.x) * CameraMoveSensitivity.x;
            cameraMoveVelocity.y = (firstTouchMoveInput.y + secondTouchMoveInput.y) * CameraMoveSensitivity.y;
        }
        else if (!isFirstTouchPressed && !isSecondTouchPressed)
        {
            cameraMoveVelocity = Vector2.Lerp(cameraMoveVelocity, Vector2.zero, cameraStopMoveSpeed * Time.deltaTime);
        }
    }

    public void StartPlacingBuilding(Building newBuildingToPlace)
    {
        if (isBuildingToPlaceSelected)
        {
            StopPlacingBuilding(buildingToPlace);
        }

        isBuildingToPlaceSelected = true;
        buildingToPlace = newBuildingToPlace;

        cityManager.ShowBuildingPlacesByType(newBuildingToPlace);
        UIManager.CloseBuildingManagementMenu();
        UIManager.OnBuildingPlacingStarted();
    }

    private void PlaceBuilding(BuildingPlace buildingPlace)
    {
        cityManager.PlaceBuilding(buildingToPlace, buildingPlace, 0, true);
    }

    public void StopPlacingBuilding(Building building)
    {
        if (buildingToPlace && building && buildingToPlace.buildingData.buildingIdName == building.buildingData.buildingIdName)
        {
            cityManager.HideBuildingPlacesByType(buildingToPlace.buildingData.buildingType);

            isBuildingToPlaceSelected = false;
            buildingToPlace = null;

            UIManager.OnBuildingPlacingStopped();

            SaveData();
        }
    }

    private void TakeItem(ItemInstance item)
    {
        int index = gameManager.GetItemIndexByIdName(item.itemData.itemIdName);

        if (cityManager.items[index].amount < cityManager.items[index].maxAmount)
        {
            cityManager.AddItemByIndex(index, item.amount);
            UpdateUIStorageItemByIndex(index);
        }
    }

    private void TakeItems(List<ItemInstance> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            int index = gameManager.GetItemIndexByIdName(items[i].itemData.itemIdName);
			if (cityManager.items[index].amount < cityManager.items[index].maxAmount)
            {
                cityManager.AddItemByIndex(index, items[i].amount);

				UpdateUIStorageItemByIndex(index);
            }
        }
    }

    private void SpendItems(List<ResourceToBuild> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            int index = gameManager.GetItemIndexByIdName(items[i].resourceData.itemIdName);

            cityManager.SpendItemById(index, items[i].amount);
            UpdateUIStorageItemByIndex(index);
        }
    }

    private void UpdateUIStorageItems()
    {
        for (int i = 0; i < cityManager.items.Count; i++)
        {
            int index = gameManager.GetItemIndexByIdName(cityManager.items[i].itemData.itemIdName);
            int amount = cityManager.items[index].amount;
            int maxAmount = cityManager.items[index].maxAmount;

            UIManager.UpdateStorageItemByIndex(index, amount, maxAmount);
        }
    }

    private void UpdateUIStorageItemByIndex(int index)
    {
        int amount = cityManager.items[index].amount;
        int maxAmount = cityManager.items[index].maxAmount;

        UIManager.UpdateStorageItemByIndex(index, amount, maxAmount);
    }

    private void SelectBuilding(Building building)
    {
        //Debug.Log("select " + building);

        isSelectedBuilding = true;
        selectedBuilding = building;

        if (building.isRuined)
        {
            UIManager.OpenRepairBuildingMenu(building);
        }
        else
        {
            UIManager.OpenBuildingManagementMenu(building);
        }
    }

    public void UnselectBuilding()
    {
        if (!UIManager.isBuildingResourcesMenuOpened)
        {
            isSelectedBuilding = false;

            UIManager.CloseBuildingManagementMenu();
        }
        else
        {
            UIManager.CloseBuildingActionMenu();
        }
    }

    private void SaveData()
    {
       SaveSystem.SaveData(this, cityManager);
    }

    private void LoadData()
    {
        SaveData data = SaveSystem.LoadData();

        if (data != null)
        {
            gameManager.hasSavedData = true;
            cameraYawRotateAlpha = data.cameraYawRotation;
        }

        cityManager.LoadCity(data);
    }
}
