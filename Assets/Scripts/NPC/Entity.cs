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

public class Entity : MonoBehaviour
{
    protected CityManager cityManager = null;
    public LevelComponent levelComponent { get; private set; } = null;
    public SelectComponent selectComponent { get; private set; } = null;
    public NavMeshAgent navMeshAgent { get; private set; } = null;

    [SerializeField] protected int maxHealth = 100;
    public int MaxHealth => maxHealth;
    protected int currentHealth = 100;

    public Building currentBuilding = null;
    [HideInInspector] public Building targetBuilding { get; protected set; } = null;
    public List<Building> pathBuildings = new List<Building>();
    [HideInInspector] public int pathIndex = 0;
    [HideInInspector] public int currentFloorIndex { get; protected set; } = 0;
    [HideInInspector] public int currentBuildingPlaceIndex { get; protected set; } = 0;
    public Vector3 targetPosition = Vector3.zero;
    protected const float applyTargetPosition = 1.0f;

    protected bool isWalking = false;
    public bool isRidingOnElevator { get; protected set; } = false;
    public bool isWaitingForElevator { get; protected set; } = false;
    public bool isWalkingToElevator { get; protected set; } = false;

    [HideInInspector] public ResidentWork currentWork { get; protected set; } = ResidentWork.None;
    [HideInInspector] public bool isWorking { get; protected set; } = false;
    [HideInInspector] public int workerIndex { get; protected set; } = 0;
    [HideInInspector] public Building workBuilding { get; protected set; } = null;

    public List<ItemInstance> carriedItems = new List<ItemInstance>();
    public Dictionary<int, ItemInstance> carriedItemsDict = new Dictionary<int, ItemInstance>();

    protected double currentActionTime = 0.0f;
    protected int currentActionIndex = 0;
    private const float takeItemDuration = 1.0f;

    public string firstName = "";
    public string lastName = "";

    // Stats
    private const int ñarryWeight = 2000;

    public static event System.Action OnWorkerAdd;
    public static event System.Action OnWorkerRemove;

    protected virtual void Awake()
    {
        cityManager = FindAnyObjectByType<CityManager>();
        levelComponent = GetComponent<LevelComponent>();
        selectComponent = GetComponent<SelectComponent>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    protected virtual void OnEnable()
    {
        ConstructionComponent.onAnyConstructionStartConstructing += OnBuildingStartConstructing;
    }

    protected virtual void OnDisable()
    {
        ConstructionComponent.onAnyConstructionFinishConstructing -= OnBuildingStartConstructing;
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        if (isWorking)
        {
            Work();
        }
    }

    // Work
    private void Work()
    {
        if (currentWork == ResidentWork.BuildingWork)
        {
            if (workBuilding.constructionComponent.spawnedConstruction.buildingInteractions.Count > workerIndex)
            {
                BuildingAction buildingAction = workBuilding.constructionComponent.spawnedConstruction.buildingInteractions[workerIndex];

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

                        navMeshAgent.SetDestination(buildingAction.waypoints[currentActionIndex].position);
                    }
                }
            }
        }
        else if (currentWork == ResidentWork.ConstructingBuilding)
        {
            int levelIndex = workBuilding.levelComponent.levelIndex;
            List<ItemInstance> resourcesToBuild = workBuilding.constructionComponent.ConstructionLevelsData[levelIndex].ResourcesToBuild;
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
                                if (!b.storageComponent || (b.GetFloorIndex() == workBuilding.GetFloorIndex() && b.GetPlaceIndex() == workBuilding.GetPlaceIndex())) return false;

                                int itemIndex = workBuilding.ConstructionLevelsData[workBuilding.levelComponent.levelIndex].ResourcesToBuild[0].ItemData.ItemId;

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
                                int amountToTake = currentBuilding.storageComponent.SpendItem(itemId, math.min(ñarryWeight, remainedAmount));
                                TakeItem(itemId, amountToTake);
                                int amountToIncoming = carriedItemsDict[itemId].Amount;
                                targetBuilding.constructionComponent.AddIncomingConstructionResources(itemId, amountToIncoming);

                                if ((deliveredResources.Count > i ? deliveredResources[i].Amount : 0) + (incomingResources.Count > i ? incomingResources[i].Amount : 0) < resourcesToBuild[i].Amount)
                                    isNeededToWork = true;
                            }
                        }

                        if (isNeededToWork)
                            SetTargetBuilding(currentBuilding.buildingPlace, b => b.GetFloorIndex() == workBuilding.GetFloorIndex() && b.GetPlaceIndex() == workBuilding.GetPlaceIndex());
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
        StopWorking();

        if (targetBuilding)
            targetBuilding = null;

        OnWorkerRemove?.Invoke();
    }

    public void SetWorkerIndex(int index)
    {
        workerIndex = index;
    }

    public void SetWork(ResidentWork newWork, Building newWorkBuilding = null)
    {
        currentWork = newWork;

        if (newWorkBuilding)
        {
            if (newWork == ResidentWork.BuildingWork)
            {
                workBuilding = newWorkBuilding;
                newWorkBuilding.AddWorker(this);

                SetTargetBuilding(currentBuilding ? currentBuilding.buildingPlace : null, b => b.GetFloorIndex() == newWorkBuilding.GetFloorIndex() && b.GetPlaceIndex() == newWorkBuilding.GetPlaceIndex());

                OnWorkerAdd?.Invoke();
            }
            else if (newWork == ResidentWork.ConstructingBuilding)
            {
                int levelIndex = newWorkBuilding.levelComponent.levelIndex;
                List<ItemInstance> resourcesToBuild = newWorkBuilding.constructionComponent.ConstructionLevelsData[levelIndex].ResourcesToBuild;
                for (int i = 0; i < resourcesToBuild.Count; i++)
                {
                    if (newWorkBuilding.constructionComponent.incomingConstructionResources.Count <= i || newWorkBuilding.constructionComponent.incomingConstructionResources[i].Amount < resourcesToBuild[i].Amount)
                    {
                        if (SetTargetBuilding(newWorkBuilding.buildingPlace, b =>
                        {
                            if (!b.storageComponent || (b.GetFloorIndex() == newWorkBuilding.GetFloorIndex() && b.GetPlaceIndex() == newWorkBuilding.GetPlaceIndex())) return false;

                            int itemIndex = newWorkBuilding.constructionComponent.ConstructionLevelsData[newWorkBuilding.levelComponent.levelIndex].ResourcesToBuild[0].ItemData.ItemId;

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

    private void StartWorking()
    {
        isWorking = true;
    }

    private void StopWorking()
    {
        isWorking = false;
        navMeshAgent.ResetPath();
    }

    private void StartConstructingBuilding()
    {

    }

    // Movement
    private void FollowPath()
    {
        if (isWalkingToElevator)
            Debug.Log("isWalkingToElevator " + isWalkingToElevator);
        if (isWaitingForElevator)
            Debug.Log("isWaitingForElevator " + isWaitingForElevator);
        if (isRidingOnElevator)
            Debug.Log("isRidingOnElevator " + isRidingOnElevator);

        if (!isRidingOnElevator && pathIndex < pathBuildings.Count)
        {
            ElevatorBuilding currentElevatorBuilding = currentBuilding as ElevatorBuilding;
            Building currentPathBuilding = pathBuildings[pathIndex];
            ElevatorBuilding currentPathElevator = currentPathBuilding as ElevatorBuilding;

            Building nextPathBuilding = null;
            ElevatorBuilding nextPathElevator = null;
            if (pathIndex + 1 < pathBuildings.Count)
            {
                nextPathBuilding = pathBuildings[pathIndex + 1];
                nextPathElevator = nextPathBuilding as ElevatorBuilding;
            }

            if (currentPathBuilding)
            {
                if (currentPathBuilding == targetBuilding)
                {
                    if (currentWork != ResidentWork.None)
                    {
                        if (currentWork == ResidentWork.BuildingWork)
                        {
                            targetPosition = currentPathBuilding.GetInteractionPosition();
                        }
                        else if (currentWork == ResidentWork.ConstructingBuilding)
                        {
                            targetPosition = currentPathBuilding.GetPickupItemPointPosition();
                        }
                    }
                }
                else if (currentPathElevator)
                {
                    if (nextPathElevator)
                    {
                        if (isWalkingToElevator)
                            targetPosition = currentPathElevator.GetPlatformRidingPosition();
                        else
                            targetPosition = currentPathElevator.GetInteractionPosition();
                    }
                    else if (nextPathBuilding)
                    {
                        targetPosition = nextPathBuilding.GetInteractionPosition();
                    }
                }

                navMeshAgent.SetDestination(targetPosition);
            }
            //else
            //{
            //    if (currentElevatorBuilding)
            //    {
            //        if (currentPathElevator && currentPathElevator.GetPlaceIndex() == currentElevatorBuilding.GetPlaceIndex() && currentPathElevator.buildingData.buildingIdName == currentElevatorBuilding.buildingData.buildingIdName)
            //        {
            //            //navMeshAgent.SetDestination(currentPathElevatorBuilding.spawnedBuildingConstruction.buildingInteractions[currentPathElevatorBuilding.elevatorWaitingPassengers.Count].waypoints[0].transform.position);
            //        }
            //        else
            //        {
            //            if (currentPathElevator)
            //            {
            //                navMeshAgent.SetDestination(currentPathElevator.spawnedBuildingConstruction.buildingInteractions[currentPathElevator.elevatorWaitingPassengers.Count].waypoints[0].transform.position);
            //            }
            //            else if (currentPathBuilding)
            //            {
            //                navMeshAgent.SetDestination(currentPathBuilding.spawnedBuildingConstruction.transform.position);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        if (currentPathElevator)
            //        {
            //            navMeshAgent.SetDestination(currentPathElevator.GetInteractionPointPosition());
            //        }
            //        else if (currentPathBuilding)
            //        {
            //            navMeshAgent.SetDestination(currentPathBuilding.transform.position);
            //        }
            //    }
            //}
        }
    }

    public void SetFloorIndex(int newFloorIndex)
    {
        currentFloorIndex = newFloorIndex;
    }

    public void Move(Vector3 direction, float speed)
    {
        transform.position += direction * speed;
    }

    // Elevators
    public void StartElevatorWalking()
    {
        ElevatorBuilding elevatorBuilding = pathBuildings[pathIndex] as ElevatorBuilding;

        isWaitingForElevator = false;
        isWalkingToElevator = true;
        isRidingOnElevator = false;

        ElevatorPlatformConstruction elevatorPlatformConstruction = elevatorBuilding.spawnedElevatorPlatform;
        navMeshAgent.SetDestination(elevatorPlatformConstruction.buildingInteractions[elevatorPlatformConstruction.elevatorRidingPassengers.Count].waypoints[0].position);

        elevatorBuilding.AddWalkingPassenger(this);
        elevatorBuilding.RemoveWaitingPassenger(this);
        elevatorBuilding.RemoveRidingPassenger(this);
    }

    public void StopElevatorWalking()
    {
        ElevatorBuilding elevatorBuilding = pathBuildings[pathIndex] as ElevatorBuilding;
        elevatorBuilding.RemoveWalkingPassenger(this);
    }

    public void StartElevatorWaiting()
    {
        ElevatorBuilding elevatorBuilding = pathBuildings[pathIndex] as ElevatorBuilding;
        isWaitingForElevator = true;
        isWalkingToElevator = false;
        isRidingOnElevator = false;

        BuildingConstruction buildingConstruction = elevatorBuilding.constructionComponent.spawnedConstruction;
        navMeshAgent.SetDestination(buildingConstruction.buildingInteractions[elevatorBuilding.elevatorWaitingPassengers.Count].waypoints[0].position);

        elevatorBuilding.RemoveRidingPassenger(this);
        elevatorBuilding.AddWaitingPassenger(this);
        elevatorBuilding.RemoveWalkingPassenger(this);
    }

    public void StopElevatorWaiting()
    {
        ElevatorBuilding elevatorBuilding = pathBuildings[pathIndex] as ElevatorBuilding;
        isWaitingForElevator = false;

        elevatorBuilding.RemoveWaitingPassenger(this);
    }

    public void StartElevatorRiding()
    {
        ElevatorBuilding elevatorBuilding = pathBuildings[pathIndex] as ElevatorBuilding;
        isRidingOnElevator = true;
        isWalkingToElevator = false;
        isWaitingForElevator = false;

        navMeshAgent.enabled = false;

        elevatorBuilding.AddRidingPassenger(this);
        elevatorBuilding.RemoveWalkingPassenger(this);
        elevatorBuilding.RemoveWaitingPassenger(this);
    }

    public void StopElevatorRiding()
    {
        Building building = null;

        if (pathIndex > 0)
            building = pathBuildings[pathIndex - 1];
        else
            building = pathBuildings[pathIndex];

        ElevatorBuilding elevatorBuilding = building as ElevatorBuilding;
        isRidingOnElevator = false;

        navMeshAgent.enabled = true;
        elevatorBuilding.RemoveRidingPassenger(this);

        EnterBuilding(pathBuildings[pathIndex]);
    }

    // Buildings
    public virtual void EnterBuilding(Building building)
    {
        if (building)
        {
            if (!isRidingOnElevator)
            {
                currentBuilding = building;
                currentFloorIndex = building.GetFloorIndex();

                building.EnterBuilding(this);

                if (currentBuilding == targetBuilding)
                {
                    if (currentWork != ResidentWork.None)
                    {
                        StartWorking();
                    }
                }

                if (pathBuildings.Count > pathIndex && currentBuilding.GetFloorIndex() == pathBuildings[pathIndex].GetFloorIndex() && currentBuilding.GetPlaceIndex() == pathBuildings[pathIndex].GetPlaceIndex())
                {
                    FollowPath();
                    pathIndex++;
                }
            }
        }
        else
            Debug.LogWarning("building is NULL");
    }

    public virtual void ExitBuilding()
    {
        //currentBuilding = null;
    }

    public Building SetTargetBuilding(BuildingPlace startBuildingPlace, Func<Building, bool> targetBuildingCondition)
    {
        pathIndex = 0;

        if (cityManager)
        {
            targetBuilding = cityManager.FindPathToBuilding(startBuildingPlace, targetBuildingCondition, ref pathBuildings);

            if (targetBuilding)
            {
                if (startBuildingPlace == (currentBuilding ? currentBuilding.buildingPlace : null))
                {
                    FollowPath();
                    return targetBuilding;
                }
                else
                {
                    if (cityManager.FindPathToBuilding(currentBuilding ? currentBuilding.buildingPlace : null, b => b.GetFloorIndex() == targetBuilding.GetFloorIndex() && b.GetPlaceIndex() == targetBuilding.GetPlaceIndex(), ref pathBuildings))
                    {
                        FollowPath();
                        return targetBuilding;
                    }
                }
            }

            return null;
        }
        else
        {
            Debug.LogError("cityManager is NULL");
            return null;
        }
    }

    protected void OnBuildingStartConstructing(ConstructionComponent building)
    {
        StartCoroutine(OnBuildingStartConstructingCoroutine(building));
    }

    protected virtual IEnumerator OnBuildingStartConstructingCoroutine(ConstructionComponent construction)
    {
        yield return new WaitForEndOfFrame();

        if (currentWork == ResidentWork.None)
        {
            Building building = construction.GetComponent<Building>();
            SetWork(ResidentWork.ConstructingBuilding, building);
        }
        else
        {
            SetTargetBuilding(currentBuilding ? currentBuilding.buildingPlace : null, b => b.GetFloorIndex() == targetBuilding.GetFloorIndex() && b.GetPlaceIndex() == targetBuilding.GetPlaceIndex());
        }

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
            ItemInstance item = new ItemInstance(ItemDatabase.itemsById[itemId]); // The same item instance for list and dictionary.
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
            int levelIndex = building.levelComponent.levelIndex;
            List<ItemInstance> constructionResources = building.constructionComponent.ConstructionLevelsData[levelIndex].ResourcesToBuild;
            for (int j = 0; j < building.constructionComponent.ConstructionLevelsData[levelIndex].ResourcesToBuild.Count; j++)
            {
                if (item.ItemData.ItemId == building.constructionComponent.ConstructionLevelsData [levelIndex].ResourcesToBuild[j].ItemData.ItemId)
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
