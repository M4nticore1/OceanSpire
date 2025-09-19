using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;

public class PlayerController : MonoBehaviour
{
    // Main
    private GameManager gameManager = null;
    private CityManager cityManager = null;
    private UIManager UIManager = null;

    // Camera Movement
    [SerializeField] private Camera mainCamera = null;
    [SerializeField] private GameObject ñameraHolder = null;

    private Vector3 ñameraHolderStartPosition = Vector3.zero;
    private Quaternion ñameraHolderStartRotation = new Quaternion(0f, 0f, 0f, 0);

    private Vector3 cameraPosition = Vector3.zero;
    private Vector3 cameraRotation = Vector3.zero;

    private Vector2 CameraMoveSensitivity = new Vector2(5.0f, 1.0f);
    private const float cameraStopMoveSpeed = 10.0f;

    private Vector2 cameraMoveVelocity = Vector2.zero;
    private const float cameraHeightBoundaryPadding = 10.0f;
    private const float cameraHeightReturnSpeed = 5.0f;

    private float cameraMoveMultiplier = 1.0f;

    // Camera Arm
    private float startCameraArmLength = 0.0f;
    private float currentCameraArmLength = 0.0f;
    private const float minCameraArmLength = 30.0f;
    private const float maxCameraArmLength = 100.0f;

    private const float nearCameraArmBoundaryPadding = 5.0f;
    private const float farCameraArmBoundaryPadding = 20.0f;
    private const float cameraArmReturnSpeed = 5.0f;

    private float cameraArmMoveMultiplier = 1.0f;

    // Input System
    private bool isFirstTouchPressed = false;
    private bool isSecondTouchPressed = false;

    private PlayerInput playerInput;
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
    [SerializeField] private LayerMask clickableLayers;

    private bool isPressedOnGUI = false;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        gameManager = FindAnyObjectByType<GameManager>();
        cityManager = FindAnyObjectByType<CityManager>();
        UIManager = FindAnyObjectByType<UIManager>();
        //UIManager.InitializeUIManager();

        SetInputSystem();

        ñameraHolderStartPosition = ñameraHolder.transform.position;
        ñameraHolderStartRotation = ñameraHolder.transform.rotation;

        cameraPosition = ñameraHolderStartPosition;
        cameraRotation = ñameraHolderStartRotation.eulerAngles;
    }

    private void Start()
    {
        currentCameraArmLength = -mainCamera.transform.localPosition.z;
        startCameraArmLength = -currentCameraArmLength;
    }

    private void Update()
    {
        OnTouchPresing();
        MoveCamera();
        SetCameraArmLength();
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
        Building.OnBuildingPlaced += StopPlacingBuilding;
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
        if (cameraPosition.y > cityManager.cityHeight)
            cameraMoveMultiplier = math.pow(((cityManager.cityHeight - cameraPosition.y) + cameraHeightBoundaryPadding) / cameraHeightBoundaryPadding, 2.0f);
        else if (cameraPosition.y < 0.0f)
            cameraMoveMultiplier = ((0 - math.abs(cameraPosition.y)) + cameraHeightBoundaryPadding) / cameraHeightBoundaryPadding;

        Vector3 currentCameraMoveVelocity = new Vector3(0, cameraMoveVelocity.y, 0) * Time.deltaTime * cameraMoveMultiplier;
        cameraPosition -= currentCameraMoveVelocity;
        ñameraHolder.transform.position = cameraPosition;

        Vector3 cameraRotationVelocity = new Vector3(0, cameraMoveVelocity.x, 0) * Time.deltaTime;
        cameraRotation += cameraRotationVelocity;
        Quaternion newCameraQuaternion = Quaternion.Euler(cameraRotation);
        ñameraHolder.transform.rotation = newCameraQuaternion;

        if (!isFirstTouchPressed)
        {
            Vector3 clampPosition = Vector3.zero;

            if (cameraPosition.y > cityManager.cityHeight)
            {
                clampPosition.y = cityManager.cityHeight;
                cameraPosition = math.lerp(cameraPosition, clampPosition, cameraHeightReturnSpeed * Time.deltaTime);
            }
            else if (cameraPosition.y < 0.0f)
            {
                cameraPosition = math.lerp(cameraPosition, clampPosition, cameraHeightReturnSpeed * Time.deltaTime);
                clampPosition.y = 0;
            }

            ñameraHolder.transform.position = cameraPosition;
        }

        //ñameraHolder.transform.SetPositionAndRotation(newCameraPosition, newCameraQuaternion);
    }

    private void SetCameraArmLength()
    {
        if (isFirstTouchPressed && isSecondTouchPressed)
        {
            currentTouchPitchDistance = Vector2.Distance(firstTouchCurrentPosition, secondTouchCurrentPosition);
            touchPitchInput = currentTouchPitchDistance - startTouchPitchDistance;

            touchPitchVelocity = touchPitchInput - touchPitchLastInput;
            touchPitchLastInput = touchPitchInput;

            if (currentCameraArmLength > maxCameraArmLength && touchPitchVelocity > 0)
                cameraArmMoveMultiplier = 1.0f - ((currentCameraArmLength - maxCameraArmLength) / farCameraArmBoundaryPadding);
            else if (currentCameraArmLength < minCameraArmLength && touchPitchVelocity < 0)
                cameraArmMoveMultiplier = 1.0f - (math.abs(minCameraArmLength - currentCameraArmLength) / nearCameraArmBoundaryPadding);
            else
                cameraArmMoveMultiplier = 1;

            cameraArmMoveMultiplier = math.clamp(cameraArmMoveMultiplier, 0, 1);

            currentCameraArmLength -= touchPitchVelocity * touchPitchSensitivity * cameraArmMoveMultiplier;

            Debug.Log(touchPitchVelocity);

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
                    int clickableMask = LayerMask.GetMask("Clickable");
                    int UIMask = LayerMask.GetMask("UI");
                    Ray ray = mainCamera.ScreenPointToRay(firstTouchCurrentPosition);

                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, 500, clickableLayers))
                    {
                        if (!EventSystem.current.IsPointerOverGameObject(Input.touchCount > 0 ? Input.GetTouch(0).fingerId : 0))
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
                                Building hittedBuilding = hit.collider.gameObject.GetComponent<Building>();
                                BuildingConstruction hittedBuildingConstruction = null;

                                if (hit.collider.gameObject.transform.parent)
                                    hittedBuildingConstruction = hit.collider.gameObject.transform.parent.GetComponent<BuildingConstruction>();

                                if (hittedLootContainer)
                                {
                                    TakeItems(hittedLootContainer.GetContainedLoot());
                                    hittedLootContainer.TakeItems();

                                    UnselectBuilding();
                                }
                                else if (hittedBuilding || hittedBuildingConstruction)
                                {
                                    if (hittedBuildingConstruction)
                                        hittedBuilding = hittedBuildingConstruction.transform.parent.GetComponent<Building>();

                                    ProductionBuildingComponent hittedProductionBuilding = hit.collider.gameObject.GetComponent<ProductionBuildingComponent>();

                                    if (hittedProductionBuilding)
                                    {
                                        if (hittedProductionBuilding.readyToCollect)
                                        {
                                            ItemInstance storageItemInstance = cityManager.items[gameManager.GetItemIndexByIdName(hittedProductionBuilding.GetProducedItem().itemData.itemIdName)];

                                            TakeItem(hittedProductionBuilding.TakeProducedItem(storageItemInstance.maxAmount - storageItemInstance.amount));
                                        }
                                        else
                                        {
                                            if (!isSelectedBuilding || selectedBuilding != hittedBuilding)
                                                SelectBuilding(hittedBuilding);
                                            else
                                                UnselectBuilding();
                                        }
                                    }
                                    else
                                    {
                                        if (!isSelectedBuilding || selectedBuilding != hittedBuilding)
                                            SelectBuilding(hittedBuilding);
                                        else
                                            UnselectBuilding();
                                    }
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
                    else
                    {
                        //UnselectBuilding();
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

            if (isSecondTouchPressed)
            {
                secondTouchCurrentPosition = secondTouchPositionAction.ReadValue<Vector2>();
                secondTouchMoveInput = secondTouchMoveAction.ReadValue<Vector2>();
            }

            cameraMoveVelocity.x = (firstTouchMoveInput.x + secondTouchMoveInput.x) * CameraMoveSensitivity.x;
            cameraMoveVelocity.y = (firstTouchMoveInput.y + secondTouchMoveInput.y) * CameraMoveSensitivity.y;
        }
        else if (!isFirstTouchPressed && !isSecondTouchPressed)
        {
            float CameraStopSpeed = cameraStopMoveSpeed * Time.deltaTime;
            cameraMoveVelocity = Vector2.Lerp(cameraMoveVelocity, Vector2.zero, CameraStopSpeed);
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
        //bool canPlace = true;

        //if (buildingToPlace.buildingData.buildingType != BuildingType.FloorFrame)
        //{
        //    if (cityManager.buildedFloorsCount >= buildingPlace.floorIndex + buildingToPlace.buildingData.buildingHeightInFloors &&)

        //    {
        //        for (int i = 0; i < buildingToPlace.buildingData.buildingHeightInFloors; i++)
        //        {
        //            if (buildingToPlace.buildingData.buildingType == BuildingType.Room)
        //            {
        //                int floorIndex = buildingPlace.floorIndex + i;
        //                int roomIndex = buildingPlace.buildingPlaceIndex;

        //                if (cityManager.spawnedFloors[floorIndex].roomsBuildingPlaces[roomIndex].isBuildingPlaced)
        //                {
        //                    canPlace = false;
        //                    break;
        //                }
        //            }
        //            else if (buildingToPlace.buildingData.buildingType == BuildingType.Hall)
        //            {
        //                int floorIndex = buildingPlace.floorIndex + i;

        //                if (cityManager.currentRoomsNumberOnFloor[floorIndex] > 0 || cityManager.currentHallsNumberOnFloor[floorIndex] > 0)
        //                {
        //                    canPlace = false;
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        canPlace = false;
        //    }
        //}

        //if (canPlace)
        //{
        //    List<ResourceToBuild> resourcesToBuild = buildingToPlace.buildingLevelsData[0].ResourcesToBuild;

        //    cityManager.PlaceBuilding(buildingToPlace, buildingPlace);
        //    StopPlacingBuilding();

        //    SpendItems(resourcesToBuild);
        //}

        cityManager.PlaceBuilding(buildingToPlace, buildingPlace);

        //if (buildingPlace.isBuildingPlaced)
            //StopPlacingBuilding();
    }

    public void StopPlacingBuilding(Building building)
    {
        if (buildingToPlace && building && buildingToPlace.buildingData.buildingIdName == building.buildingData.buildingIdName)
        {
            cityManager.HideBuildingPlacesByType(buildingToPlace.buildingData.buildingType);

            isBuildingToPlaceSelected = false;
            buildingToPlace = null;

            UIManager.OnBuildingPlacingStopped();
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

    private void UnselectBuilding()
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
}
