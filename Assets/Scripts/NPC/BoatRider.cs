using System;
using UnityEngine;

[RequireComponent(typeof(EntityMovement))]
public class BoatRider : MonoBehaviour
{
    private Boat currentBoat;
    public Boat CurrentBoat => currentBoat;
    private const float enteringBoatTime = 1;

    public event Action<Boat> onEnteredBoat;
    public event Action<Boat> onExitedBoat;

    public void SetBoat(Boat boat)
    {
        currentBoat = boat;
    }

    public void StartEnteringBoat()
    {
        TimerManager.StartTimer(enteringBoatTime, EnterBoat);
    }

    private void EnterBoat()
    {
        currentBoat.EnterBoat();
        onEnteredBoat?.Invoke(currentBoat);
        transform.position = currentBoat.SeatSlot.position;
        transform.rotation = currentBoat.SeatSlot.rotation;
        transform.parent = currentBoat.SeatSlot;
    }

    public void StartExitingBoat()
    {
        TimerManager.StartTimer(enteringBoatTime, ExitBoat);
    }

    private void ExitBoat()
    {
        currentBoat.ExitBoat();
        currentBoat = null;
        transform.position = currentBoat.BoatDock.EntraceTransform.position;
        transform.rotation = currentBoat.BoatDock.EntraceTransform.rotation;
        transform.parent = null;
        onEnteredBoat?.Invoke(currentBoat);
    }

    public void StartMovingToDock()
    {
        CurrentBoat.StartMovingToDock();
    }
}