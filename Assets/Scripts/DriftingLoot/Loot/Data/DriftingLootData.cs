using System;
using UnityEngine;

[Serializable]
public abstract class DriftingLootData
{
    public int Id = 0;
    public int InstanceId = 0;
    public Vector3Data Position;
    public Vector3Data Rotation;
    public int MeshId = 0;

    public static DriftingLootData Create(DriftingLoot driftingLoot)
    {
        return null;
    }

    public static DriftingLootData[] Create(DriftingLoot[] driftingLoot)
    {
        return null;
    }

    protected void Fill(DriftingLoot driftingLoot)
    {
        Id = (int)driftingLoot.Definition.Id;
        InstanceId = driftingLoot.InstanceId.GetInstanceID();
        Position = new Vector3Data(driftingLoot.transform.position);
        Rotation = new Vector3Data(driftingLoot.transform.rotation.eulerAngles);
        MeshId = driftingLoot.MeshId;
    }
}