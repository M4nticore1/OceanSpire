using System;
using UnityEngine;

[RequireComponent(typeof(EntityMovement))]
public class BoatRider : MonoBehaviour
{
    private Boat currentBoat;
    public Boat CurrentBoat => currentBoat;

    public bool isRidingOnBoat { get; private set; } = false;

    private const float useBoatTime = 1;
    private TimerHandle useBoatTimerHandle = new TimerHandle();

    public bool isEnteringBoat { get; private set; } = false;
    public bool isExitingBoat { get; private set; } = false;

    public event Action<Boat> onEnteredBoat;
    public event Action<Boat> onExitedBoat;

    public void StartEnteringBoat(Boat boat)
    {
        TimerManager.StartTimer(useBoatTimerHandle, useBoatTime, () => EnterBoat(boat));
        isEnteringBoat = true;
    }

    public void StopEnteringBoat()
    {
        TimerManager.RemoveTimer(useBoatTimerHandle);
        isEnteringBoat = false;
    }

    public void StopExitingBoat()
    {
        TimerManager.RemoveTimer(useBoatTimerHandle);
        isExitingBoat = false;
    }

    public void HandleBoatSetedIdle()
    {
        Human human = GetComponent<Human>();
        if (human.currentStatus == HumanStatus.Wanderer) return;

        StartExitingBoat();
    }

    public void EnterBoat(Boat boat)
    {
        SetCurrentBoat(boat);
        currentBoat.EnterBoat(this);
        transform.position = currentBoat.SeatSlot.position;
        transform.rotation = currentBoat.SeatSlot.rotation;
        transform.parent = currentBoat.SeatSlot;

        isRidingOnBoat = true;
        isEnteringBoat = false;
        onEnteredBoat?.Invoke(currentBoat);
    }

    private void SetCurrentBoat(Boat boat)
    {
        currentBoat = boat;
    }

    private void StartExitingBoat()
    {
        TimerManager.StartTimer(useBoatTimerHandle, useBoatTime, ExitBoat);
        isExitingBoat = true;
    }

    private void ExitBoat()
    {
        currentBoat.ExitBoat();
        transform.position = currentBoat.dockPoint.EntraceTransform.position;
        transform.rotation = currentBoat.dockPoint.EntraceTransform.rotation;
        transform.parent = null;
        currentBoat = null;

        isRidingOnBoat = false;
        isExitingBoat = false;
        SetCurrentBoat(null);

        onExitedBoat?.Invoke(currentBoat);
    }

    //public void StartMovingToDock()
    //{
    //    CurrentBoat.StartMovingToDock();
    //}
}