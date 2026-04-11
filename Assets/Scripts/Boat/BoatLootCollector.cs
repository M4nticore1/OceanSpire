using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class BoatLootHandler : MonoBehaviour
{
    private LootManager lootManager;

    [SerializeField] private Movement movement = null;
    [SerializeField] private Inventory inventory = null;

    [SerializeField] private int collectLootTime = 0;
    public const float unloadLootSpeed = 20.0f;

    private const double updateDestinationRate = 1f;
    private double lastUpdateDestinationTime = 0;

    public LootContainer currentTarget { get; private set; } = null;
    public bool isCollectingLoot { get; private set; } = false;

    private TimerHandle collectLootTimer = new TimerHandle();

    public event Action onCollectedLoot;

    public void Init()
    {
        lootManager = FindAnyObjectByType<LootManager>();
    }

    public void HandleCollectingLoot()
    {
        if (isCollectingLoot) return;

        ProcessUpdateDestination();
    }

    public void HandleDocking()
    {
        if (inventory.items.Count == 0) return;

        ProcessStoreResources();
    }

    public void OnReachedPath()
    {
        StartCollectingLoot();
    }

    private void ProcessStoreResources()
    {
        if (inventory.RemainingWeight <= 0) return;

        float currentWeightToUnload = unloadLootSpeed * Time.deltaTime;
        ItemInstance loot = inventory.items[0].item;
        int lootId = loot.ItemData.ItemId;
        float lootWeight = loot.ItemData.Weight;
        int amountToUnload = math.min((int)(currentWeightToUnload / lootWeight), loot.Amount);

        inventory.RemoveItemAmount(lootId, amountToUnload);
    }

    private void StartCollectingLoot()
    {
        currentTarget.StopMoving();
        TimerManager.StartTimer(collectLootTimer, collectLootTime, CollectLoot);
        isCollectingLoot = true;
    }

    private void CollectLoot()
    {
        float remainingWeight = inventory.MaxWeight - inventory.CurrentWeight;
        LootContainer lootContainer = currentTarget.GetComponent<LootContainer>();
        List<ItemInstance> collectedLoot = lootContainer.TakeItems(remainingWeight);

        foreach (var loot in collectedLoot) {
            if (inventory.RemainingWeight <= 0) break;

            ItemData data = loot.ItemData;
            int id = loot.ItemData.ItemId;
            int amountToTake = math.min(loot.Amount, (int)(inventory.RemainingWeight / loot.ItemData.Weight));

            inventory.AddItemAmount(id, amountToTake);
        }

        isCollectingLoot = false;
        onCollectedLoot?.Invoke();
    }

    private LootContainer TryFindNearestTarget()
    {
        int count = lootManager.spawnedLootContainers.Count;

        if (count == 0) return null;

        LootContainer nearestContainer = null;

        foreach (var container in lootManager.spawnedLootContainers) {
            if (!container || container.IsFlying) continue;

            Vector3 position = container.transform.position;

            if (nearestContainer && position.magnitude >= nearestContainer.transform.position.magnitude) continue;

            NavMeshPath path = new NavMeshPath();

            if (NavMesh.CalculatePath(transform.position, position, NavMesh.AllAreas, path)) {
                nearestContainer = container;
            }
        }

        return nearestContainer;
    }

    private void ProcessUpdateDestination()
    {
        if (Time.timeAsDouble < lastUpdateDestinationTime + updateDestinationRate) return;

        UpdateDestination();
        lastUpdateDestinationTime = Time.timeAsDouble;
    }

    private void UpdateDestination()
    {
        LootContainer target = TryFindNearestTarget();

        if (!target) return;

        SetTarget(target);
    }

    private void SetTarget(LootContainer target)
    {
        if (!movement.TryMoveTo(target.transform.position)) return;

        currentTarget = target;
    }
}