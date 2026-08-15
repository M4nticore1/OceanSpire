using System;
using System.Collections;
using UnityEngine;

public class BoatRider : MonoBehaviour
{
    public Boat TargetBoat;
    public Boat RidingBoat;

    [SerializeField] private Movement movement;
    public Movement Movement => movement;

    [SerializeField] private HealthComponent healthComponent;
    public HealthComponent HealthComponent => healthComponent;

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

    public event Action<Boat> OnBoatSetIdle;

    public static event Action<BoatRider, Boat> OnRiderEnteredBoat;
    public static event Action<BoatRider, Boat> OnRiderExitedBoat;

    private void OnEnable()
    {
        movement.OnMovementStopped += OnMovementStopped;
        healthComponent.OnDied += OnDied;
    }

    private void OnDisable()
    {
        movement.OnMovementStopped -= OnMovementStopped;
        healthComponent.OnDied -= OnDied;

        StopAllRidingProcesses();
    }

    public void Init()
    {
        Init(BoatRiderData.Default() ?? new BoatRiderData());
    }

    public void Init(BoatRiderData boatRiderData)
    {
        if (boatRiderData == null) {
            Debug.LogError("boatRiderData is not valid");
            Init();
            return;
        }

        Guid? targetBoatInstanceId = boatRiderData.TargetBoatInstanceId;
        if (targetBoatInstanceId != null) {
            var instance = InstancesManager.Instance.GetInstance(targetBoatInstanceId.Value);

            if (instance != null) {
                var boat = instance.GetComponent<Boat>();

                if (boat != null)
                    TrySetTargetBoat(boat);
            }
        }

        Guid? ridingBoatInstanceId = boatRiderData.RidingBoatInstanceId;
        if (ridingBoatInstanceId != null) {
            var instance = InstancesManager.Instance.GetInstance(ridingBoatInstanceId.Value);

            if (instance != null) {
                var boat = instance.GetComponent<Boat>();

                if (boat != null) {
                    EnterBoat(boat);
                }
            }
        }
    }

    public bool TryStartEnteringBoat(Boat boat)
    {
        if (!ShouldStartEnteringBoat()) return false;

        CancelUseBoatTimer();

        TimerManager.Instance.StartTimer(useBoatTimerHandle, useBoatTime, () => EnterBoat(boat));
        IsEnteringBoat = true;

        return true;
    }

    public bool TryStopEnteringBoat()
    {
        if (!IsEnteringBoat) return false;

        CancelUseBoatTimer();
        IsEnteringBoat = false;

        return true;
    }

    public void StartExitingBoat()
    {
        if (RidingBoat == null) return;

        StopWaitingForBoat();
        TryStopEnteringBoat();
        CancelUseBoatTimer();

        TimerManager.Instance.StartTimer(useBoatTimerHandle, useBoatTime, ExitBoat);
        IsExitingBoat = true;
    }

    public void StopExitingBoat()
    {
        if (!IsExitingBoat) return;

        CancelUseBoatTimer();
        IsExitingBoat = false;
    }

    public void WaitForBoatAndEnter()
    {
        if (RidingBoat != null) {
            StopExitingBoat();
        }
        else if (ShouldStartEnteringBoat()) {
            StopWaitingForBoat();
            waitingBoatCoroutine = StartCoroutine(WaitForBoatAndEnterCoroutine(TargetBoat));
        }
    }

    public void StopWaitingForBoat()
    {
        if (waitingBoatCoroutine != null) {
            StopCoroutine(waitingBoatCoroutine);
            waitingBoatCoroutine = null;
        }
    }

    public void HandleBoatSetIdle(Boat boat)
    {
        Debug.Log("HandleBoatSetIdle");
        OnBoatSetIdle?.Invoke(boat);
    }

    public void EnterBoat(Boat boat)
    {
        IsEnteringBoat = false;

        if (boat == null) {
            Debug.LogError($"[{nameof(BoatRider)}] Boat not found at {name}");
            return;
        }

        if (boat.CurrentRider != null) {
            Debug.LogError($"[{nameof(BoatRider)}] Boat already has another rider!");
            return;
        }

        SetRidingBoat(boat);

        movement.SetAgentEnabled(false);
        boat.SetCurrentRider(this);

        transform.position = boat.SeatSlot.position;
        transform.rotation = boat.SeatSlot.rotation;
        transform.SetParent(boat.SeatSlot);

        OnEnteredBoat?.Invoke(boat);
        OnRiderEnteredBoat?.Invoke(this, boat);
    }

    public void ExitBoat()
    {
        IsExitingBoat = false;

        if (RidingBoat == null) {
            Debug.LogError($"[{nameof(BoatRider)}] RidingBoat not found at {name}");
            return;
        }

        if (RidingBoat.DockPoint == null) {
            Debug.LogError($"[{nameof(BoatRider)}] DockPoint not found at {RidingBoat}");
            return;
        }

        if (RidingBoat.DockPoint.EntraceTransform == null) {
            Debug.LogError($"[{nameof(BoatRider)}] EntraceTransform not found at {RidingBoat.DockPoint}");
            return;
        }

        RidingBoat.RemoveCurrentRider();

        Vector3 pos = RidingBoat.DockPoint.EntraceTransform.position;
        Quaternion rot = RidingBoat.DockPoint.EntraceTransform.rotation;

        transform.SetParent(null);
        transform.SetPositionAndRotation(pos, rot);

        movement.NavAgent.Warp(transform.position);
        movement.SetAgentEnabled(true);

        var lastRidingBoat = RidingBoat;
        RemoveRidingBoat();

        OnExitedBoat?.Invoke(lastRidingBoat);
        OnRiderExitedBoat?.Invoke(this, lastRidingBoat);
    }

    public bool TrySetTargetBoat(Boat boat)
    {
        if (boat == null) {
            Debug.LogError("Boat is not valid. Use RemoveTargetBoat method instead of this.");
            return false;
        }

        if (boat == TargetBoat) return false;

        StopWaitingForBoat();

        TargetBoat = boat;
        boat.SetTargetRider(this);

        OnTargetBoatSeted?.Invoke(boat);

        return true;
    }

    public void RemoveTargetBoat()
    {
        if (TargetBoat == null) return;

        StopWaitingForBoat();
        TryStopEnteringBoat();

        var boat = TargetBoat;

        TargetBoat = null;
        boat.RemoveTargetRider();

        OnTargetBoatRemoved?.Invoke(boat);
    }

    public void SetRidingBoat(Boat boat)
    {
        if (boat == null) {
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

    private void OnDied()
    {
        StopAllRidingProcesses();
    }

    private void CancelUseBoatTimer()
    {
        TimerManager.Instance.RemoveTimer(useBoatTimerHandle);
    }

    private void StopAllRidingProcesses()
    {
        StopWaitingForBoat();
        TryStopEnteringBoat();
    }

    private bool ShouldStartEnteringBoat()
    {
        if (IsEnteringBoat) return false;
        if (TargetBoat == null) return false;

        if (TargetBoat.DockPoint == null) {
            Debug.LogError($"BoatDock not found at {TargetBoat}");
            return false;
        }

        if (!movement.IsReachedPosition(TargetBoat.DockPoint.EntraceTransform.position)) return false;

        return true;
    }

    private IEnumerator WaitForBoatAndEnterCoroutine(Boat boat)
    {
        if (RidingBoat != null) yield break;

        if (boat == null) {
            Debug.LogError($"RidingBoat not found at {name}");
            yield break;
        }

        while (boat.CurrentStateEnum != BoatStateEnum.Idle || boat.CurrentRider != null) {
            yield return new WaitForEndOfFrame();
        }

        if (boat != TargetBoat) yield break;

        waitingBoatCoroutine = null;
        TryStartEnteringBoat(boat);
    }
}