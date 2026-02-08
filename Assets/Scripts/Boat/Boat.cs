using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class BoatEntry
{
    public int dockIndex = 0;
    public float health = 0;
    public Vector3 position;
    public Vector3 rotation;
}

public class Boat : MonoBehaviour
{
    [SerializeField] private BoatData boatData = null;
    public BoatData BoatData => boatData;

    private NavMeshAgent navAgent = null;
    private EntityMovement movement = null;
    public HealthComponent healthComponent { get; private set; } = null;

    private bool hasRider = false;

    [SerializeField] private Transform seatSlot = null;
    public Transform SeatSlot => seatSlot;

    public bool isDocked { get; private set; } = true;
    public bool isReturningToDock { get; private set; } = false;
    public bool isCollectingLoot { get; private set; } = false;

    private BoatDockPoint boatDock = null;
    public BoatDockPoint BoatDock => boatDock;

    private const double updateDestinationRate = 1f;
    private double lastUpdateDestinationTime = 0;

    private Transform currentTargetTransform = null;

    [SerializeField] private List<ItemInstance> storedLoot = new List<ItemInstance>();
    private Dictionary<int, ItemInstance> storedLootDict = new Dictionary<int, ItemInstance>();
    public float currentWeight { get; private set; } = 0;
    private float currentWeightToUnload = 0f;

    private const float distanceToChangeLootTarget = 10.0f;
    private bool isInitialized = false;

    private double lastDrainHealthTime = 0d;
    public bool isDemolished { get; private set; } = false;

    public ContextMenuUI spawnedDetailsMenu { get; set; } = null;

    private TimerHandle collectLootTimer = new TimerHandle();

    public static event Action<Boat> OnBoadDestroyed;
    public static event Action<Boat> onBoatDocked;

    private void OnEnable()
    {
        LootContainer.onLootEnteredToArea += OnLootEnteredToArea;
        LootContainer.OnLootExitedFromArea += OnLootExitedFromArea;
    }

    private void OnDisable()
    {
        LootContainer.onLootEnteredToArea -= OnLootEnteredToArea;
        LootContainer.OnLootExitedFromArea -= OnLootExitedFromArea;
    }

    private void Update()
    {
        if (!isInitialized) return;
        if (isDemolished) return;

        if (isDocked) {
            Mooring();
            StoringResources();
        }
        else {
            DrainingHealth();
            CheckPosition();

            if (!isReturningToDock && !isCollectingLoot)
                UpdateDestination();

            if (currentTargetTransform || isReturningToDock) {
                if (currentTargetTransform) {
                    Debug.Log("currentTarget");
                    UpdateDestination();
                }

                //float distance = math.distance(transform.position, currentTargetPosition);
                if (navAgent.hasPath && navAgent.remainingDistance <= navAgent.stoppingDistance) {
                    if (isReturningToDock) {
                        Dock();
                    }
                    else if (currentTargetTransform) {
                        LootContainer loot = currentTargetTransform.GetComponent<LootContainer>();
                        if (loot && !isCollectingLoot)
                            StartCollectingLoot();
                    }
                }
            }
        }
    }

    public void Init(BoatEntry data)
    {
        GetComponents();

        transform.position = data.position;
        transform.rotation = Quaternion.Euler(data.rotation);

        healthComponent.Init(BoatData.MaxHealth, data.health);
        PierConstruction pierConstruction = CityManager.Instance.PierBuilding.spawnedConstruction as PierConstruction;
        boatDock = pierConstruction.BoatDocks[data.dockIndex];

        isInitialized = true;
    }

    private void GetComponents()
    {
        navAgent = GetComponent<NavMeshAgent>();
        movement = GetComponent<EntityMovement>();
        healthComponent = GetComponent<HealthComponent>();
    }

    // Docking
    private void Mooring()
    {
        if (transform.rotation == BoatDock.DockTransform.rotation) return;

        transform.rotation = Quaternion.Lerp(transform.rotation, BoatDock.DockTransform.rotation, BoatData.correctDockRotationSpeed * Time.deltaTime);
    }

    private void StoringResources()
    {
        if (currentWeight <= 0) return;

        currentWeightToUnload += BoatData.unloadLootSpeed * Time.deltaTime;
        StorageBuildingModule storageComponent = CityManager.Instance.GetComponent<StorageBuildingModule>();
        StorageModuleLevelData storageLevelData = storageComponent.StorageLevelData;
        ItemInstance loot = storedLoot[0];
        int lootId = loot.ItemData.ItemId;

        if (currentWeightToUnload < loot.ItemData.Weight) return;

        int maxAmountToUnload = (int)(currentWeightToUnload / loot.ItemData.Weight);
        int minAmountToUnload = math.min(maxAmountToUnload, loot.Amount);
        int amountToUnload = math.min(minAmountToUnload, ItemsList.Instance.GetItem(lootId, storageLevelData.storageItems).Amount);
        int weightToUnload = amountToUnload * loot.ItemData.Weight;

        storedLootDict[lootId].RemoveAmount(amountToUnload);
        storageComponent.storedItems[lootId].AddAmount(amountToUnload);
        currentWeight -= weightToUnload;
        currentWeightToUnload -= weightToUnload;
    }

    // Health
    private void DrainingHealth()
    {
        if (Time.timeAsDouble >= lastDrainHealthTime + BoatData.healthDrainInterval) {
            DrainHealth();
        }
    }

    private void DrainHealth()
    {
        healthComponent.RemoveHealth(1f);
        lastDrainHealthTime = Time.timeAsDouble;
    }

    public void StartMovingToDock()
    {
        Debug.Log("ReturnToDock");
        isReturningToDock = true;
        movement.MoveTo(BoatDock.DockTransform.position);
    }

    private void SetTarget(Transform target)
    {
        if (!movement.MoveTo(target.position)) return;

        currentTargetTransform = target;
    }

    private void Dock()
    {
        isDocked = true;
        isReturningToDock = false;
        onBoatDocked?.Invoke(this);
    }

    // Collect Loot
    private void CheckPosition()
    {
        if (navAgent.isOnNavMesh && navAgent.remainingDistance <= navAgent.stoppingDistance) {
            if (isReturningToDock)
                Dock();
            else if (currentTargetTransform)
                StartCollectingLoot();
        }
    }

    private void StartCollectingLoot()
    {
        movement.StopMoving();
        float remainingWeight = BoatData.MaxWeight - currentWeight;

        LootContainer loot = currentTargetTransform.GetComponent<LootContainer>();
        loot.StartCollecting(remainingWeight);
        TimerManager.StartTimer(collectLootTimer, boatData.LootCollectTime, CollectLoot);

        isCollectingLoot = true;
    }

    private void CollectLoot()
    {
        float remainingWeight = BoatData.MaxWeight - currentWeight;
        LootContainer loot = currentTargetTransform.GetComponent<LootContainer>();
        List<ItemInstance> collectedLoot = loot.TakeItems(remainingWeight);
        for (int i = 0; i < collectedLoot.Count; i++)
        {
            ItemInstance currentLoot = collectedLoot[i];
            ItemData data = currentLoot.ItemData;
            int id = currentLoot.ItemData.ItemId;
            //int amount = (int)math.min(currentLoot.Amount, remainingWeight / data.Weight);
            int weight = currentLoot.Amount * currentLoot.ItemData.Weight;

            if (!storedLootDict.ContainsKey(id))
            {
                storedLootDict.Add(id, currentLoot);
                storedLoot.Add(currentLoot);
            }
            else
            {
                storedLootDict[id].AddAmount(currentLoot.Amount);
            }

            currentWeight += weight;

            if (spawnedDetailsMenu)
                spawnedDetailsMenu.SetBoatCurrentWeight(currentWeight, BoatData.MaxWeight);
        }

        isCollectingLoot = false;

        if (currentWeight >= BoatData.MaxWeight)
            StartMovingToDock();
        else
            UpdateDestination();
    }

    public void EnterBoat()
    {
        if (TryFindNearestTarget(out currentTargetTransform)) {
            movement.MoveTo(currentTargetTransform.position);
        }
        lastDrainHealthTime = Time.timeAsDouble;
        isDocked = false;
    }

    public void ExitBoat()
    {
        movement.SetAgentEnabled(true);
        isDocked = true;
    }

    private void Demolish(bool isFXDemolish = true)
    {
        Destroy(gameObject);
    }

    private void OnLootEnteredToArea(LootContainer loot)
    {
        if (isDocked) return;
        if (isReturningToDock) return;

        if (currentTargetTransform)
        {
            float oldDistance = math.distance(transform.position, currentTargetTransform.position);
            float newDistance = math.distance(transform.position, loot.transform.position);
            if (newDistance <= distanceToChangeLootTarget && newDistance < oldDistance)
                SetTarget(loot.transform);
        }
        else
        {
            SetTarget(loot.transform);
        }
    }

    private void OnLootExitedFromArea(LootContainer loot)
    {
        if (isDocked) return;
        if (isReturningToDock) return;

        LootContainer currentLoot = currentTargetTransform ? currentTargetTransform.GetComponent<LootContainer>() : null;
        if (loot != currentLoot) return;

        TryFindNearestTarget(out currentTargetTransform);
        if (currentTargetTransform) {
            movement.MoveTo(currentTargetTransform.position);
        }
        else {
            movement.StopMoving();
        }
    }

    private bool TryFindNearestTarget(out Transform target)
    {
        int count = LootManager.Instance.spawnedLootContainers.Count;
        if (count == 0) {
            target = null;
            return false;
        }

        foreach (LootContainer loot in LootManager.Instance.spawnedLootContainers)
        {
            if (!loot || loot.currentTransportMethod == TransportMethod.Flying) continue;

            Transform transform = loot.GetComponent<Transform>();
            Vector3 position = transform.position;

            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(transform.position, position, NavMesh.AllAreas, path)) {
                target = transform;
                return true;
            }
        }

        target = null;
        return false;
    }

    private void UpdateDestination()
    {
        if (Time.timeAsDouble >= lastUpdateDestinationTime + updateDestinationRate)
        {
            if (TryFindNearestTarget(out currentTargetTransform)) {
                movement.MoveTo(currentTargetTransform.position);
            }
            lastUpdateDestinationTime = Time.timeAsDouble;
        }
    }
}
