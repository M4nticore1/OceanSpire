using System.Collections.Generic;
using UnityEngine;

public static class DriftingLootFinder
{
    private struct LootDistancePair
    {
        public SwimmingDriftingLoot loot;
        public float sqrDistance;
    }

    private static readonly List<LootDistancePair> preFilteredLoot = new();

    public static SwimmingDriftingLoot TryFindNearestSwimmingDriftingLoot( DriftingLootManager lootManager, Boat boat)
    {
        if (lootManager == null || boat == null)
            return null;

        var spawnedDriftingLoot = lootManager.SpawnedSwimmingDriftingLoot;
        if (spawnedDriftingLoot == null || spawnedDriftingLoot.Count == 0)
            return null;

        preFilteredLoot.Clear();

        for (int i = 0; i < spawnedDriftingLoot.Count; i++) {
            var loot = spawnedDriftingLoot[i];

            if (loot == null)
                continue;

            if (!boat.ShouldSetTargetLoot(loot))
                continue;

            var sqrDistance = (loot.transform.position - boat.transform.position).sqrMagnitude;
            if (!IsCloserThanTargetBoat(loot, boat, sqrDistance))
                continue;

            preFilteredLoot.Add(new LootDistancePair
            {
                loot = loot,
                sqrDistance = sqrDistance
            });
        }

        if (preFilteredLoot.Count == 0)
            return null;

        preFilteredLoot.Sort((a, b) => a.sqrDistance.CompareTo(b.sqrDistance));

        SwimmingDriftingLoot nearestLoot = null;
        var shortestPathDistance = float.MaxValue;

        var checkLimit = Mathf.Min(preFilteredLoot.Count, 5);
        for (int i = 0; i < checkLimit; i++) {
            var pair = preFilteredLoot[i];
            if (pair.sqrDistance >= shortestPathDistance * shortestPathDistance)
                continue;

            var loot = pair.loot;
            if (!boat.Movement.CanReachPosition(loot.transform.position))
                continue;

            var pathDistance = Vector3.Distance(boat.transform.position, loot.transform.position);
            if (pathDistance < shortestPathDistance) {
                shortestPathDistance = pathDistance;
                nearestLoot = loot;
            }
        }

        preFilteredLoot.Clear();
        return nearestLoot;
    }

    private static bool IsCloserThanTargetBoat(SwimmingDriftingLoot loot, Boat boat, float sqrDistance)
    {
        var targetBoat = loot.TargetBoat;
        if (targetBoat == null || targetBoat == boat)
            return true;

        var targetSqrDistance = (loot.transform.position - targetBoat.transform.position).sqrMagnitude;

        return sqrDistance < targetSqrDistance;
    }
}