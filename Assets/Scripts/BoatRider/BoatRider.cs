using System;
using UnityEngine;

public class BoatRider : MonoBehaviour
{
    public Boat SelectedBoat;

    [SerializeField] private Movement movement;

    [SerializeField] private float useBoatTime = 1;
    private TimerHandle useBoatTimerHandle = new TimerHandle();

    public bool IsRidingOnBoat { get; private set; } = false;
    public bool IsEnteringBoat { get; private set; } = false;
    public bool IsExitingBoat { get; private set; } = false;
    public bool IsMovingToBoat { get; private set; } = false;

    public event Action<Boat> OnEnteredBoat;
    public event Action<Boat> OnExitedBoat;

    public event Action<Boat> OnStartedMovingToBoat;
    public event Action<Boat> OnStoppedMovingToBoat;

    public event Action<Boat> OnBoatMovementStarted;
    public event Action<Boat> OnBoatMovementStopped;

    private void OnEnable()
    {
        movement.OnMovementStopped += OnMovementStopped;
    }

    private void OnDisable()
    {
        movement.OnMovementStopped -= OnMovementStopped;
    }

    public void Init(BoatRiderData boatRiderData)
    {
        if (boatRiderData.BoatInstanceId != null) {
            var instanceId = InstancesManager.Instance.GetInstance(boatRiderData.BoatInstanceId.Value);
            var selectedBoat = instanceId.GetComponent<Boat>();

            SetSelectedBoat(selectedBoat);
        }

        if (boatRiderData.Riding) {
            EnterBoat();
        }

        if (boatRiderData.MovingToBoat) {
            TryMoveToBoat();
        }
    }

    public void StartEnteringBoat()
    {
        TimerManager.Instance.StartTimer(useBoatTimerHandle, useBoatTime, EnterBoat);
        IsEnteringBoat = true;
    }

    public void StartExitingBoat()
    {
        TimerManager.Instance.StartTimer(useBoatTimerHandle, useBoatTime, ExitBoat);
        IsExitingBoat = true;
    }

    public void StopEnteringBoat()
    {
        TimerManager.Instance.RemoveTimer(useBoatTimerHandle);
        IsEnteringBoat = false;
    }

    public void StopExitingBoat()
    {
        TimerManager.Instance.RemoveTimer(useBoatTimerHandle);
        IsExitingBoat = false;
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
        IsEnteringBoat = false;
        OnEnteredBoat?.Invoke(SelectedBoat);
    }

    public void ExitBoat()
    {
        SelectedBoat.RemoveRider();

        Vector3 pos = SelectedBoat.DockPoint.EntraceTransform.position;
        Quaternion rot = SelectedBoat.DockPoint.EntraceTransform.rotation;

        transform.SetParent(null);
        transform.SetPositionAndRotation(pos, rot);

        IsRidingOnBoat = false;
        IsExitingBoat = false;

        movement.NavAgent.Warp(transform.position);
        movement.SetAgentEnabled(true);

        OnExitedBoat?.Invoke(SelectedBoat);
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

    public void TryMoveToBoat()
    {
        if (IsMovingToBoat) {
            Debug.Log("Rider is already moving to boat");
            return;
        }

        IsMovingToBoat = true;
        OnStartedMovingToBoat?.Invoke(SelectedBoat);
    }

    public void TryEndMoveBoat()
    {
        if (!IsMovingToBoat) {
            Debug.Log("Rider is already not moving to boat");
            return;
        }

        IsMovingToBoat = false;
        OnStoppedMovingToBoat?.Invoke(SelectedBoat);
    }

    public void HandleBoatMovementStarted()
    {
        OnBoatMovementStarted?.Invoke(SelectedBoat);
    }

    public void HandleBoatMovementStopped()
    {
        OnBoatMovementStopped?.Invoke(SelectedBoat);
    }

    private bool ShouldStartEnteringBoat()
    {
        if (!IsMovingToBoat) return false;
        
        if (Vector3.Distance(transform.position, SelectedBoat.DockPoint.EntraceTransform.position) > movement.NavAgent.stoppingDistance) return false;

        return true;
    }

    private void OnMovementStopped()
    {
        if (!ShouldStartEnteringBoat()) return;

        StartEnteringBoat();
        TryEndMoveBoat();
    }
}