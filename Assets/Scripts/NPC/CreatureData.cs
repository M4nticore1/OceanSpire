using System;
using UnityEngine;

[Serializable]
public abstract class CreatureData
{
    public int Id = 0;
    public int InstanceId = 0;
    public Vector3Data Position = Vector3Data.Zero();
    public Vector3Data Rotation = Vector3Data.Zero();
}