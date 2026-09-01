using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class Creature : MonoBehaviour
{
    [Header("Creature")]
    [SerializeField] private CreatureDefinition definition;
    public CreatureDefinition Definition => definition;

    [SerializeField] protected NavMeshAgent agent;

    [SerializeField] protected Movement movement;
    public Movement Movement => movement;

    [SerializeField] private InstanceId instanceId;
    public InstanceId InstanceId => instanceId;

    [field: SerializeField] public bool IsIdle { get; private set; } = true;
    [field: SerializeField] public bool IsInited { get; private set; } = false;

    private Coroutine determineNextActionCoroutine;

    public event Action OnIdleStarted;
    public event Action OnIdleStopped;

    protected virtual void Awake()
    {

    }

    protected virtual void OnEnable()
    {
        movement.OnDestinationReached += HandleDestinationReached;
        movement.OnMovementStopped += HandleMovementStopped;
    }

    protected virtual void OnDisable()
    {
        movement.OnDestinationReached -= HandleDestinationReached;
        movement.OnMovementStopped -= HandleMovementStopped;
    }

    protected virtual void OnDestroy()
    {

    }

    protected virtual void Start()
    {
        if (!IsInited) {
            var data = GetDefaultData();
            data.Position = new Vector3Data(transform.position);
            Init(data);
        }
    }

    public virtual void Tick()
    {

    }

    public void Init(CreatureData creatureData)
    {
        StartCoroutine(InitNextFrameCoroutine());
        HandleInit(creatureData);
        UpdateIdle();
        IsInited = true;
    }

    protected virtual void HandleInit(CreatureData data)
    {
        transform.position = data.Position.Vector3();
        transform.rotation = Quaternion.Euler(data.Rotation.Vector3());

        instanceId.SetGuid(data.InstanceId);
    }

    protected virtual void HandleInitNextFrame()
    {
        DetermineNextAction();
    }

    protected virtual void DetermineNextAction()
    {
        if (ShouldStartIdle()) {
            //Debug.Log("StartIdle");
            StartIdle();
            return;
        }
        else {
            StopIdle();
            return;
        }
    }

    protected virtual void StartIdle()
    {
        if (IsIdle) return;

        IsIdle = true;
        OnIdleStarted?.Invoke();
    }

    protected virtual void StopIdle()
    {
        if (!IsIdle) return;

        IsIdle = false;
        OnIdleStopped?.Invoke();
    }

    protected virtual bool ShouldStartIdle()
    {
        if (movement != null && movement.IsMoving) return false;

        return true;
    }

    protected abstract CreatureData GetDefaultData();

    // Idle
    protected void UpdateIdle()
    {
        if (ShouldStartIdle()) {
            StartIdle();
        }
        else {
            StopIdle();
        }
    }

    protected void TryStartIdle()
    {
        if (!ShouldStartIdle()) return;

        StartIdle();
    }

    protected void TryStopIdle()
    {
        if (ShouldStartIdle()) return;

        StopIdle();
    }

    // Movement
    protected virtual void HandleDestinationReached()
    {
        UpdateIdle();
    }

    protected virtual void HandleMovementStopped()
    {
        UpdateIdle();
    }

    protected void RunDetermineNextActionCoroutine()
    {
        //Debug.Log("RunDetermineNextActionCoroutine");
        if (determineNextActionCoroutine == null) {
            determineNextActionCoroutine = StartCoroutine(DetermineNextActionCoroutine());
        }
    }

    private IEnumerator DetermineNextActionCoroutine()
    {
        yield return new WaitForEndOfFrame();

        determineNextActionCoroutine = null;
        DetermineNextAction();
    }

    private IEnumerator InitNextFrameCoroutine()
    {
        yield return null;

        HandleInitNextFrame();
    }
}