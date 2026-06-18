using System;
using UnityEngine;

[Serializable]
public class ElevatorPassengerData
{
    public ElevatorPassengerStateEnum PassengerState = ElevatorPassengerStateEnum.None;

    public static ElevatorPassengerData Default()
    {
        return new ElevatorPassengerData();
    }

    public static ElevatorPassengerData Create(ElevatorPassenger elevatorPassenger)
    {
        return new ElevatorPassengerData()
        {
            PassengerState = elevatorPassenger.CurrentStateEnum,
        };
    }
}