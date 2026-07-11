using System;
using UnityEngine;

[Serializable]
public class BoatData
{
    public BoatIdEnum Id = 0;
    public Guid InstanceId = Guid.NewGuid();
    public Guid? DockInstanceId = null;
    public HumanStatusEnum Status = HumanStatusEnum.Citizen;
    public BoatStateEnum State = BoatStateEnum.Idle;
    public InventoryData InventoryData = InventoryData.Default();
    public Vector3Data Position = Vector3Data.Zero();
    public Vector3Data Rotation = Vector3Data.Zero();

    public static BoatData Default()
    {
        return new BoatData();
    }

    public static BoatData Create(Boat boat)
    {
        return new BoatData()
        {
            Id = boat.Definition.BoatId,
            InstanceId = boat.InstanceId.GetGuid(),
            Status = boat.CurrentStatus,
            State = boat.CurrentStateEnum,
            DockInstanceId = boat.DockPoint?.InstanceId.GetGuid(),
            InventoryData = InventoryData.Create(boat.Inventory),
            Position = new Vector3Data(boat.transform.position),
            Rotation = new Vector3Data(boat.transform.rotation.eulerAngles),
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
