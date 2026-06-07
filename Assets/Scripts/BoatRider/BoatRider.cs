using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class BoatRider : MonoBehaviour
{
    public Boat TargetBoat;
    public Boat RidingBoat;

    [SerializeField] private Movement movement;

    [SerializeField] private float useBoatTime = 1;
    private TimerHandle useBoatTimerHandle = new TimerHandle();

    public bool IsEnteringBoat { get; private set; } = false;
    public bool IsExitingBoat { get; private set; } = false;
    public bool IsMovingToBoat { get; private set; } = false;

    private Coroutine waitingBoatCoroutine;

    public event Action<Boat> OnEnteredBoat;
    public event Action<Boat> OnExitedBoat;

    public event Action<Boat> OnStartedMovingToBoat;
    public event Action<Boat> OnStoppedMovingToBoat;

    public event Action<Boat> OnBoatMovementStarted;
    public event Action<Boat> OnBoatMovementStopped;

    public event Action<Boat> OnBoatSetedIdle;

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
        int? targetBoatInstanceId = boatRiderData.TargetBoatInstanceId;
        if (targetBoatInstanceId != null) {
            var instance = InstancesManager.Instance.GetInstance(targetBoatInstanceId.Value);
            var boat = instance.GetComponent<Boat>();

            TrySetTargetBoat(boat);
        }

        int? ridingBoatInstanceId = boatRiderData.RidingBoatInstanceId;
        if (ridingBoatInstanceId != null) {
            var instance = InstancesManager.Instance.GetInstance(ridingBoatInstanceId.Value);
            var boat = instance.GetComponent<Boat>();

            EnterBoat(boat);
        }

        if (boatRiderData.MovingToBoat) {
            TryMoveToBoat();
        }
    }

    public bool TryStartEnteringBoat(Boat boat)
    {
        if (IsEnteringBoat) return false;

        TimerManager.Instance.StartTimer(useBoatTimerHandle, useBoatTime, () => EnterBoat(boat));
        IsEnteringBoat = true;

        return true;
    }

    public bool TryStopEnteringBoat()
    {
        if (!IsEnteringBoat) return false;

        TimerManager.Instance.RemoveTimer(useBoatTimerHandle);
        IsEnteringBoat = false;

        return true;
    }

    public void StartExitingBoat()
    {
        Debug.Log("StartExitingBoat");
        TimerManager.Instance.StartTimer(useBoatTimerHandle, useBoatTime, ExitBoat);
        IsExitingBoat = true;
    }

    public void StopExitingBoat()
    {
        Debug.Log("StopExitingBoat");
        TimerManager.Instance.RemoveTimer(useBoatTimerHandle);
        IsExitingBoat = false;
    }

    public void WaitForBoatAndEnter()
    {
        waitingBoatCoroutine = StartCoroutine(WaitForBoatAndEnterCoroutine(TargetBoat));
    }

    public void StopWaitingForBoat()
    {
        if (waitingBoatCoroutine == null) return;

        StopCoroutine(waitingBoatCoroutine);
    }

    public void HandleBoatSetedIdle(Boat boat)
    {
        OnBoatSetedIdle?.Invoke(boat);
    }

    public void EnterBoat(Boat boat)
    {
        if (!boat) {
            Debug.Log($"Boat not found at {name}");
            return;
        }

        if (!TargetBoat) {
            Debug.Log($"Selected Boat not found at {name}");
            return;
        }

        if (boat != TargetBoat) return;

        SetRidingBoat(TargetBoat);

        RidingBoat.SetRider(this);

        transform.position = TargetBoat.SeatSlot.position;
        transform.rotation = TargetBoat.SeatSlot.rotation;
        transform.SetParent(TargetBoat.SeatSlot);

        IsEnteringBoat = false;

        OnEnteredBoat?.Invoke(RidingBoat);
    }

    public void ExitBoat()
    {
        Debug.Log("ExitBoat");
        if (!RidingBoat) {
            Debug.Log($"Entered Boat not found at {name}");
            return;
        }

        if (!RidingBoat.DockPoint) {
            Debug.Log($"Dock Point not found at {RidingBoat}");
            return;
        }

        if (!RidingBoat.DockPoint.EntraceTransform) {
            Debug.Log($"Entrace Transform Boat not found at {RidingBoat.DockPoint}");
            return;
        }

        RidingBoat.RemoveRider();

        Vector3 pos = RidingBoat.DockPoint.EntraceTransform.position;
        Quaternion rot = RidingBoat.DockPoint.EntraceTransform.rotation;

        transform.SetParent(null);
        transform.SetPositionAndRotation(pos, rot);

        IsExitingBoat = false;

        movement.NavAgent.Warp(transform.position);
        movement.SetAgentEnabled(true);

        RemoveRidingBoat();
        OnExitedBoat?.Invoke(TargetBoat);
    }

    public bool TrySetTargetBoat(Boat boat)
    {
        if (!boat) {
            Debug.Log("Target Boat not found");
            return false;
        }

        if (boat == TargetBoat) return false;

        TargetBoat = boat;
        return true;
    }

    public void RemoveTargetBoat()
    {
        TargetBoat = null;
    }

    public void SetRidingBoat(Boat boat)
    {
        RidingBoat = boat;
    }

    public void RemoveRidingBoat()
    {
        RidingBoat = null;
    }

    public void TryMoveToBoat()
    {
        if (!ShouldMoveToBoat()) return;

        MoveToBoat();
    }

    public void MoveToBoat()
    {
        IsMovingToBoat = true;
        OnStartedMovingToBoat?.Invoke(TargetBoat);
    }

    public void TryEndMoveBoat()
    {
        if (!IsMovingToBoat) {
            Debug.Log("Rider is already not moving to boat");
            return;
        }

        EndMoveToBoat(TargetBoat);
    }

    public void EndMoveToBoat(Boat boat)
    {
        IsMovingToBoat = false;
        OnStoppedMovingToBoat?.Invoke(boat);
    }

    public void HandleBoatMovementStarted()
    {
        OnBoatMovementStarted?.Invoke(TargetBoat);
    }

    public void HandleBoatMovementStopped()
    {
        OnBoatMovementStopped?.Invoke(TargetBoat);
    }

    private void OnMovementStopped()
    {
        if (!ShouldStartEnteringBoat()) return;

        WaitForBoatAndEnter();
        TryEndMoveBoat();
    }

    private bool ShouldMoveToBoat()
    {
        if (IsMovingToBoat) return false;

        return true;
    }

    private bool ShouldStartEnteringBoat()
    {
        if (!IsMovingToBoat) return false;

        if (!TargetBoat) {
            Debug.Log($"Selected Boat not found at {name}");
            return false;
        }

        if (!TargetBoat.DockPoint) {
            Debug.Log($"Boat Dock not found at {TargetBoat}");
            return false;
        }

        if (Vector3.Distance(transform.position, TargetBoat.DockPoint.EntraceTransform.position) > movement.NavAgent.stoppingDistance) return false;

        return true;
    }

    private IEnumerator WaitForBoatAndEnterCoroutine(Boat boat)
    {
        if (RidingBoat) yield break;

        if (!boat) {
            Debug.Log($"Selected Boat not found at {name}");
            yield break;
        }

        while (boat.CurrentStateEnum != BoatStateEnum.Idle || boat.CurrentRider) {
            Debug.Log(boat.CurrentStateEnum);
            yield return new WaitForEndOfFrame();
        }

        if (boat != TargetBoat) yield break;

        TryStartEnteringBoat(boat);
    }
}