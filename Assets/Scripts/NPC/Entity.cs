using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public enum ResidentWork
{
    None,
    BuildingWork,
    ConstructingBuilding,
}

public class Entity : MonoBehaviour
{
    private GameManager gameManager = null;
    private CityManager cityManager = null;
    public LevelComponent levelComponent { get; private set; } = null;
    public SelectComponent selectComponent { get; private set; } = null;
    private NavMeshAgent navMeshAgent = null;

    // Stats
    [SerializeField] private int maxHealth = 100;
    public int MaxHealth => maxHealth;
    private int currentHealth = 100;

    private const int currentMaxCarryWeight = 2000;

    public string firstName = "";
    public string lastName = "";

    // Path
    public List<Building> pathBuildings = new List<Building>();
    public Building currentBuilding = null;
    public ElevatorBuilding currentElevator => currentBuilding as ElevatorBuilding;
    public Building currentPathBuilding => pathBuildings.Count > 0 ? pathBuildings[pathIndex] : null;
    public ElevatorBuilding currentPathElevator => currentPathBuilding as ElevatorBuilding;
    public Building nextPathBuilding => pathBuildings.Count > pathIndex + 1 ? pathBuildings[pathIndex + 1] : null;
    public ElevatorBuilding nextPathElevator => nextPathBuilding as ElevatorBuilding;
    public Building lastPathBuilding => pathIndex > 0 ? pathBuildings[pathIndex - 1] : null;
    public ElevatorBuilding lastPathElevator => lastPathBuilding as ElevatorBuilding;
    public Building targetBuilding => pathBuildings.Count > 0 ? pathBuildings[pathBuildings.Count - 1] : null;
    public int pathIndex = 0;

    // Positions
    public int floorIndex => currentBuilding.floorIndex;
    public Vector3 targetPosition = Vector3.zero;
    private const float applyTargetPosition = 0.6f;
    private const float applyTargetMagnitude = 0.1f;
    private const float enteringBoatTime = 1;

    // Boats
    private Boat currentBoat = null;

    // States
    public bool isMoving = false;
    public bool isRidingOnElevator { get; private set; } = false;
    public bool isWaitingForElevator { get; private set; } = false;
    //public bool isWalkingToElevator { get; private set; } = false;

    // Work
    public ResidentWork currentWork { get; private set; } = ResidentWork.None;
    public bool isWorking { get; private set; } = false;
    public int workerIndex { get; private set; } = 0;
    public Building workBuilding { get; private set; } = null;

    // Inventory
    public List<ItemInstance> carriedItems = new List<ItemInstance>();
    public Dictionary<int, ItemInstance> carriedItemsDict = new Dictionary<int, ItemInstance>();

    // Times
    private double currentActionTime = 0.0f;
    private int currentActionIndex = 0;
    private const float takeItemDuration = 1.0f;

    public static event Action OnWorkerAdd;
    public static event Action OnWorkerRemove;
    public static event Action<Entity> OnEntityStopped;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        cityManager = FindAnyObjectByType<CityManager>();
        levelComponent = GetComponent<LevelComponent>();
        selectComponent = GetComponent<SelectComponent>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        ConstructionComponent.onAnyConstructionStartConstructing += OnBuildingStartConstructing;
        Boat.onBoatDocked += OnBoatDocked;
        ElevatorPlatformConstruction.onElevatorPlatformStopped += OnElevatorPlatformStopped;
        ElevatorPlatformConstruction.onElevatorPlatformChangedFloor += OnElevatorPlatformChangedFloor;
    }

    private void OnDisable()
    {
        ConstructionComponent.onAnyConstructionFinishConstructing -= OnBuildingStartConstructing;
        Boat.onBoatDocked -= OnBoatDocked;
        ElevatorPlatformConstruction.onElevatorPlatformStopped -= OnElevatorPlatformStopped;
        ElevatorPlatformConstruction.onElevatorPlatformChangedFloor -= OnElevatorPlatformChangedFloor;
    }

    private void Start()
    {

    }

    private void Update()
    {
        if (isMoving && !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)){
            StopMoving();
        }

        if (isWorking) {
            Work();
        }
    }

    // Work
    private void Work()
    {
        if (currentWork == ResidentWork.BuildingWork)
        {
            if (workBuilding.constructionComponent.SpawnedConstruction.BuildingInteractions.Count > workerIndex)
            {
                BuildingAction buildingAction = workBuilding.constructionComponent.SpawnedConstruction.BuildingInteractions[workerIndex];

                if (buildingAction.actionTimes[currentActionIndex] > 0)
                {
                    currentActionTime += Time.deltaTime;
                    if (currentActionTime >= buildingAction.actionTimes[currentActionIndex])
                    {
                        if (currentActionIndex < buildingAction.actionTimes.Count - 1)
                            currentActionIndex++;
                        else
                            currentActionIndex = 0;

                        currentActionTime = 0;

                        Vector3 position = buildingAction.waypoints[currentActionIndex].position;
                        StartMoving(position);
                    }
                }
            }
        }
        else if (currentWork == ResidentWork.ConstructingBuilding)
        {
            int levelIndex = workBuilding.levelComponent.LevelIndex;
            List<ItemInstance> resourcesToBuild = workBuilding.ConstructionLevelsData[levelIndex].ResourcesToBuild;
            List<ItemInstance> deliveredResources = workBuilding.constructionComponent.deliveredConstructionResources;
            List<ItemInstance> incomingResources = workBuilding.constructionComponent.incomingConstructionResources;
            bool isNeededToWork = false;

            if (currentBuilding == workBuilding)
            {
                float distance = Vector3.Distance(transform.position, targetPosition);
                if (distance < applyTargetPosition)
                {
                    currentActionTime += Time.deltaTime;
                    if (currentActionTime >= takeItemDuration)
                    {
                        for (int i = 0; i < carriedItems.Count; i++)
                        {
                            int itemId = carriedItems[i].ItemData.ItemId;
                            int amountToAdd = carriedItems[i].Amount;
                            int amountToSpend = currentBuilding.constructionComponent.AddConstructionResources(itemId, amountToAdd);
                            SpendItem(itemId, amountToSpend);

                            if ((deliveredResources.Count > i ? deliveredResources[i].Amount : 0) + (incomingResources.Count > i ? incomingResources[i].Amount : 0) < resourcesToBuild[i].Amount)
                                isNeededToWork = true;
                        }

                        if (isNeededToWork)
                            SetTargetBuilding(currentBuilding.buildingPlace, b =>
                            {
                                if (!b.storageComponent || (b.floorIndex == workBuilding.floorIndex && b.placeIndex == workBuilding.placeIndex)) return false;

                                int itemIndex = workBuilding.ConstructionLevelsData[workBuilding.levelComponent.LevelIndex].ResourcesToBuild[0].ItemData.ItemId;

                                return b.storageComponent.storedItems.ContainsKey(itemIndex) && b.storageComponent.storedItems[itemIndex].Amount >= 0;
                            });
                        else
                            SetWork(ResidentWork.None);
                        currentActionTime = 0;
                    }
                }
            }
            else if (currentBuilding == targetBuilding)
            {
                float distance = Vector3.Distance(transform.position, targetPosition);
                if (distance < applyTargetPosition)
                {
                    currentActionTime += Time.deltaTime;
                    if (currentActionTime >= takeItemDuration)
                    {
                        for (int i = 0; i < resourcesToBuild.Count; i++)
                        {
                            if ((deliveredResources.Count > i ? deliveredResources[i].Amount : 0) + (incomingResources.Count > i ? incomingResources[i].Amount : 0) < resourcesToBuild[i].Amount)
                            {
                                int itemId = resourcesToBuild[i].ItemData.ItemId;
                                if (targetBuilding.constructionComponent.incomingConstructionResourcesDict.ContainsKey(itemId))
                                    targetBuilding.constructionComponent.incomingConstructionResourcesDict[itemId].SetAmount(0);

                                int remainedAmount = resourcesToBuild[i].Amount - (deliveredResources.Count > i ? deliveredResources[i].Amount : 0) + (incomingResources.Count > i ? incomingResources[i].Amount : 0);
                                int amountToTake = currentBuilding.storageComponent.SpendItem(itemId, math.min(currentMaxCarryWeight, remainedAmount));
                                TakeItem(itemId, amountToTake);
                                int amountToIncoming = carriedItemsDict[itemId].Amount;
                                targetBuilding.constructionComponent.AddIncomingConstructionResources(itemId, amountToIncoming);

                                if ((deliveredResources.Count > i ? deliveredResources[i].Amount : 0) + (incomingResources.Count > i ? incomingResources[i].Amount : 0) < resourcesToBuild[i].Amount)
                                    isNeededToWork = true;
                            }
                        }

                        if (isNeededToWork)
                            SetTargetBuilding(currentBuilding.buildingPlace, b => b ? b == workBuilding : false);
                        else
                            SetWork(ResidentWork.None);
                        currentActionTime = 0;
                    }
                }
            }
        }
    }

    public void RemoveWorkBuilding()
    {
        if (currentWork == ResidentWork.BuildingWork)
            workBuilding.RemoveWorker(this);
        workBuilding = null;

        currentWork = ResidentWork.None;
        RemoveWork();

        OnWorkerRemove?.Invoke();
    }

    public void SetWorkerIndex(int index)
    {
        workerIndex = index;
    }

    public void SetWork(ResidentWork newWork, Building newWorkBuilding = null)
    {
        currentWork = newWork;

        if (newWorkBuilding) {
            if (newWork == ResidentWork.BuildingWork) {
                workBuilding = newWorkBuilding;
                newWorkBuilding.AddWorker(this);

                SetTargetBuilding(currentBuilding ? currentBuilding.buildingPlace : null, newWorkBuilding);

                OnWorkerAdd?.Invoke();
            }
            else if (newWork == ResidentWork.ConstructingBuilding) {
                int levelIndex = newWorkBuilding.levelComponent.LevelIndex;
                List<ItemInstance> resourcesToBuild = newWorkBuilding.ConstructionLevelsData[levelIndex].ResourcesToBuild;

                for (int i = 0; i < resourcesToBuild.Count; i++) {
                    if (newWorkBuilding.constructionComponent.incomingConstructionResources.Count <= i || newWorkBuilding.constructionComponent.incomingConstructionResources[i].Amount < resourcesToBuild[i].Amount) {
                        if (SetTargetBuilding(newWorkBuilding.buildingPlace, b => {
                            if (!b || !b.storageComponent || b == newWorkBuilding) return false;

                            int itemIndex = newWorkBuilding.ConstructionLevelsData[newWorkBuilding.levelComponent.LevelIndex].ResourcesToBuild[0].ItemData.ItemId;

                            return b.storageComponent.storedItems.ContainsKey(itemIndex) && b.storageComponent.storedItems[itemIndex].Amount >= 0;
                        }))
{
                            workBuilding = newWorkBuilding;
                            StartWorking();
                        }

                        break;
                    }
                }
            }
        }
    }

    private void RemoveWork()
    {
        StopWorking();
        pathBuildings.Clear();
        pathIndex = 0;
        if (isRidingOnElevator && !currentElevator.elevatorPlatform.isMoving)
            StopRidingOnElevator();
        else if (isWaitingForElevator)
            StopWaitingForElevator();

        if (workBuilding as PierBuilding) {
            currentBoat.ReturnToDock();
        }
    }

    private void StartWorking()
    {
        isWorking = true;
    }

    private void StopWorking()
    {
        isWorking = false;
        //navMeshAgent.ResetPath();
    }

    private void StartConstructingBuilding()
    {

    }

    // Movement
    private void FollowPath()
    {
        if (currentElevator && nextPathElevator && currentElevator.placeIndex == nextPathElevator.placeIndex) {
            ElevatorPlatformConstruction platform = currentPathElevator.elevatorPlatform;

            if (!isRidingOnElevator) {
                if (!platform.isMoving && platform.floorIndex == floorIndex && platform.ridingPassengers.Count < currentPathElevator.currentLevelData.maxResidentsCount) {
                    StartRidingOnElevator();
                }
                else {
                    StartWaitingForElevator();
                }
            }
        }
        else if (currentPathBuilding) {
            if (!isRidingOnElevator) {
                Vector3 position = Vector3.zero;

                if (currentPathBuilding == targetBuilding) {
                    if (workBuilding) {
                        if (currentWork == ResidentWork.BuildingWork) {
                            position = currentPathBuilding.constructionComponent.GetInteractionPosition(workerIndex);
                        }
                        else if (currentWork == ResidentWork.ConstructingBuilding) {
                            position = currentPathBuilding.constructionComponent.GetPickupItemPointPosition();
                        }
                    }
                }
                else if (currentPathElevator) {
                    position = currentPathBuilding.constructionComponent.GetInteractionPosition(currentPathElevator.elevatorWaitingPassengers.Count);
                }

                StartMoving(position);
            }
        }
        else {
            Vector3 position = currentBuilding.constructionComponent.GetInteractionPosition(currentBuilding.entities.Count);
            StartMoving(position);
        }

        if (currentBuilding == currentPathBuilding) {
            pathIndex++;
        }
    }

    public void Move(Vector3 direction, float speed)
    {
        transform.position += direction * speed;
    }

    private void StartMoving(Vector3 position)
    {
        isMoving = true;
        targetPosition = position;
        navMeshAgent.SetDestination(position);
    }

    private void StopMoving()
    {
        isMoving = false;
        navMeshAgent.ResetPath();
        OnEntityStopped?.Invoke(this);

        if (isRidingOnElevator) {
            navMeshAgent.enabled = false;
        }
        else if (currentWork == ResidentWork.BuildingWork){
            if (workBuilding as PierBuilding) {
                StartEnteringBoat();
            }
        }
    }

    // Buildings
    public virtual void EnterBuilding(Building building)
    {
        if (building) {
            currentBuilding = building;
            building.EnterBuilding(this);

            if (building == targetBuilding) {
                if (currentWork != ResidentWork.None) {
                    StartWorking();
                }
            }
            else if (currentBuilding == currentPathBuilding) {
                FollowPath();
            }
        }
        else {
            Debug.LogWarning("building is NULL");
        }
    }

    public virtual void ExitBuilding()
    {
        //currentBuilding = null;
    }

    public Building SetTargetBuilding(BuildingPlace startBuildingPlace, Building targetBuilding)
    {
        pathIndex = 0;

        if (!cityManager) {
            Debug.LogError("cityManager is NULL");
            return null;
        }

        cityManager.FindPathToBuilding(startBuildingPlace, targetBuilding, ref pathBuildings, isRidingOnElevator);

        if (!targetBuilding) {
            return null;
        }

        FollowPath();
        return targetBuilding;
    }

    public Building SetTargetBuilding(BuildingPlace startBuildingPlace, Func<Building, bool> targetBuildingCondition)
    {
        if (!cityManager) {
            Debug.LogError("cityManager is NULL");
            return null;
        }

        cityManager.FindPathToBuilding(startBuildingPlace, targetBuildingCondition, ref pathBuildings, isRidingOnElevator);

        pathIndex = 0;

        if (!targetBuilding)
            return null;

        FollowPath();
        return targetBuilding;
    }

    private void OnBuildingStartConstructing(ConstructionComponent building)
    {
        //StartCoroutine(OnBuildingStartConstructingCoroutine(building));
    }

    private IEnumerator OnBuildingStartConstructingCoroutine(ConstructionComponent construction)
    {
        yield return CityManager.bakeNavMeshSurfaceCoroutine;

        if (currentWork == ResidentWork.None) {
            Building building = construction.GetComponent<Building>();
            SetWork(ResidentWork.ConstructingBuilding, building);
        }
        else {
            SetTargetBuilding(currentBuilding ? currentBuilding.buildingPlace : null, b => b.floorIndex == targetBuilding.floorIndex && b.placeIndex == targetBuilding.placeIndex);
        }

    }

    private void StartEnteringBoat()
    {
        TimerManager.SetTimer(enteringBoatTime, EnterBoat);
    }

    // Elevators
    private void StartWaitingForElevator()
    {
        Debug.Log("StartWaitingForElevator");

        isWaitingForElevator = true;
        ElevatorBuilding elevatorBuilding = pathBuildings[pathIndex] as ElevatorBuilding;

        elevatorBuilding.AddWaitingPassenger(this);
        StopRidingOnElevator();

        Vector3 position = elevatorBuilding.constructionComponent.GetInteractionPosition(elevatorBuilding.elevatorWaitingPassengers.Count);
        StartMoving(position);
    }

    private void StopWaitingForElevator()
    {
        if (!isWaitingForElevator) return;
        Debug.Log("StopWaitingForElevator");

        isWaitingForElevator = false;
        currentElevator.RemoveWaitingPassenger(this);
    }

    private void StartRidingOnElevator()
    {
        isRidingOnElevator = true;

        currentElevator.AddRidingPassenger(this);
        StopWaitingForElevator();

        Vector3 position = currentElevator.GetPlatformRidingPosition();
        StartMoving(position);
    }

    private void StopRidingOnElevator()
    {
        if (!isRidingOnElevator) return;

        isRidingOnElevator = false;
        isRidingOnElevator = false;
        navMeshAgent.enabled = true;
        currentElevator.RemoveRidingPassenger(this);
        FollowPath();
    }

    private void OnElevatorPlatformStopped(ElevatorPlatformConstruction elevatorPlatform)
    {
        if (currentElevator && currentElevator.elevatorPlatform == elevatorPlatform && currentElevator.elevatorPlatform.floorIndex == floorIndex) {
            if (isRidingOnElevator) {
                if (!lastPathBuilding || lastPathBuilding == currentBuilding)
                    StopRidingOnElevator();
            }
            else if (isWaitingForElevator) {
                StartRidingOnElevator();
            }
        }
    }

    private void OnElevatorPlatformChangedFloor(ElevatorPlatformConstruction elevatorPlatform)
    {
        if (isRidingOnElevator && currentElevator && currentElevator.elevatorPlatform == elevatorPlatform) {
            EnterBuilding(elevatorPlatform.ownedElevator);
        }
    }

    // Boat
    private void EnterBoat()
    {
        PierBuilding workPier = workBuilding as PierBuilding;
        if (workPier)
        {
            PierBuilding pier = workBuilding as PierBuilding;
            Boat boat = pier.GetBoatByIndex(workerIndex);
            currentBoat = boat;
            currentBoat.EnterBoat(this);

            navMeshAgent.enabled = false;
            transform.position = boat.SeatSlot.position;
        }
    }

    private void StartExitingBoat()
    {
        TimerManager.SetTimer(enteringBoatTime, ExitBoat);
    }

    private void ExitBoat()
    {
        transform.position = currentBoat.ownedPier.constructionComponent.GetInteractionPosition(workBuilding.workers.Count - 1);
        navMeshAgent.enabled = true;
        StopWorking();
    }

    private void OnBoatDocked(Boat boat)
    {
        //if (currentBoat == boat)
        //{
        //    StartExitingBoat();
        //}
    }

    // Actions
    private void TakeItem(int itemId, int itemAmount)
    {
        TakeItem_Internal(itemId, itemAmount);
    }

    private void TakeItem(ItemInstance item)
    {
        TakeItem_Internal(item.ItemData.ItemId, item.Amount);
    }

    private void TakeItem_Internal(int itemId, int itemAmount)
    {
        if (!carriedItemsDict.ContainsKey(itemId))
        {
            ItemInstance item = new ItemInstance(gameManager.LootList.lootById[itemId]); // The same item instance for list and dictionary.
            carriedItems.Add(item);
            carriedItemsDict.Add(itemId, item);
        }

        // We can change only the list or dictionary because we use the same item instance for them.
        carriedItemsDict[itemId].AddAmount(itemAmount);
    }

    private int SpendItem(int itemId, int amount)
    {
        return SpendItem_Internal(itemId, carriedItemsDict[itemId].SubtractAmount(amount));
    }

    private int SpendItem(ItemInstance item)
    {
        int id = item.ItemData.ItemId;
        int amount = item.Amount;
        return SpendItem_Internal(id, carriedItemsDict[id].SubtractAmount(amount));
    }

    private int SpendItem_Internal(int itemId, int amount)
    {
        return carriedItemsDict[itemId].SubtractAmount(amount);
    }

    private void DeliverItem(Building building, ItemInstance item)
    {
        DeliverItem_Internal(building, item);
    }

    private void DeliverItems(Building building, List<ItemInstance> items)
    {
        for (int i = 0; i < items.Count; i++)
            DeliverItem_Internal(building, items[i]);
    }

    private void DeliverItem_Internal(Building building, ItemInstance item)
    {
        if (building.constructionComponent.isUnderConstruction)
        {
            int levelIndex = building.levelComponent.LevelIndex;
            List<ItemInstance> constructionResources = building.ConstructionLevelsData[levelIndex].ResourcesToBuild;
            for (int j = 0; j < building.ConstructionLevelsData[levelIndex].ResourcesToBuild.Count; j++)
            {
                if (item.ItemData.ItemId == building.ConstructionLevelsData [levelIndex].ResourcesToBuild[j].ItemData.ItemId)
                {
                    int id = item.ItemData.ItemId;
                    int amountToSpend = building.constructionComponent.AddConstructionResources(item);
                    SpendItem(id, amountToSpend);
                }
            }
        }
        else if (building.storageComponent)
        {
            if (building.storageComponent.storedItems.ContainsKey(item.ItemData.ItemId))
            {
                int id = item.ItemData.ItemId;
                int amountToSpend = building.storageComponent.AddItem(item);
                SpendItem(id, amountToSpend);
                //building.storageComponent.AddItem(item.ItemData.ItemId, SpendItem(item));
            }
        }
    }

    public void TakeDamage(int damange)
    {
        if (damange > 0)
        {
            currentHealth -= damange;

            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }
    }

    // Select
    public void Select()
    {
        foreach (GameObject child in GameUtils.GetAllChildren(transform))
        {
            child.layer = LayerMask.NameToLayer("Outlined");
        }
    }

    public void Deselect()
    {
        foreach (GameObject child in GameUtils.GetAllChildren(transform))
        {
            child.layer = LayerMask.NameToLayer("Default");
        }
    }
}
