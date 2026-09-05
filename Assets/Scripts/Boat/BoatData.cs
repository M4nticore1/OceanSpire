using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BoatData
{
    public BoatIdEnum Id = 0;
    public Guid InstanceId = Guid.NewGuid();
    public Guid? DockInstanceId = null;
    public BoatStatusEnum Status = BoatStatusEnum.Citizen;
    public BoatStateEnum State = BoatStateEnum.Idle;
    public InventoryData InventoryData = InventoryData.Default();
    public Vector3Data Position = Vector3Data.Zero();
    public Vector3Data Rotation = Vector3Data.Zero();
    public bool IsForcedMovingToDock;

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
            IsForcedMovingToDock = boat.IsForcedMovingToDock,
        };
    }

    public static List<BoatData> Create(IReadOnlyList<Boat> boats)
    {
        var boatsData = new List<BoatData>();
        foreach (var boat in boats) {
            if (boat == null) continue;

            var data = Create(boat);
            if (data == null) continue;

            boatsData.Add(data);
        }

        return boatsData;
    }
}