using UnityEngine;
using UnityEngine.AI;

public class BoatFindingLootState : BoatState
{
    private LootManager lootManager;

    private const double updateDestinationRate = 0.5f;
    private double lastUpdateDestinationTime = 0;

    public LootContainer currentTarget { get; private set; } = null;
    public bool isCollectingLoot { get; private set; } = false;

    public BoatFindingLootState(Boat boat) : base(boat)
    {
        lootManager = Object.FindAnyObjectByType<LootManager>();
    }

    public override void Enter()
    {

    }

    public override void Exit()
    {

    }

    public override void Process()
    {
        boat.ProcessDrainHealth();

        if (Time.timeAsDouble < lastUpdateDestinationTime + updateDestinationRate) return;

        UpdateDestination();
        lastUpdateDestinationTime = Time.timeAsDouble;
    }

    public override void HandleReachedPath()
    {
        
    }

    // Detecting Loot
    private void UpdateDestination()
    {
        LootContainer target = TryFindNearestTarget();

        if (!target) return;

        SetTarget(target);
    }

    private LootContainer TryFindNearestTarget()
    {
        var containers = lootManager.spawnedLootContainers;

        if (containers.Count == 0) return null;

        LootContainer nearestContainer = null;
        float shortestDistance = float.MaxValue;

        foreach (var container in containers) {
            if (!container || container.currentTransportMethod == TransportMethod.Flying) continue;

            Vector3 position = container.transform.position;

            float distance = Vector3.Distance(boat.transform.position, position);

            if (distance >= shortestDistance) continue;

            NavMeshPath path = new NavMeshPath();

            if (NavMesh.CalculatePath(boat.transform.position, position, NavMesh.AllAreas, path)) {
                shortestDistance = distance;
                nearestContainer = container;
            }
        }

        return nearestContainer;
    }

    private void SetTarget(LootContainer target)
    {
        boat.SetTargetLoot(target);
        boat.SetState(BoatStateEnum.MovingToLoot);
    }
}