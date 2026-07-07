using System;
using UnityEngine;

[Serializable]
public class ElevatorPassengerData
{
    public ElevatorPassengerStateEnum State = ElevatorPassengerStateEnum.None;

    public static ElevatorPassengerData Default()
    {
        return new ElevatorPassengerData();
    }

    public static ElevatorPassengerData Create(ElevatorPassenger elevatorPassenger)
    {
        return new ElevatorPassengerData()
        {
            State = elevatorPassenger.CurrentStateEnum,
        };
    }
}