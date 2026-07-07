using System;
using UnityEngine;

[Serializable]
public abstract class DriftingLootData
{
    public DriftingLootId Id = 0;
    public Guid InstanceId = Guid.NewGuid();
    public Vector3Data Position = Vector3Data.Zero();
    public Vector3Data Rotation = Vector3Data.Zero();
    public Vector3Data Destination = Vector3Data.Zero();
    public Vector3Data MeshRotation = Vector3Data.Zero();
    public int MeshId = 0;

    protected void Fill(DriftingLoot driftingLoot)
    {
        Id = driftingLoot.Definition.Id;
        InstanceId = driftingLoot.InstanceId.GetGuid();
        Position = new Vector3Data(driftingLoot.transform.position);
        Rotation = new Vector3Data(driftingLoot.transform.rotation.eulerAngles);
        Destination = new Vector3Data(driftingLoot.Destination);
        MeshId = driftingLoot.MeshId;
    }
}