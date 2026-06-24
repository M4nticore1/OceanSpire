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

    private Coroutine waitingBoatCoroutine;

    public event Action<Boat> OnEnteredBoat;
    public event Action<Boat> OnExitedBoat;

    public event Action<Boat> OnTargetBoatSeted;
    public event Action<Boat> OnTargetBoatRemoved;

    public event Action<Boat> OnBoatMovementStarted;
    public event Action<Boat> OnBoatMovementStopped;

    public event Action<Boat> OnBoatSetedIdle;

    public static event Action<BoatRider, Boat> OnRiderEnteredBoat;
    public static event Action<BoatRider, Boat> OnRiderExitedBoat;

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

            if (instance) {
                var boat = instance.GetComponent<Boat>();

                if (boat)
                    TrySetTargetBoat(boat);
            }
        }

        int? ridingBoatInstanceId = boatRiderData.RidingBoatInstanceId;
        if (ridingBoatInstanceId != null) {
            var instance = InstancesManager.Instance.GetInstance(ridingBoatInstanceId.Value);

            if (instance) {
                var boat = instance.GetComponent<Boat>();

                if (boat)
                    EnterBoat(boat);
            }
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
        TimerManager.Instance.StartTimer(useBoatTimerHandle, useBoatTime, ExitBoat);
        IsExitingBoat = true;
    }

    public void StopExitingBoat()
    {
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
            Debug.LogError($"Boat not found at {name}");
            return;
        }

        SetRidingBoat(boat);

        movement.SetAgentEnabled(false);
        boat.SetCurrentRider(this);

        transform.position = boat.SeatSlot.position;
        transform.rotation = boat.SeatSlot.rotation;
        transform.SetParent(boat.SeatSlot);

        IsEnteringBoat = false;

        OnEnteredBoat?.Invoke(boat);
        OnRiderEnteredBoat?.Invoke(this, boat);
    }

    public void ExitBoat()
    {
        if (!RidingBoat) {
            Debug.LogError($"RidingBoat not found at {name}");
            return;
        }

        if (!RidingBoat.DockPoint) {
            Debug.LogError($"DockPoint not found at {RidingBoat}");
            return;
        }

        if (!RidingBoat.DockPoint.EntraceTransform) {
            Debug.LogError($"EntraceTransform not found at {RidingBoat.DockPoint}");
            return;
        }

        RidingBoat.RemoveCurrentRider();

        Vector3 pos = RidingBoat.DockPoint.EntraceTransform.position;
        Quaternion rot = RidingBoat.DockPoint.EntraceTransform.rotation;

        transform.SetParent(null);
        transform.SetPositionAndRotation(pos, rot);

        IsExitingBoat = false;

        movement.NavAgent.Warp(transform.position);
        movement.SetAgentEnabled(true);

        RemoveRidingBoat();

        OnExitedBoat?.Invoke(TargetBoat);
        OnRiderExitedBoat?.Invoke(this, TargetBoat);
    }

    public bool TrySetTargetBoat(Boat boat)
    {
        if (!boat) {
            Debug.LogError("Boat is not valid. Use RemoveTargetBoat method instead of this.");
            return false;
        }

        if (boat == TargetBoat) return false;

        TargetBoat = boat;
        boat.SetTargetRider(this);

        OnTargetBoatSeted?.Invoke(boat);

        return true;
    }

    public void RemoveTargetBoat()
    {
        if (!TargetBoat) {
            Debug.LogError("TargetBoat is already null");
            return;
        }

        var boat = TargetBoat;

        TargetBoat = null;
        boat.RemoveTargetRider();

        OnTargetBoatRemoved?.Invoke(boat);
    }

    public void SetRidingBoat(Boat boat)
    {
        if (!boat) {
            Debug.LogError("ridingBoat is not valid", this);
            return;
        }

        RidingBoat = boat;
    }

    public void RemoveRidingBoat()
    {
        RidingBoat = null;
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
    }

    private bool ShouldMoveToBoat()
    {
        if (TargetBoat) return false;

        return true;
    }

    private bool ShouldStartEnteringBoat()
    {
        if (!TargetBoat) return false;

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
            Debug.LogError($"RidingBoat not found at {name}");
            yield break;
        }

        while (boat.CurrentStateEnum != BoatStateEnum.Idle || boat.CurrentRider) {
            yield return new WaitForEndOfFrame();
        }

        if (boat != TargetBoat) yield break;

        TryStartEnteringBoat(boat);
    }
}