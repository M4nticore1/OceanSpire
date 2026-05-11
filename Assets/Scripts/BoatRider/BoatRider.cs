using System;
using UnityEngine;

public class BoatRider : MonoBehaviour
{
    public Boat SelectedBoat;

    public bool IsRidingOnBoat { get; private set; } = false;
    public bool isEnteringBoat { get; private set; } = false;
    public bool isExitingBoat { get; private set; } = false;
    public bool isMovingToBoat { get; private set; } = false;

    private const float useBoatTime = 1;
    private TimerHandle useBoatTimerHandle = new TimerHandle();

    public event Action<Boat> onEnteredBoat;
    public event Action<Boat> onExitedBoat;
    public event Action onStartedFloating;
    public event Action onStoppedFloating;

    public void StartEnteringBoat()
    {
        TimerManager.Instance.StartTimer(useBoatTimerHandle, useBoatTime, EnterBoat);
        isEnteringBoat = true;
    }

    public void StartExitingBoat()
    {
        TimerManager.Instance.StartTimer(useBoatTimerHandle, useBoatTime, ExitBoat);
        isExitingBoat = true;
    }

    public void StopEnteringBoat()
    {
        TimerManager.Instance.RemoveTimer(useBoatTimerHandle);
        isEnteringBoat = false;
    }

    public void StopExitingBoat()
    {
        TimerManager.Instance.RemoveTimer(useBoatTimerHandle);
        isExitingBoat = false;
    }

    public void OnBoatSetedIdle()
    {
        Human human = GetComponent<Human>();
        if (human.CurrentStatusEnum == HumanStatusEnum.Wanderer) return;

        StartExitingBoat();
    }

    public void EnterBoat()
    {
        SelectedBoat.SetRider(this);
        transform.position = SelectedBoat.SeatSlot.position;
        transform.rotation = SelectedBoat.SeatSlot.rotation;
        transform.SetParent(SelectedBoat.SeatSlot);

        IsRidingOnBoat = true;
        isEnteringBoat = false;
        onEnteredBoat?.Invoke(SelectedBoat);
    }

    public void ExitBoat()
    {
        SelectedBoat.RemoveRider();
        transform.position = SelectedBoat.DockPoint.EntraceTransform.position;
        transform.rotation = SelectedBoat.DockPoint.EntraceTransform.rotation;
        transform.SetParent(null);

        IsRidingOnBoat = false;
        isExitingBoat = false;

        onExitedBoat?.Invoke(SelectedBoat);
    }

    public void SetSelectedBoat(Boat boat)
    {
        SelectedBoat = boat;
    }

    public void SetSelectedBoat(int boatInstanceId)
    {
        Boat boat = BoatsManager.Instance.GetBoat(boatInstanceId);
        SetSelectedBoat(boat);
    }

    public void RemoveSelectedBoat()
    {
        SelectedBoat = null;
    }

    public void StartMovingToBoat()
    {
        isMovingToBoat = true;
    }

    public void StopMovingToBoat()
    {
        isMovingToBoat = false;
    }

    public void HandleBoatStartedMoving()
    {
        onStartedFloating?.Invoke();
    }

    public void HandleBoatStoppedMoving()
    {
        onStoppedFloating?.Invoke();
    }
}