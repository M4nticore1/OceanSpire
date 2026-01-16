using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Mathematics;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public enum ResidentWork
{
    None,
    BuildingWork,
    ConstructingBuilding,
}

public enum ElevatorPassengerState
{
    None,
    GoingToWaiting,
    Waiting,
    GoingToRiding,
    Riding
}

public class Creature : MonoBehaviour
{
    private GameManager gameManager = null;
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
    public Building CurrentBuilding = null;
    public ElevatorBuilding CurrentElevator => CurrentBuilding as ElevatorBuilding;
    public Building CurrentPathBuilding => pathBuildings.Count > 0 ? pathBuildings[pathIndex] : null;
    public ElevatorBuilding CurrentPathElevator => CurrentPathBuilding as ElevatorBuilding;
    public Building NextPathBuilding => pathBuildings.Count > pathIndex + 1 ? pathBuildings[pathIndex + 1] : null;
    public ElevatorBuilding NextPathElevator => NextPathBuilding as ElevatorBuilding;
    public Building LastPathBuilding => pathIndex > 0 ? pathBuildings[pathIndex - 1] : null;
    public ElevatorBuilding LastPathElevator => LastPathBuilding as ElevatorBuilding;
    public Building TargetBuilding => pathBuildings.Count > 0 ? pathBuildings[pathBuildings.Count - 1] : null;
    public int pathIndex = 0;

    // Positions
    public int floorIndex => ((TowerBuilding)CurrentBuilding).floorIndex;
    public int buildingIndex => ((TowerBuilding)CurrentBuilding).placeIndex;
    public Vector3 targetPosition = Vector3.zero;
    private const float applyTargetPosition = 0.6f;
    private const float applyTargetMagnitude = 0.1f;
    private const float enteringBoatTime = 1;

    // Boats
    private Boat currentBoat = null;

    // States
    public bool isMoving = false;
    public ElevatorPassengerState elevatorPassengerState = ElevatorPassengerState.None;
    public bool IsUsingElevator => elevatorPassengerState != ElevatorPassengerState.None;
    public bool IsGoingToWaitingForElevator => elevatorPassengerState == ElevatorPassengerState.GoingToWaiting;
    public bool IsWaitingForElevator => elevatorPassengerState == ElevatorPassengerState.Waiting;
    public bool IsGoingToRidingOnElevator => elevatorPassengerState == ElevatorPassengerState.GoingToRiding;
    public bool IsRidingOnElevator => elevatorPassengerState == ElevatorPassengerState.Riding;

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
    public static event Action<Creature> OnEntityStopped;

    public void Initialize(GameManager gameManager)
    {
        this.gameManager = gameManager;
        levelComponent = GetComponent<LevelComponent>();
        selectComponent = GetComponent<SelectComponent>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        ConstructionComponent.onAnyConstructionStartConstructing += OnBuildingStartConstructing;
        Boat.onBoatDocked += OnBoatDocked;
        //ElevatorPlatformConstruction.onElevatorPlatformStopped += OnElevatorPlatformStopped;
        //ElevatorPlatformConstruction.onElevatorPlatformChangedFloor += OnElevatorPlatformChangedFloor;
    }

    private void OnDisable()
    {
        ConstructionComponent.onAnyConstructionFinishConstructing -= OnBuildingStartConstructing;
        Boat.onBoatDocked -= OnBoatDocked;
        ElevatorPlatformConstruction.onElevatorPlatformStopped -= OnElevatorPlatformStopped;
        ElevatorPlatformConstruction.onElevatorPlatformChangedFloor -= OnElevatorCabinChangedFloor;
    }

    private void Update()
    {
        if (CheckDistanceToTargetPosition()) {
            DecideAction();
            if (IsWaitingForElevator || isWorking || !CurrentPathBuilding)
                StopMoving();
        }

        if (isWorking) {
            Working();
        }
    }

    // Main
    public void DecideAction()
    {
        Debug.Log("DecideAction");
        if (workBuilding) {
            // Start working
            if (CurrentBuilding == workBuilding) {
                Debug.Log("Start working");
                if (!isWorking) {
                    StartWorking();
                    return;
                }
                return;
            }

            // Set target
            if (TargetBuilding != workBuilding) {
                ResetPath();
                if (SetTargetBuilding(workBuilding)) {
                    Debug.Log("Set target " + TargetBuilding);
                    ContinuePath();
                    return;
                }
                return;
            }

            // Continue Path
            if (CurrentBuilding == CurrentPathBuilding) {
                ContinuePath();
                return;
            }

            // Use elevator
            if (CurrentElevator) {
                UseElevator();
                FollowPath();
                return;
            }

            // Pier
            if (!CurrentBuilding) {
                if (workBuilding as PierBuilding) {
                    StartEnteringBoat();
                    return;
                }
                return;
            }
            return;
        }

        if (!workBuilding) {
            if (TargetBuilding) {
                if (IsUsingElevator) {
                    ResetPath();
                    UseElevator();
                    FollowPath();
                    return;
                }

                if (!IsUsingElevator) {
                    ResetPath();
                    return;
                }
                return;
            }

            if (!TargetBuilding) {
                if (IsUsingElevator) {
                    UseElevator();
                    FollowPath();
                    return;
                }
                return;
            }
            return;
        }
    }

    private void ContinuePath()
    {
        Debug.Log("ContinuePath");
        // Enter Building
        if (CurrentBuilding == CurrentPathBuilding) {
            // Enter elevator
            if (CurrentElevator) {
                UseElevator();
                FollowPath();
                return;
            }

            if (CurrentBuilding) {
                FollowPath();
                return;
            }

            if (!CurrentBuilding) {
                FollowPath();
                return;
            }
            return;
        }

        if (CurrentBuilding != CurrentPathBuilding) {

            if (CurrentElevator) {
                UseElevator();
                FollowPath();
                return;
            }

            if (CurrentBuilding) {
                FollowPath();
                return;
            }

            if (!CurrentBuilding) {
                FollowPath();
                return;
            }
            return;
        }
    }

    private void UseElevator()
    {
        Debug.Log("Use elevator");
        //if (!CurrentPathElevator) {
        //    SetElevatorPassengerState(ElevatorPassengerState.None);
        //    return;
        //}

        if (!IsUsingElevator && CurrentPathElevator && CurrentPathElevator == CurrentElevator) {
            if (CurrentElevator.IsPossibleToEnter()) {
                SetElevatorPassengerState(ElevatorPassengerState.GoingToRiding);
                return;
            }

            if (!CurrentElevator.IsPossibleToEnter()) {
                SetElevatorPassengerState(ElevatorPassengerState.GoingToWaiting);
                return;
            }
            return;
        }

        // Start Waiting
        if (IsGoingToWaitingForElevator) {
            SetElevatorPassengerState(ElevatorPassengerState.Waiting);
            return;
        }

        // Start riding
        if (IsGoingToRidingOnElevator) {
            if (CurrentPathElevator) {
                SetElevatorPassengerState(ElevatorPassengerState.Riding);
                return;
            }

            if (!CurrentPathElevator) {
                SetElevatorPassengerState(ElevatorPassengerState.None);
                return;
            }
            return;
        }

        // Go to ride
        if (IsWaitingForElevator && CurrentElevator.IsPossibleToEnter()) {
            SetElevatorPassengerState(ElevatorPassengerState.GoingToRiding);
            return;
        }

        if (IsRidingOnElevator && CurrentElevator.IsPossibleToExit()) {
            // Exit
            if (CurrentBuilding == CurrentPathBuilding/* && LastPathElevator && LastPathElevator.ConnectedWith(CurrentPathBuilding)*/) {
                SetElevatorPassengerState(ElevatorPassengerState.None);
                return;
            }

            if (!CurrentPathBuilding) {
                SetElevatorPassengerState(ElevatorPassengerState.None);
                return;
            }
            return;
        }
    }

    // Work
    public void SetWork(Building building = null)
    {
        if (workBuilding) {
            RemoveWork();
        }

        if (building) {
            workBuilding = building;
            workBuilding.AddWorker(this);
            SetWorkerIndex(workBuilding.workers.Count - 1);
        }
        Debug.Log("SetWork " + workBuilding);

        if (building) {
//            if (!workBuilding.constructionComponent.isUnderConstruction) {
//                this.workBuilding = workBuilding;
//                workBuilding.AddWorker(this);

//                if (currentBuilding == workBuilding)
//                    StartWorking();
//                else
//                    SetTargetBuilding(currentBuilding ? currentBuilding.buildingPlace : null, workBuilding);

//                OnWorkerAdd?.Invoke();
//            }
//            else {
//                int levelIndex = workBuilding.levelIndex;
//                List<ItemInstance> resourcesToBuild = workBuilding.ConstructionLevelsData[levelIndex].ResourcesToBuild;

//                for (int i = 0; i < resourcesToBuild.Count; i++) {
//                    if (workBuilding.constructionComponent.incomingConstructionResources.Count <= i || workBuilding.constructionComponent.incomingConstructionResources[i].Amount < resourcesToBuild[i].Amount) {
//                        if (SetTargetBuilding(workBuilding.buildingPlace, b => {
//                            if (!b || !b.storageComponent || b == workBuilding) return false;

//                            int itemIndex = workBuilding.ConstructionLevelsData[workBuilding.levelIndex].ResourcesToBuild[0].ItemData.ItemId;

//                            return b.storageComponent.storedItems.ContainsKey(itemIndex) && b.storageComponent.storedItems[itemIndex].Amount >= 0;
//                        }))
//{
//                            this.workBuilding = workBuilding;
//                            StartWorking();
//                        }
//                        break;
//                    }
//                }
//            }
        }

        OnWorkerAdd?.Invoke();
    }

    public void RemoveWork()
    {
        if (workBuilding) {
            StopWorking();
            workBuilding.RemoveWorker(this);
            workBuilding = null;
        }
        OnWorkerRemove?.Invoke();
    }

    public void SetWorkerIndex(int index)
    {
        workerIndex = index;
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
        if (workBuilding) {
            workBuilding.RemoveCurrentWorker(this);
        }
    }

    private void Working()
    {
        if (workBuilding) {
            if (!workBuilding.constructionComponent.isUnderConstruction) {
                if (workBuilding.constructionComponent.SpawnedConstruction.BuildingInteractions.Length > workerIndex) {
                    BuildingAction buildingAction = workBuilding.constructionComponent.SpawnedConstruction.BuildingInteractions[workerIndex];

                    if (buildingAction.actionTimes[currentActionIndex] > 0) {
                        currentActionTime += Time.deltaTime;
                        if (currentActionTime >= buildingAction.actionTimes[currentActionIndex]) {
                            if (currentActionIndex < buildingAction.actionTimes.Length - 1)
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

                if (CurrentBuilding == workBuilding) {
                    float distance = Vector3.Distance(transform.position, targetPosition);
                    if (distance < applyTargetPosition) {
                        currentActionTime += Time.deltaTime;
                        if (currentActionTime >= takeItemDuration) {
                            for (int i = 0; i < carriedItems.Count; i++) {
                                int itemId = carriedItems[i].ItemData.ItemId;
                                int amountToAdd = carriedItems[i].Amount;
                                int amountToSpend = CurrentBuilding.constructionComponent.AddConstructionResources(itemId, amountToAdd);
                                SpendItem(itemId, amountToSpend);

                                if ((deliveredResources.Count > i ? deliveredResources[i].Amount : 0) + (incomingResources.Count > i ? incomingResources[i].Amount : 0) < resourcesToBuild[i].Amount)
                                    isNeededToWork = true;
                            }

                            if (isNeededToWork)
                                SetTargetBuilding(b =>
                                {
                                    if (!b.storageComponent || (((TowerBuilding)b).floorIndex == ((TowerBuilding)workBuilding).floorIndex && ((TowerBuilding)b).placeIndex == ((TowerBuilding)workBuilding).placeIndex)) return false;

                                    int itemIndex = workBuilding.ConstructionLevelsData[workBuilding.levelIndex].ResourcesToBuild[0].ItemData.ItemId;

                                    return b.storageComponent.storedItems.ContainsKey(itemIndex) && b.storageComponent.storedItems[itemIndex].Amount >= 0;
                                });
                            //else
                            //SetWork(ResidentWork.None);
                            currentActionTime = 0;
                        }
                    }
                }
                else if (CurrentBuilding == TargetBuilding) {
                    float distance = Vector3.Distance(transform.position, targetPosition);
                    if (distance < applyTargetPosition) {
                        currentActionTime += Time.deltaTime;
                        if (currentActionTime >= takeItemDuration) {
                            for (int i = 0; i < resourcesToBuild.Count; i++) {
                                if ((deliveredResources.Count > i ? deliveredResources[i].Amount : 0) + (incomingResources.Count > i ? incomingResources[i].Amount : 0) < resourcesToBuild[i].Amount) {
                                    int itemId = resourcesToBuild[i].ItemData.ItemId;
                                    if (TargetBuilding.constructionComponent.incomingConstructionResourcesDict.ContainsKey(itemId))
                                        TargetBuilding.constructionComponent.incomingConstructionResourcesDict[itemId].SetAmount(0);

                                    int remainedAmount = resourcesToBuild[i].Amount - (deliveredResources.Count > i ? deliveredResources[i].Amount : 0) + (incomingResources.Count > i ? incomingResources[i].Amount : 0);
                                    int amountToTake = CurrentBuilding.storageComponent.SpendItem(itemId, math.min(currentMaxCarryWeight, remainedAmount));
                                    TakeItem(itemId, amountToTake);
                                    int amountToIncoming = carriedItemsDict[itemId].Amount;
                                    TargetBuilding.constructionComponent.AddIncomingConstructionResources(itemId, amountToIncoming);

                                    if ((deliveredResources.Count > i ? deliveredResources[i].Amount : 0) + (incomingResources.Count > i ? incomingResources[i].Amount : 0) < resourcesToBuild[i].Amount)
                                        isNeededToWork = true;
                                }
                            }

                            if (isNeededToWork)
                                SetTargetBuilding(b => b ? b == workBuilding : false);
                            //else
                            //SetWork(ResidentWork.None);
                            currentActionTime = 0;
                        }
                    }
                }
            }
        }
    }

    private void StartConstructingBuilding()
    {

    }

    // Movement
    public void Move(Vector3 direction, float speed)
    {
        transform.position += direction * speed;
    }

    public void MoveTo(Vector3 position)
    {
        isMoving = true;
        targetPosition = position;
        navMeshAgent.SetDestination(position);
    }

    public void StopMoving()
    {
        if (!isMoving) return;
        Debug.Log("StopMoving");

        isMoving = false;
        OnEntityStopped?.Invoke(this);
    }

    private void FollowPath()
    {
        if (IsRidingOnElevator) return;

        if (CurrentPathBuilding && CurrentBuilding == CurrentPathBuilding) {
            pathIndex++;
        }

        if (CurrentPathElevator) {
            if (CurrentElevator) {
                if (elevatorPassengerState == ElevatorPassengerState.None) {
                    MoveTo(CurrentElevator.GetInteractionTransform().position);
                    return;
                }

                if (IsGoingToWaitingForElevator && CurrentElevator.IsPossibleToEnter()) {
                    MoveTo(CurrentElevator.GetInteractionTransform().position);
                    return;
                }

                if (IsGoingToRidingOnElevator && CurrentElevator.IsPossibleToEnter()) {
                    MoveTo(CurrentElevator.GetCabinRidingTransform().position);
                    return;
                }
                return;
            }

            if (CurrentBuilding) {
                MoveTo(CurrentPathBuilding.GetInteractionTransform().position);
                return;
            }

            if (!CurrentBuilding) {
                MoveTo(CurrentPathBuilding.GetInteractionTransform().position);
                return;
            }
            return;
        }

        if (CurrentPathBuilding) {
            MoveTo(CurrentPathBuilding.GetInteractionTransform().position);
            return;
        }

        if (CurrentElevator) {
            if (!IsUsingElevator) {
                MoveTo(CurrentBuilding.GetInteractionTransform().position);
                return;
            }
            return;
        }
    }

    private void ResetPath()
    {
        pathIndex = 0;
        pathBuildings.Clear();
        if (navMeshAgent.enabled)
            navMeshAgent.ResetPath();
    }

    private bool CheckDistanceToTargetPosition()
    {
        return isMoving && navMeshAgent.enabled && !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f);
    }

    // Buildings
    public virtual void EnterBuilding(Building building)
    {
        if (building == null) {
            Debug.LogWarning("building is NULL");
            return;
        }
        if (building == CurrentBuilding) {
            Debug.LogWarning("building is a currentBuilding already");
            return;
        }

        CurrentBuilding = building;
        building.EnterBuilding(this);
    }

    public virtual void ExitBuilding()
    {

    }

    public Building SetTargetBuilding(Building targetBuilding)
    {
        Debug.Log("SetTargetBuilding");
        if (!gameManager) {
            Debug.LogError("cityManager is NULL");
            return null;
        }

        Building startBuilding = GetPathStartBuilding();
        BuildingPlace startBuildingPlace = startBuilding ? startBuilding.buildingPlace : null;
        Building target = gameManager.TryGetPathToBuilding(startBuildingPlace, targetBuilding, ref pathBuildings);
        if (target && IsRidingOnElevator) {
            if (pathBuildings.Count == 1)
                pathBuildings.Insert(0, CurrentElevator);
            else
                pathBuildings.RemoveAt(0);
        }
        return target;
    }

    public Building SetTargetBuilding(Func<Building, bool> targetBuildingCondition)
    {
        if (!gameManager) {
            Debug.LogError("cityManager is NULL");
            return null;
        }

        Building startBuilding = GetPathStartBuilding();
        BuildingPlace startBuildingPlace = startBuilding ? startBuilding.buildingPlace : null;
        Building target = gameManager.TryGetPathToBuilding(startBuildingPlace, targetBuildingCondition, ref pathBuildings);
        if (target && IsRidingOnElevator) {
            if (pathBuildings.Count == 1)
                pathBuildings.Insert(0, CurrentElevator);
            else
                pathBuildings.RemoveAt(0);
        }
        return target;
    }

    private Building GetPathStartBuilding()
    {
        Building startBuilding = null;
        if (IsRidingOnElevator) {
            int floorIndex = CurrentElevator.spawnedElevatorCabin.startFloorIndex;
            startBuilding = gameManager.builtFloors[floorIndex].roomBuildingPlaces[buildingIndex].placedBuilding;
        }
        else {
            startBuilding = CurrentBuilding && CurrentBuilding.buildingPlace ? CurrentBuilding.buildingPlace.placedBuilding : null;
        }
        //startBuilding = CurrentBuilding && CurrentBuilding.buildingPlace ? CurrentBuilding.buildingPlace.placedBuilding : null;
        return startBuilding;
    }

    private void OnBuildingStartConstructing(ConstructionComponent building)
    {
        //StartCoroutine(OnBuildingStartConstructingCoroutine(building));
    }

    private IEnumerator OnBuildingStartConstructingCoroutine(ConstructionComponent construction)
    {
        yield return gameManager.isBakingNavMesh;

        if (!workBuilding) {
            Building building = construction.GetComponent<Building>();
            SetWork(building);
        }
        else {
            SetTargetBuilding(b => ((TowerBuilding)b).floorIndex == ((TowerBuilding)TargetBuilding).floorIndex && ((TowerBuilding)b).placeIndex == ((TowerBuilding)TargetBuilding).placeIndex);
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
        Debug.Log(state);

        // Exit old state
        switch (elevatorPassengerState) {
            case ElevatorPassengerState.GoingToWaiting:
                break;
            case ElevatorPassengerState.GoingToRiding:
                break;
            case ElevatorPassengerState.Riding:
                navMeshAgent.enabled = true;
                break;
        }
        CurrentElevator.RemovePassenger(this);

        // Enter new state
        elevatorPassengerState = state;
        CurrentElevator.AddPassenger(this);
        switch (elevatorPassengerState) {
            case ElevatorPassengerState.GoingToWaiting:
                break;
            case ElevatorPassengerState.GoingToRiding:
                break;
            case ElevatorPassengerState.Riding:
                navMeshAgent.enabled = false;
                break;
        }

    }

    public void OnElevatorPlatformStopped(ElevatorPlatformConstruction cabin)
    {
        DecideAction();
    }

    public void OnElevatorCabinChangedFloor(ElevatorPlatformConstruction cabin)
    {
        if (IsRidingOnElevator && CurrentElevator && CurrentElevator.spawnedElevatorCabin == cabin) {
            EnterBuilding(cabin.ownedElevator);
        }
    }

    // Boat
    private void EnterBoat()
    {
        PierBuilding workPier = workBuilding as PierBuilding;
        if (workPier)
        {
            PierBuilding pier = workBuilding as PierBuilding;
            Boat boat = gameManager.GetBoatByIndex(workerIndex);
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
            ItemInstance item = new ItemInstance(gameManager.lootList.loot[itemId]); // The same item instance for list and dictionary.
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
