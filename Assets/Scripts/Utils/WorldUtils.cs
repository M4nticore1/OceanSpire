using UnityEngine;

public static class WorldUtils
{
    private const float spawnDistance = 140f;

    public static Vector3 GetRandomBorderPosition()
    {
        Vector3 dir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        dir.Normalize();
        Vector3 position = dir * spawnDistance;
        return position;
    }
}