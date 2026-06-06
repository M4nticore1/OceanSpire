using UnityEngine;
using UnityEngine.AI;

public static class DriftingLootFinder
{
    public static SwimmingDriftingLoot TryFindNearestSwimmingDriftingLoot(DriftingLootManager driftingLootManager, Vector3 position)
    {
        var driftingLoot = driftingLootManager.SpawnedSwimmingDriftingLoot;

        if (driftingLoot.Count == 0) return null;

        SwimmingDriftingLoot nearestContainer = null;
        float shortestPathDistance = float.MaxValue;

        foreach (var loot in driftingLoot) {
            if (!loot) continue;

            Vector3 lootPosition = loot.transform.position;
            var path = new NavMeshPath();

            if (NavMesh.CalculatePath(position, lootPosition, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete) {
                float pathDistance = GetPathLength(path);

                if (pathDistance < shortestPathDistance) {
                    shortestPathDistance = pathDistance;
                    nearestContainer = loot;
                }
            }
        }

        return nearestContainer;
    }

    private static float GetPathLength(NavMeshPath path)
    {
        if (path.corners.Length < 2) return 0;

        float length = 0;
        for (int i = 1; i < path.corners.Length; i++) {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }

        return length;
    }
}