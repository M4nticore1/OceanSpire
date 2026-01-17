using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class Boat : MonoBehaviour, IDamageable, ISelectable
{
    public PierBuilding ownedPier { get; private set; } = null;
    private NavMeshAgent navAgent = null;
    public ConstructionComponent constructionComponent { get; private set; } = null;
    private SelectComponent selectComponent = null;

    // Damageable
    private float currentHealth = 0;
    public float CurrentHealth { get { return currentHealth; } }
    [SerializeField] private float maxHealth = 0;
    public float MaxHealth { get { return maxHealth; } }

    // Seletable
    private bool isSelected = false;
    public bool IsSelected { get { return isSelected; } set { isSelected = value; } }

    [SerializeField] private BoatData boatData = null;
    public BoatData BoatData => boatData;

    [SerializeField] private Transform seatSlot = null;
    public Transform SeatSlot => seatSlot;

    private Creature rider = null;

    private const double updateDestinationRate = 1f;
    private double lastUpdateDestinationTime = 0;

    private Transform currentTarget = null;
    private Vector3 currentTargetPosition = Vector3.zero;

    public int dockIndex { get; private set; } = 0;
    public bool isFloating { get; private set; } = false;
    public bool isReturningToDock { get; private set; } = false;
    public bool isCollectingLoot { get; private set; } = false;

    [SerializeField] private List<ItemInstance> storedLoot = new List<ItemInstance>();
    private Dictionary<int, ItemInstance> storedLootDict = new Dictionary<int, ItemInstance>();
    public float currentWeight { get; private set; } = 0;
    private float lastMaxWeight = 0;
    private float currentWeightToUnload = 0f;

    private const float distanceToChangeLootTarget = 10.0f;
    private bool isInitialized = false;

    private double lastDrainHealthTime = 0d;
    public bool isDemolished { get; private set; } = false;

    public ContextMenu spawnedDetailsMenu { get; set; } = null;
    [SerializeField] private StatsWorldUI statsWorldWidget = null;

    private TimerHandle collectLootTimer = new TimerHandle();

    public static event System.Action<Boat> OnBoadDestroyed;
    public static event System.Action<Boat> onBoatDocked;

    private void OnEnable()
    {
        LootContainer.OnLootEntered += OnLootEntered;
        LootContainer.OnLootExited += OnLootExited;
    }

    private void OnDisable()
    {
        LootContainer.OnLootEntered -= OnLootEntered;
        LootContainer.OnLootExited -= OnLootExited;
    }

    public void Initialize(bool isUnderConstruction, int dockIndex, bool isFloating = false, bool isReturningToDock = false, float? health = null)
    {
        navAgent = GetComponent<NavMeshAgent>();
        constructionComponent = GetComponent<ConstructionComponent>();

        lastUpdateDestinationTime = Time.timeAsDouble - updateDestinationRate;

        this.ownedPier = ownedPier;
        this.dockIndex = dockIndex;

        if (!isFloating)
            ToDock();
        else if (isReturningToDock)
            ReturnToDock();

        if (health != null)
            currentHealth = health.Value;
        else
            currentHealth = boatData.MaxHealth;

        if (statsWorldWidget)
            statsWorldWidget.Initialize(currentHealth, BoatData.MaxHealth, BoatData.healthDisplayThreshold);

        constructionComponent.InitializeConstruction(isUnderConstruction);
        isInitialized = true;
    }

    private void Update()
    {
        Tick();
    }

    public void Tick()
    {
        if (!isDemolished)
        {
            if (isInitialized && rider)
            {
                if (isFloating)
                {
                    if (Time.timeAsDouble >= lastDrainHealthTime + BoatData.healthDrainInterval)
                    {
                        TakeDamage(1f);
                        lastDrainHealthTime = Time.timeAsDouble;
                    }

                    if (isCollectingLoot)
                    {
                        if (statsWorldWidget)
                            statsWorldWidget.SetActionProgressFillAmount(collectLootTimer.alpha);
                    }
                    else if (currentTarget || isReturningToDock)
                    {
                        if (currentTarget)
                        {
                            Debug.Log("currentTarget");
                            UpdateDestination();
                        }

                        //float distance = math.distance(transform.position, currentTargetPosition);
                        if (navAgent.hasPath && navAgent.remainingDistance <= navAgent.stoppingDistance)
                        {
                            if (isReturningToDock)
                            {
                                ToDock();
                            }
                            else if (currentTarget)
                            {
                                LootContainer loot = currentTarget.GetComponent<LootContainer>();
                                if (loot && !isCollectingLoot)
                                    StartCollectingLoot(loot);
                            }
                        }
                    }
                    else
                    {
                        //UpdateDestination();
                    }
                }
                else
                {
                    if (transform.rotation != GetOwnedDockTransform().rotation)
                        transform.rotation = Quaternion.Lerp(transform.rotation, GetOwnedDockTransform().rotation, BoatData.correctDockRotationSpeed * Time.deltaTime);

                    if (currentWeight > 0)
                    {
                        currentWeightToUnload += BoatData.unloadLootSpeed * Time.deltaTime;
                        StorageBuildingComponent storageComponent = ownedPier.storageComponent;
                        StorageBuildingLevelData storageLevelData = storageComponent.StorageLevelData;
                        ItemInstance loot = storedLoot[0];
                        int lootId = loot.ItemData.ItemId;

                        if (currentWeightToUnload < loot.ItemData.Weight) return;

                        int maxAmountToUnload = (int)(currentWeightToUnload / loot.ItemData.Weight);
                        int minAmountToUnload = math.min(maxAmountToUnload, loot.Amount);
                        int amountToUnload = math.min(minAmountToUnload, GameManager.Instance.lootList.GetItem(lootId, storageLevelData.storageItems).Amount);
                        int weightToUnload = amountToUnload * loot.ItemData.Weight;

                        storedLootDict[lootId].SubtractAmount(amountToUnload);
                        storageComponent.storedItems[lootId].AddAmount(amountToUnload);
                        currentWeight -= weightToUnload;
                        currentWeightToUnload -= weightToUnload;

                        if (spawnedDetailsMenu)
                            spawnedDetailsMenu.SetBoatCurrentWeight(currentWeight, BoatData.MaxWeight);
                        if (statsWorldWidget)
                        {
                            float unloadAlpha = 1f - (currentWeight / lastMaxWeight);
                            statsWorldWidget.SetActionProgressFillAmount(unloadAlpha);
                        }
                    }
                    else
                    {
                        statsWorldWidget.HideActionProgressBar();
                        FindNearestLootTarget();
                    }
                }
            }
        }
    }

    // Health
    public void TakeDamage(float value)
    {
        currentHealth -= value;
        if (CurrentHealth <= 0) {
            Demolish();
        }

        if (statsWorldWidget) {
            if (currentHealth <= BoatData.MaxHealth * BoatData.healthDisplayThreshold) {
                if (!statsWorldWidget.isHealthBarShowed)
                    statsWorldWidget.ShowHealthBar();
                statsWorldWidget.SetHealthBarAlpha(currentHealth / BoatData.MaxHealth);
            }
            else {
                if (statsWorldWidget.isHealthBarShowed)
                    statsWorldWidget.HideHealthBar();
            }
        }
    }

    public void Heal(float value)
    {
        float valueToHeal = math.clamp(value, 0, MaxHealth - CurrentHealth);
        currentHealth += valueToHeal;
    }

    private void Demolish()
    {

    }

    // Select
    public void Select()
    {
        isSelected = true;
        foreach (GameObject child in GameUtils.GetAllChildren(transform)) {
            child.layer = LayerMask.NameToLayer("Outlined");
        }
    }

    public void Deselect()
    {
        isSelected = false;
        foreach (GameObject child in GameUtils.GetAllChildren(transform)) {
            child.layer = LayerMask.NameToLayer("Default");
        }
    }

    public void ReturnToDock()
    {
        Debug.Log("ReturnToDock");
        isReturningToDock = true;
        SetTargetPosition(GetOwnedDockTransform().position);
    }

    private void SetTarget(Transform target)
    {
        if (!target || !SetTargetPosition(target.position)) return;
        currentTarget = target;
    }

    private bool SetTargetPosition(Vector3 position)
    {
        if (!navAgent || !navAgent.SetDestination(position)) return false;

        Debug.Log("SetTargetPosition");
        currentTargetPosition = position;
        return true;
    }

    private void ToDock()
    {
        isFloating = false;
        isReturningToDock = false;
        StopMoving();
        onBoatDocked?.Invoke(this);

        if (currentWeight > 0)
            statsWorldWidget.ShowActionProgressBar();
    }

    private void StartCollectingLoot(LootContainer loot)
    {
        StopMoving();
        float remainingWeight = BoatData.MaxWeight - currentWeight;
        loot.StartCollecting(remainingWeight);
        TimerManager.StartTimer(collectLootTimer, boatData.LootCollectTime, () => CollectLoot(loot));

        if (statsWorldWidget)
            statsWorldWidget.ShowActionProgressBar();

        isCollectingLoot = true;
    }

    private void CollectLoot(LootContainer loot)
    {
        float remainingWeight = BoatData.MaxWeight - currentWeight;
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
            lastMaxWeight = currentWeight;

            if (spawnedDetailsMenu)
                spawnedDetailsMenu.SetBoatCurrentWeight(currentWeight, BoatData.MaxWeight);
        }

        //Transform lastTarget = currentTarget;
        //currentTarget = null;
        isCollectingLoot = false;

        if (statsWorldWidget)
            statsWorldWidget.HideActionProgressBar();

        if (currentWeight >= BoatData.MaxWeight)
            ReturnToDock();
        else
            StartCoroutine(FindNearestLootTargetCoroutine());
    }

    private void StopMoving()
    {
        currentTarget = null;
        currentTargetPosition = Vector3.zero;
    }

    public void EnterBoat(Creature entity)
    {
        rider = entity;
        navAgent.isStopped = false;
        isFloating = true;
        FindNearestLootTarget();
        lastDrainHealthTime = Time.timeAsDouble;
    }

    public void ExitBoat()
    {
        rider = null;
    }

    public void Demolish(bool isFXDemolish = true)
    {
        Destroy(gameObject);
    }

    private void OnLootEntered(LootContainer loot)
    {
        if (!isFloating || isReturningToDock) return;

        if (currentTarget)
        {
            float oldDistance = math.distance(transform.position, currentTarget.position);
            float newDistance = math.distance(transform.position, loot.transform.position);
            if (newDistance <= distanceToChangeLootTarget && newDistance < oldDistance)
                SetTarget(loot.transform);
        }
        else
        {
            SetTarget(loot.transform);
        }
    }

    private void OnLootExited(LootContainer loot)
    {
        LootContainer currentLoot = currentTarget ? currentTarget.GetComponent<LootContainer>() : null;
        if (currentLoot && currentLoot == loot)
        {
            StopMoving();
            FindNearestLootTarget();
            //currentTarget = null;
            //StopMoving();
        }
    }

    private void FindNearestLootTarget()
    {
        Debug.Log("FindNearestLootTarget");
        if (!isFloating || isReturningToDock) return;

        int count = LootManager.Instance.spawnedLootContainers.Count;
        if (count == 0) return;

        Transform nearestTarget = null;
        Vector3 nearestPosition = Vector3.zero;
        float minDistance = 0f;

        foreach (LootContainer loot in LootManager.Instance.spawnedLootContainers)
        {
            if (!loot || loot.currentTransportMethod == TransportMethod.Flying) continue;

            Transform target = loot.GetComponent<Transform>();
            Vector3 position = target.position;

            NavMeshPath path = new NavMeshPath();
            bool pathFound = NavMesh.CalculatePath(transform.position, position, NavMesh.AllAreas, path);
            if (!pathFound)
            {
                if (currentTarget && loot.gameObject == currentTarget.gameObject)
                    StopMoving();
                continue;
            }

            float distance = math.distance(transform.position, position);

            if (distance < minDistance || !nearestTarget)
            {
                minDistance = distance;
                nearestTarget = target;
            }
        }
        SetTarget(nearestTarget);
    }

    private IEnumerator FindNearestLootTargetCoroutine()
    {
        yield return new WaitUntil(() => currentTarget == null);
        FindNearestLootTarget();
    }

    private void UpdateDestination()
    {
        if (Time.timeAsDouble >= lastUpdateDestinationTime + updateDestinationRate)
        {
            FindNearestLootTarget();
            lastUpdateDestinationTime = Time.timeAsDouble;
        }
    }

    private Transform GetOwnedDockTransform()
    {
        PierConstruction pierConstruction = ownedPier.constructionComponent.SpawnedConstruction.GetComponent<PierConstruction>();
        return pierConstruction.BoatDockPositions[dockIndex];
    }
}
