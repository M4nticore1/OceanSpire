using UnityEngine;
using UnityEngine.AI;

public class BoatFindingLootState : BoatState
{
    private const double updateDestinationRate = 0.5f;
    private double lastUpdateDestinationTime = 0;

    public LootContainer currentTarget { get; private set; } = null;
    public bool isCollectingLoot { get; private set; } = false;

    public BoatFindingLootState(Boat boat) : base(boat)
    {

    }

    public override void Enter()
    {

    }

    public override void Exit()
    {

    }

    public override void Process()
    {
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
        int count = LootManager.Instance.spawnedLootContainers.Count;

        if (count == 0) return null;

        LootContainer nearestContainer = null;

        foreach (var container in LootManager.Instance.spawnedLootContainers) {
            if (!container || container.currentTransportMethod == TransportMethod.Flying) continue;

            Vector3 position = container.transform.position;

            if (nearestContainer && position.magnitude >= nearestContainer.transform.position.magnitude) continue;

            NavMeshPath path = new NavMeshPath();

            if (NavMesh.CalculatePath(boat.transform.position, position, NavMesh.AllAreas, path)) {
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