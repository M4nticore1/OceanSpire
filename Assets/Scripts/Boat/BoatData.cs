using System;
using UnityEngine;

[Serializable]
public class BoatData
{
    public int Id = 0;
    public int InstanceId = -1;
    public Vector3Data Position = Vector3Data.Zero();
    public Vector3Data Rotation = Vector3Data.Zero();
    public int StateId = 0;
    public int? DockInstanceId = null;
    public HumanStatusEnum Status = HumanStatusEnum.Citizen; 

    public static BoatData Create(Boat boat)
    {
        return new BoatData()
        {
            Id = boat.Definition.BoatId,
            InstanceId = boat.InstanceId.GetInstanceId(),
            Position = new Vector3Data(boat.transform.position),
            Rotation = new Vector3Data(boat.transform.rotation.eulerAngles),
            StateId = (int)boat.CurrentStateEnum,
            DockInstanceId = boat.DockPoint?.InstanceId.GetInstanceId(),
            Status = boat.CurrentStatus
        };
    }

    public static BoatData[] Create(Boat[] boats)
    {
        var boatsData = new BoatData[boats.Length];

        for (int i = 0; i < boats.Length; i++) {
            boatsData[i] = Create(boats[i]);
        }

        return boatsData;
    }
}
