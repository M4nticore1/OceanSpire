using UnityEngine;
using UnityEngine.AI;

public class BoatFindingLootState : BoatState
{
    private DriftingLootManager lootManager;

    private const double updateDestinationRate = 0.5f;
    private double lastUpdateDestinationTime = 0;

    public DriftingLoot currentTarget { get; private set; } = null;
    public bool isCollectingLoot { get; private set; } = false;

    public BoatFindingLootState(Boat boat) : base(boat)
    {
        lootManager = Object.FindAnyObjectByType<DriftingLootManager>();
    }

    public override void Enter()
    {

    }

    public override void Exit()
    {

    }

    public override void Tick()
    {
        if (Time.timeAsDouble < lastUpdateDestinationTime + updateDestinationRate) return;

        UpdateDestination();
        lastUpdateDestinationTime = Time.timeAsDouble;
    }

    public override void OnReachedPath()
    {
        
    }

    private void UpdateDestination()
    {
        var target = TryFindNearestLoot();
        if (!target) return;

        SetTarget(target);
    }

    private SwimmingDriftingLoot TryFindNearestLoot()
    {
        var driftingLoot = lootManager.SpawnedSwimmingDriftingLoot;

        if (driftingLoot.Count == 0) return null;

        SwimmingDriftingLoot nearestContainer = null;
        float shortestDistance = float.MaxValue;

        foreach (var loot in driftingLoot) {
            if (!loot) continue;

            var swimmingLoot = loot as SwimmingDriftingLoot;
            if (!swimmingLoot) continue;

            Vector3 position = loot.transform.position;

            float distance = Vector3.Distance(boat.transform.position, position);

            if (distance >= shortestDistance) continue;

            var path = new NavMeshPath();

            if (NavMesh.CalculatePath(boat.transform.position, position, NavMesh.AllAreas, path)) {
                shortestDistance = distance;
                nearestContainer = swimmingLoot;
            }
        }

        return nearestContainer;
    }

    private void SetTarget(SwimmingDriftingLoot driftingLoot)
    {
        boat.SetTargetLoot(driftingLoot);
        boat.SetState(BoatStateEnum.MovingToLoot);
    }
}