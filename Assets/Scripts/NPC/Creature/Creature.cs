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

    public bool IsIdle { get; private set; } = true;
    public bool IsInited { get; private set; } = false;

    private IEnumerator determineNextActionCoroutine;

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
        StartCoroutine(DetermineNextActionCoroutine());
        transform.position = data.Position.Vector3();
        transform.rotation = Quaternion.Euler(data.Rotation.Vector3());

        instanceId.SetGuid(data.InstanceId);
    }

    protected virtual void HandleInitNextFrame()
    {

    }

    protected abstract void DetermineNextAction();
    protected abstract bool ShouldStartIdle();
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

    protected void StartIdle()
    {
        IsIdle = true;
        OnIdleStarted?.Invoke();
    }

    protected void TryStopIdle()
    {
        if (ShouldStartIdle()) return;

        StopIdle();
    }

    protected void StopIdle()
    {
        IsIdle = false;
        OnIdleStopped?.Invoke();
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

    protected IEnumerator DetermineNextActionCoroutine()
    {
        if (determineNextActionCoroutine != null) yield break;
        yield return new WaitForEndOfFrame();

        DetermineNextAction();
        determineNextActionCoroutine = null;
    }

    private IEnumerator InitNextFrameCoroutine()
    {
        yield return new WaitForEndOfFrame();

        HandleInitNextFrame();
    }
}