//using System;
//using System.Collections.Generic;
//using Unity.Mathematics;
//using UnityEngine;
//using UnityEngine.AI;

//public class BoatLootHandler : MonoBehaviour
//{
//    private DriftingLootManager lootManager;

//    [SerializeField] private Movement movement = null;
//    [SerializeField] private Inventory inventory = null;

//    [SerializeField] private int collectLootTime = 0;
//    public const float unloadLootSpeed = 20.0f;

//    private const double updateDestinationRate = 1f;
//    private double lastUpdateDestinationTime = 0;

//    public DriftingLoot currentTarget { get; private set; } = null;
//    public bool isCollectingLoot { get; private set; } = false;

//    private TimerHandle collectLootTimer = new TimerHandle();

//    public event Action onCollectedLoot;

//    public void Init()
//    {
//        lootManager = FindAnyObjectByType<DriftingLootManager>();
//    }

//    public void HandleCollectingLoot()
//    {
//        if (isCollectingLoot) return;

//        ProcessUpdateDestination();
//    }

//    public void HandleDocking()
//    {
//        if (inventory.Items.Count == 0) return;

//        ProcessStoreResources();
//    }

//    public void OnReachedPath()
//    {
//        StartCollectingLoot();
//    }

//    private void ProcessStoreResources()
//    {
//        if (inventory.RemainingWeight <= 0) return;

//        float currentWeightToUnload = unloadLootSpeed * Time.deltaTime;
//        ItemInstance loot = inventory.GetItemByIndex(0);
//        int lootId = loot.Definition.ItemId;
//        float lootWeight = loot.Definition.Weight;
//        int amountToUnload = math.min((int)(currentWeightToUnload / lootWeight), loot.Amount);

//        inventory.RemoveItem(lootId, amountToUnload);
//    }

//    private void StartCollectingLoot()
//    {
//        currentTarget.StopMoving();
//        TimerManager.Instance.StartTimer(collectLootTimer, collectLootTime, CollectLoot);
//        isCollectingLoot = true;
//    }

//    private void CollectLoot()
//    {
//        float remainingWeight = inventory.WeightLimit - inventory.CurrentWeight;
//        DriftingLoot lootContainer = currentTarget.GetComponent<DriftingLoot>();
//        List<ItemInstance> collectedLoot = lootContainer.TakeItems(remainingWeight);

//        foreach (var loot in collectedLoot) {
//            if (inventory.RemainingWeight <= 0) break;

//            ItemDefinition data = loot.Definition;
//            int id = loot.Definition.ItemId;
//            int amountToTake = math.min(loot.Amount, (int)(inventory.RemainingWeight / loot.Definition.Weight));

//            inventory.AddItem(id, amountToTake);
//        }

//        isCollectingLoot = false;
//        onCollectedLoot?.Invoke();
//    }

//    private DriftingLoot TryFindNearestTarget()
//    {
//        int count = lootManager.spawnedLootContainers.Count;

//        if (count == 0) return null;

//        DriftingLoot nearestContainer = null;

//        foreach (var container in lootManager.spawnedLootContainers) {
//            if (!container || container.IsFlying) continue;

//            Vector3 position = container.transform.position;

//            if (nearestContainer && position.magnitude >= nearestContainer.transform.position.magnitude) continue;

//            NavMeshPath path = new NavMeshPath();

//            if (NavMesh.CalculatePath(transform.position, position, NavMesh.AllAreas, path)) {
//                nearestContainer = container;
//            }
//        }

//        return nearestContainer;
//    }

//    private void ProcessUpdateDestination()
//    {
//        if (Time.timeAsDouble < lastUpdateDestinationTime + updateDestinationRate) return;

//        UpdateDestination();
//        lastUpdateDestinationTime = Time.timeAsDouble;
//    }

//    private void UpdateDestination()
//    {
//        DriftingLoot target = TryFindNearestTarget();

//        if (!target) return;

//        SetTarget(target);
//    }

//    private void SetTarget(DriftingLoot target)
//    {
//        if (!movement.TryMoveTo(target.transform.position)) return;

//        currentTarget = target;
//    }
//}