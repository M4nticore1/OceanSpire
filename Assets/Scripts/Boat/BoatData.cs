using System;
using UnityEngine;

[Serializable]
public class BoatData
{
    public int Id = 0;
    public int InstanceId = -1;
    public Vector3Data Position = Vector3Data.Zero();
    public Vector3Data Rotation = Vector3Data.Zero();
    public int? DockInstanceId = null;

    public static BoatData Create(Boat boat)
    {
        return new BoatData()
        {
            Id = boat.Definition.BoatId,
            InstanceId = boat.InstanceId.Id,
            Position = new Vector3Data(boat.transform.position),
            Rotation = new Vector3Data(boat.transform.rotation.eulerAngles),
            DockInstanceId = boat.DockPoint?.InstanceId.Id,
        };
    }
}
