using UnityEngine;

public static class WorldUtils
{
    public const float SpawnDistance = 150f;

    public static Vector3 GetRandomBorderPosition()
    {
        var dir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        dir.Normalize();

        return GetBorderPosition(dir);
    }

    public static Vector3 GetBorderPosition(Vector3 dir)
    {
        return dir * SpawnDistance;
    }
}