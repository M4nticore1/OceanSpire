using System;
using UnityEngine;

[Serializable]
public abstract class CreatureData
{
    public CreatureIdEnum Id = 0;
    public Guid InstanceId = Guid.NewGuid();
    public Vector3Data Position = Vector3Data.Zero();
    public Vector3Data Rotation = Vector3Data.Zero();
}