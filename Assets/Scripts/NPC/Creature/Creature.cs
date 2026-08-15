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
        movement.OnDestinationReached += OnReachedPath;
        movement.OnMovementStopped += OnMovementStopped;
    }

    protected virtual void OnDisable()
    {
        movement.OnDestinationReached -= OnReachedPath;
        movement.OnMovementStopped -= OnMovementStopped;
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
        RunDetermineNextActionCoroutine();
        transform.position = data.Position.Vector3();
        transform.rotation = Quaternion.Euler(data.Rotation.Vector3());

        instanceId.SetGuid(data.InstanceId);
    }

    protected virtual void HandleInitNextFrame()
    {

    }

    protected virtual void DetermineNextAction()
    {
        if (ShouldStartIdle()) {
            //Debug.Log("StartIdle");
            StartIdle();
            return;
        }
        if (ShouldStopIdle()) {
            StopIdle();
            return;
        }
    }

    protected virtual void StartIdle()
    {
        IsIdle = true;
        movement.TryStopMoving();
        OnIdleStarted?.Invoke();
    }

    protected virtual void StopIdle()
    {
        IsIdle = false;
        OnIdleStopped?.Invoke();
    }

    protected virtual bool ShouldStartIdle()
    {
        if (IsIdle) return false;

        return true;
    }

    protected virtual bool ShouldStopIdle()
    {
        if (!IsIdle) return false;
        if (ShouldStartIdle()) return false;

        return true;
    }

    protected abstract CreatureData GetDefaultData();

    // Idle
    protected void UpdateIdle()
    {
        if (ShouldStartIdle()) {
            StartIdle();
        }
        else if (ShouldStopIdle()) {
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
        if (!ShouldStopIdle()) return;

        StopIdle();
    }

    // Movement
    protected virtual void OnReachedPath()
    {
        UpdateIdle();
    }

    protected virtual void OnMovementStopped()
    {
        UpdateIdle();
    }

    protected void RunDetermineNextActionCoroutine()
    {
        if (determineNextActionCoroutine == null) {
            determineNextActionCoroutine = StartCoroutine(DetermineNextActionCoroutine());
        }
    }

    private IEnumerator DetermineNextActionCoroutine()
    {
        yield return new WaitForEndOfFrame();

        DetermineNextAction();
        determineNextActionCoroutine = null;
    }

    private IEnumerator InitNextFrameCoroutine()
    {
        yield return null;

        HandleInitNextFrame();
    }
}