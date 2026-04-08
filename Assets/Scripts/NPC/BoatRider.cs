using System;
using UnityEngine;

public class BoatRider : MonoBehaviour
{
    [SerializeField] private EntityMovement movement;

    public Boat selectedBoat;

    public bool isRidingOnBoat { get; private set; } = false;
    public bool isEnteringBoat { get; private set; } = false;
    public bool isExitingBoat { get; private set; } = false;
    public bool isMovingToBoat { get; private set; } = false;

    private const float useBoatTime = 1;
    private TimerHandle useBoatTimerHandle = new TimerHandle();

    public event Action<Boat> onEnteredBoat;
    public event Action<Boat> onExitedBoat;

    private void OnEnable()
    {
        movement.onStoppedMoving += OnMovementStopped;
    }

    private void OnDisable()
    {
        movement.onStoppedMoving -= OnMovementStopped;
    }

    public void StartEnteringBoat()
    {
        TimerManager.StartTimer(useBoatTimerHandle, useBoatTime, EnterBoat);
        isEnteringBoat = true;
    }

    public void StartExitingBoat()
    {
        TimerManager.StartTimer(useBoatTimerHandle, useBoatTime, ExitBoat);
        isExitingBoat = true;
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
        if (human.currentStateEnum == HumanStateEnum.Wanderer) return;

        StartExitingBoat();
    }

    public void EnterBoat()
    {
        selectedBoat.SetRider(this);
        transform.position = selectedBoat.SeatSlot.position;
        transform.rotation = selectedBoat.SeatSlot.rotation;
        transform.parent = selectedBoat.SeatSlot;

        isRidingOnBoat = true;
        isEnteringBoat = false;
        onEnteredBoat?.Invoke(selectedBoat);
    }

    public void ExitBoat()
    {
        selectedBoat.RemoveRider();
        transform.position = selectedBoat.dockPoint.EntraceTransform.position;
        transform.rotation = selectedBoat.dockPoint.EntraceTransform.rotation;
        transform.parent = null;

        isRidingOnBoat = false;
        isExitingBoat = false;

        onExitedBoat?.Invoke(selectedBoat);
    }

    public void SetSelectedBoat(Boat boat)
    {
        selectedBoat = boat;
    }

    public void SetSelectedBoat(int boatInstanceId)
    {
        Boat boat = BoatsManager.Instance.boatsDict[boatInstanceId];
        SetSelectedBoat(boat);
    }

    public void RemoveSelectedBoat()
    {
        selectedBoat = null;
    }

    public void StartMovingToBoat()
    {
        Vector3 position = selectedBoat.dockPoint.EntraceTransform.position;
        movement.TryMoveTo(position);
        isMovingToBoat = true;
    }

    private void StopMovingToDock()
    {
        isMovingToBoat = false;
    }

    private void OnMovementStopped()
    {
        if (!isMovingToBoat) return;

        StartEnteringBoat();
        StopMovingToDock();
    }
}