using UnityEngine;

public static class DriftingLootFactory
{
    public static DriftingLoot CreateDriftingLoot(DriftingLoot prefab, DriftingLootData driftingLootData)
    {
        var position = driftingLootData.Position.Vector3();
        var rotation = Quaternion.Euler(driftingLootData.Rotation.Vector3());

        var driftingLoot = GameObject.Instantiate(prefab, position, rotation);
        driftingLoot.Init(driftingLootData);

        return driftingLoot;
    }
}