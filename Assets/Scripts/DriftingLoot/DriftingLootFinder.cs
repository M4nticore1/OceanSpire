using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class DriftingLootFinder
{
    private struct LootDistancePair
    {
        public SwimmingDriftingLoot loot;
        public float sqrDistance;
    }

    private static readonly List<LootDistancePair> preFilteredLoot = new();

    public static SwimmingDriftingLoot TryFindNearestSwimmingDriftingLoot(DriftingLootManager driftingLootManager, Boat boat)
    {
        if (!driftingLootManager) return null;

        var driftingLootList = driftingLootManager.SpawnedSwimmingDriftingLoot;
        if (driftingLootList == null) return null;
        if (driftingLootList.Count == 0) return null;

        preFilteredLoot.Clear();

        for (int i = 0; i < driftingLootList.Count; i++) {
            var loot = driftingLootList[i];
            if (!loot) continue;
            if (!boat.ShouldSetTargetLoot(loot)) continue;

            float sqrDist = (loot.transform.position - boat.transform.position).sqrMagnitude;
            preFilteredLoot.Add(new LootDistancePair { loot = loot, sqrDistance = sqrDist });
        }

        if (preFilteredLoot.Count == 0) return null;

        preFilteredLoot.Sort((a, b) => a.sqrDistance.CompareTo(b.sqrDistance));

        SwimmingDriftingLoot nearestContainer = null;
        float shortestPathDistance = float.MaxValue;

        var path = new NavMeshPath();

        int checkLimit = Mathf.Min(preFilteredLoot.Count, 5);
        for (int i = 0; i < checkLimit; i++) {
            var loot = preFilteredLoot[i].loot;

            if (preFilteredLoot[i].sqrDistance >= shortestPathDistance * shortestPathDistance) continue;

            if (NavMesh.CalculatePath(boat.transform.position, loot.transform.position, NavMesh.AllAreas, path)) {
                if (path.status == NavMeshPathStatus.PathComplete) {
                    float pathDistance = GetPathLength(path);

                    if (pathDistance < shortestPathDistance) {
                        shortestPathDistance = pathDistance;
                        nearestContainer = loot;
                    }
                }
            }
        }

        preFilteredLoot.Clear();
        return nearestContainer;
    }

    private static float GetPathLength(NavMeshPath path)
    {
        var corners = path.corners;
        if (corners.Length < 2) return 0f;

        float length = 0f;
        for (int i = 1; i < corners.Length; i++) {
            length += Vector3.Distance(corners[i - 1], corners[i]);
        }

        return length;
    }
}