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

    public event Action OnIdleStarted;
    public event Action OnIdleStopped;

    protected virtual void OnEnable()
    {
        movement.OnReachedDestination += OnReachedPath;
        movement.OnMovementStopped += OnMovementStopped;
    }

    protected virtual void OnDisable()
    {
        movement.OnReachedDestination -= OnReachedPath;
        movement.OnMovementStopped -= OnMovementStopped;
    }

    public virtual void Tick()
    {

    }

    public void Init(CreatureData creatureData)
    {
        StartCoroutine(InitNextFrameCoroutine());
        OnInit(creatureData);
        UpdateIdle();
    }

    protected virtual void OnInit(CreatureData data)
    {
        transform.position = data.Position.Vector3();
        transform.rotation = Quaternion.Euler(data.Rotation.Vector3());

        instanceId.SetGuid(data.InstanceId);
    }

    protected virtual void OnInitNextFrame()
    {
        DetermineNextAction();
    }

    protected abstract void DetermineNextAction();

    protected abstract bool ShouldStartIdle();

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

    private IEnumerator InitNextFrameCoroutine()
    {
        yield return new WaitForEndOfFrame();

        OnInitNextFrame();
    }
}