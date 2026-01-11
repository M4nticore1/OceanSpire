using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public enum ResidentWork
{
    None,
    BuildingWork,
    ConstructingBuilding,
}

public enum ElevatorPassengerState
{
    None,
    GoingForWaiting,
    Waiting,
    GoingToRiding,
    Riding,
    GoingToExit
}

public class Entity : MonoBehaviour
{
    private GameManager gameManager = null;
    private CityManager cityManager = null;
    public LevelComponent levelComponent { get; private set; } = null;
    public SelectComponent selectComponent { get; private set; } = null;
    public NavMeshAgent navMeshAgent { get; private set; } = null;

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
    public ElevatorPassengerState elevatorPassengerState { get; private set; } = ElevatorPassengerState.None;
    public bool isIdleOnElevator => elevatorPassengerState == ElevatorPassengerState.None;
    public bool isGoingForWaitingElevator => elevatorPassengerState == ElevatorPassengerState.GoingForWaiting;
    public bool isWaitingForElevator => elevatorPassengerState == ElevatorPassengerState.Waiting;
    public bool isGoingToRiding => elevatorPassengerState == ElevatorPassengerState.GoingToRiding;
    public bool isRidingOnElevator => elevatorPassengerState == ElevatorPassengerState.Riding;
    public bool isGointToExitElevator => elevatorPassengerState == ElevatorPassengerState.GoingToExit;

    // Work
    //public ResidentWork currentWork { get; private set; } = ResidentWork.None;
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
        CheckDistanceToTargetPosition();

        if (isWorking) {
            Work();
        }
    }

    // Work
    private void Work()
    {
        if (workBuilding) {
            if (!workBuilding.constructionComponent.isUnderConstruction) {
                if (workBuilding.constructionComponent.SpawnedConstruction.BuildingInteractions.Count > workerIndex) {
                    BuildingAction buildingAction = workBuilding.constructionComponent.SpawnedConstruction.BuildingInteractions[workerIndex];

                    if (buildingAction.actionTimes[currentActionIndex] > 0) {
                        currentActionTime += Time.deltaTime;
                        if (currentActionTime >= buildingAction.actionTimes[currentActionIndex]) {
                            if (currentActionIndex < buildingAction.actionTimes.Count - 1)
                                currentActionIndex++;
                            else
                                currentActionIndex = 0;

                            currentActionTime = 0;

                            Vector3 position = buildingAction.waypoints[currentActionIndex].position;
                            MoveTo(position);
                        }
                    }
                }
            }
            else {
                int levelIndex = workBuilding.levelIndex;
                List<ItemInstance> resourcesToBuild = workBuilding.ConstructionLevelsData[levelIndex].ResourcesToBuild;
                List<ItemInstance> deliveredResources = workBuilding.constructionComponent.deliveredConstructionResources;
                List<ItemInstance> incomingResources = workBuilding.constructionComponent.incomingConstructionResources;
                bool isNeededToWork = false;

                if (currentBuilding == workBuilding) {
                    float distance = Vector3.Distance(transform.position, targetPosition);
                    if (distance < applyTargetPosition) {
                        currentActionTime += Time.deltaTime;
                        if (currentActionTime >= takeItemDuration) {
                            for (int i = 0; i < carriedItems.Count; i++) {
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

                                    int itemIndex = workBuilding.ConstructionLevelsData[workBuilding.levelIndex].ResourcesToBuild[0].ItemData.ItemId;

                                    return b.storageComponent.storedItems.ContainsKey(itemIndex) && b.storageComponent.storedItems[itemIndex].Amount >= 0;
                                });
                            //else
                                //SetWork(ResidentWork.None);
                            currentActionTime = 0;
                        }
                    }
                }
                else if (currentBuilding == targetBuilding) {
                    float distance = Vector3.Distance(transform.position, targetPosition);
                    if (distance < applyTargetPosition) {
                        currentActionTime += Time.deltaTime;
                        if (currentActionTime >= takeItemDuration) {
                            for (int i = 0; i < resourcesToBuild.Count; i++) {
                                if ((deliveredResources.Count > i ? deliveredResources[i].Amount : 0) + (incomingResources.Count > i ? incomingResources[i].Amount : 0) < resourcesToBuild[i].Amount) {
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
                            //else
                                //SetWork(ResidentWork.None);
                            currentActionTime = 0;
                        }
                    }
                }
            }
        }
    }

    //public void RemoveWorkBuilding()
    //{
    //    if (currentWork == ResidentWork.BuildingWork)
    //        workBuilding.RemoveWorker(this);
    //    workBuilding = null;

    //    currentWork = ResidentWork.None;

    //    OnWorkerRemove?.Invoke();
    //}

    public void SetWorkerIndex(int index)
    {
        workerIndex = index;
    }

    public void SetWork(Building workBuilding = null)
    {
        //currentWork = work;

        if (workBuilding) {
            if (!workBuilding.constructionComponent.isUnderConstruction) {
                this.workBuilding = workBuilding;
                workBuilding.AddWorker(this);

                if (currentBuilding == workBuilding)
                    StartWorking();
                else
                    SetTargetBuilding(currentBuilding ? currentBuilding.buildingPlace : null, workBuilding);

                OnWorkerAdd?.Invoke();
            }
            else {
                int levelIndex = workBuilding.levelIndex;
                List<ItemInstance> resourcesToBuild = workBuilding.ConstructionLevelsData[levelIndex].ResourcesToBuild;

                for (int i = 0; i < resourcesToBuild.Count; i++) {
                    if (workBuilding.constructionComponent.incomingConstructionResources.Count <= i || workBuilding.constructionComponent.incomingConstructionResources[i].Amount < resourcesToBuild[i].Amount) {
                        if (SetTargetBuilding(workBuilding.buildingPlace, b => {
                            if (!b || !b.storageComponent || b == workBuilding) return false;

                            int itemIndex = workBuilding.ConstructionLevelsData[workBuilding.levelIndex].ResourcesToBuild[0].ItemData.ItemId;

                            return b.storageComponent.storedItems.ContainsKey(itemIndex) && b.storageComponent.storedItems[itemIndex].Amount >= 0;
                        }))
{
                            this.workBuilding = workBuilding;
                            StartWorking();
                        }
                        break;
                    }
                }
            }
        }
    }

    public void RemoveWork()
    {
        StopWorking();
        pathBuildings.Clear();
        pathIndex = 0;
        if (isRidingOnElevator)
            SetElevatorPassengerState(ElevatorPassengerState.GoingToExit);
        else if (isWaitingForElevator)
            SetElevatorPassengerState(ElevatorPassengerState.None);

        //currentWork = ResidentWork.None;
        if (workBuilding) {
            if (workBuilding as PierBuilding)
                currentBoat.ReturnToDock();

            workBuilding.RemoveWorker(this);
            workBuilding = null;
        }

        StopMoving();
        OnWorkerRemove?.Invoke();
    }

    private void StartWorking()
    {
        isWorking = true;
        if (workBuilding)
            workBuilding.AddCurrentWorker(this);
    }

    private void StopWorking()
    {
        if (!isWorking) return;

        isWorking = false;
        if (workBuilding)
            workBuilding.RemoveCurrentWorker(this);
    }

    private void StartConstructingBuilding()
    {

    }

    // Movement
    private void FollowPath()
    {
        if (!navMeshAgent.enabled) return;
        if (currentElevator && nextPathElevator && currentElevator.placeIndex == nextPathElevator.placeIndex) {
            ElevatorPlatformConstruction cabin = currentPathElevator.elevatorPlatform;

            if (!isRidingOnElevator) {
                if (!cabin.isMoving && cabin.floorIndex == floorIndex && cabin.ridingPassengers.Count < currentPathElevator.currentLevelData.maxResidentsCount) {
                    SetElevatorPassengerState(ElevatorPassengerState.GoingToRiding);
                }
                else {
                    SetElevatorPassengerState(ElevatorPassengerState.GoingForWaiting);
                }
            }
        }
        else if (currentPathBuilding) {
            if (!isRidingOnElevator) {
                Vector3 position = Vector3.zero;

                if (currentPathBuilding == targetBuilding) {
                    if (workBuilding) {
                        if (!workBuilding.constructionComponent.isUnderConstruction) {
                            position = currentPathBuilding.constructionComponent.GetInteractionPosition(workerIndex);
                        }
                        else {
                            position = currentPathBuilding.constructionComponent.GetPickupItemPointPosition();
                        }
                    }
                }
                else if (currentPathElevator) {
                    position = currentPathBuilding.constructionComponent.GetInteractionPosition(currentPathElevator.elevatorWaitingPassengers.Count);
                }

                MoveTo(position);
            }
        }
        else {
            Vector3 position = currentBuilding.constructionComponent.GetInteractionPosition(currentBuilding.enteredEntities.Count);
            MoveTo(position);
        }

        if (currentBuilding == currentPathBuilding) {
            pathIndex++;
        }
    }

    public void Move(Vector3 direction, float speed)
    {
        transform.position += direction * speed;
    }

    public void MoveTo(Vector3 position)
    {
        if (!navMeshAgent.enabled) return;

        isMoving = true;
        targetPosition = position;
        navMeshAgent.SetDestination(position);
    }

    public void StopMoving()
    {
        if (!isMoving) return;
        Debug.Log("StopMoving");
        isMoving = false;
        if (navMeshAgent.enabled)
            navMeshAgent.ResetPath();

        if (isRidingOnElevator) {
            navMeshAgent.enabled = false;
        }
        else if (workBuilding && workBuilding as PierBuilding) {
            StartEnteringBoat();
        }

        OnEntityStopped?.Invoke(this);
    }

    private void CheckDistanceToTargetPosition()
    {
        if (isMoving && navMeshAgent.enabled && !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)) {
            StopMoving();
        }
    }

    // Buildings
    public virtual void EnterBuilding(Building building)
    {
        if (building == null) {
            Debug.LogWarning("building is NULL");
            return;
        }
        if (building == currentBuilding) {
            Debug.LogWarning("building is a currentBuilding already");
            return;
        }

        currentBuilding = building;
        building.EnterBuilding(this);

        if (currentBuilding == targetBuilding) {
            if (currentBuilding == workBuilding) {
                StartWorking();
            }
            else {
                // Потом будет
            }
        }
        else {
            FollowPath();
        }
    }

    public virtual void ExitBuilding()
    {

    }

    public Building SetTargetBuilding(BuildingPlace startBuildingPlace, Building targetBuilding)
    {
        pathIndex = 0;

        if (!cityManager) {
            Debug.LogError("cityManager is NULL");
            return null;
        }

        Debug.Log(isRidingOnElevator);
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

        if (!workBuilding) {
            Building building = construction.GetComponent<Building>();
            SetWork(building);
        }
        else {
            SetTargetBuilding(currentBuilding ? currentBuilding.buildingPlace : null, b => b.floorIndex == targetBuilding.floorIndex && b.placeIndex == targetBuilding.placeIndex);
        }

    }

    private void StartEnteringBoat()
    {
        TimerManager.StartTimer(enteringBoatTime, EnterBoat);
    }

    // Elevators
    public void SetElevatorPassengerState(ElevatorPassengerState state)
    {
        if (state == elevatorPassengerState) {
            return;
        }
        if (currentElevator == null) {
            Debug.LogError("currentElevator is NULL");
            return;
        }

        // Exit old state
        switch (elevatorPassengerState) {
            case ElevatorPassengerState.Waiting:
                currentElevator?.RemoveWaitingPassenger(this);
                break;
            case ElevatorPassengerState.Riding:
                currentElevator?.RemoveRidingPassenger(this);
                break;
        }

        // Enter new state
        switch (state) {
            case ElevatorPassengerState.None:
                StopMoving();
                currentElevator.RemoveWaitingPassenger(this);
                currentElevator.RemoveRidingPassenger(this);
                break;
            case ElevatorPassengerState.GoingForWaiting:
                MoveTo(currentElevator.transform.position);
                break;
            case ElevatorPassengerState.Waiting:
                StopMoving();
                currentElevator.AddWaitingPassenger(this);
                break;
            case ElevatorPassengerState.GoingToRiding:
                MoveTo(currentElevator.GetPlatformRidingPosition());
                break;
            case ElevatorPassengerState.Riding:
                StopMoving();
                currentElevator.AddRidingPassenger(this);
                break;
        }

        elevatorPassengerState = state;
    }

    //private void StartWaitingForElevator()
    //{
    //    if (isWaitingForElevator || isRidingOnElevator) return;
    //    elevatorPassengerState = ElevatorPassengerState.Waiting;
    //    ElevatorBuilding elevatorBuilding = pathBuildings[pathIndex] as ElevatorBuilding;

    //    elevatorBuilding.AddWaitingPassenger(this);
    //    StopRidingOnElevator();   

    //    if (navMeshAgent.enabled) {
    //        Vector3 position = elevatorBuilding.constructionComponent.GetInteractionPosition(elevatorBuilding.elevatorWaitingPassengers.Count);
    //        MoveTo(position);
    //    }
    //}

    //private void StopWaitingForElevator()
    //{
    //    if (!isWaitingForElevator) return;
    //    elevatorPassengerState = ElevatorPassengerState.None;
    //    currentElevator.RemoveWaitingPassenger(this);
    //}

    //private void StartRidingOnElevator()
    //{
    //    if (isRidingOnElevator || isWaitingForElevator) return;
    //    StopWaitingForElevator();
    //    elevatorPassengerState = ElevatorPassengerState.Riding;
    //    currentElevator.AddRidingPassenger(this);
    //    FollowPath();
    //}

    //private void StopRidingOnElevator()
    //{
    //    if (!isRidingOnElevator) return;
    //    elevatorPassengerState = ElevatorPassengerState.None;
    //    navMeshAgent.enabled = true;
    //    currentElevator.RemoveRidingPassenger(this);
    //    FollowPath();
    //}

    private void OnElevatorPlatformStopped(ElevatorPlatformConstruction elevatorPlatform)
    {
        if (currentElevator && currentElevator.elevatorPlatform == elevatorPlatform && currentElevator.elevatorPlatform.floorIndex == floorIndex) {
            if (isRidingOnElevator) {
                SetElevatorPassengerState(ElevatorPassengerState.GoingToExit);
            }
            else if (isWaitingForElevator) {
                SetElevatorPassengerState(ElevatorPassengerState.GoingToRiding);
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
            Boat boat = cityManager.GetBoatByIndex(workerIndex);
            currentBoat = boat;
            currentBoat.EnterBoat(this);

            navMeshAgent.enabled = false;
            transform.position = boat.SeatSlot.position;
        }
    }

    private void StartExitingBoat()
    {
        TimerManager.StartTimer(enteringBoatTime, ExitBoat);
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
            int levelIndex = building.levelIndex;
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
