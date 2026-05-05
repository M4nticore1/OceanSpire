using System;
using UnityEngine;

public class BoatRider : MonoBehaviour
{
    public Boat selectedBoat;

    public bool isRidingOnBoat { get; private set; } = false;
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
        selectedBoat.SetRider(this);
        transform.position = selectedBoat.SeatSlot.position;
        transform.rotation = selectedBoat.SeatSlot.rotation;
        transform.SetParent(selectedBoat.SeatSlot);

        isRidingOnBoat = true;
        isEnteringBoat = false;
        onEnteredBoat?.Invoke(selectedBoat);
    }

    public void ExitBoat()
    {
        selectedBoat.RemoveRider();
        transform.position = selectedBoat.dockPoint.EntraceTransform.position;
        transform.rotation = selectedBoat.dockPoint.EntraceTransform.rotation;
        transform.SetParent(null);

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