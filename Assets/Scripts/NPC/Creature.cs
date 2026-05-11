using System;
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

    private bool isIdle = true;

    public event Action onStartedIdle;
    public event Action onStoppedIdle;

    protected virtual void OnEnable()
    {
        movement.onMovementStarted += OnStartedMoving;
        movement.onMovementStopped += OnStoppedMoving;
    }

    protected virtual void OnDisable()
    {
        movement.onMovementStarted -= OnStartedMoving;
        movement.onMovementStopped -= OnStoppedMoving;
    }

    public void Init(CreatureData data)
    {
        transform.position = data.Position.Vector3();
        transform.rotation = Quaternion.Euler(data.Rotation.Vector3());

        instanceId.Init(data.InstanceId);

        OnInit(data);
        AssignIdle();
    }

    protected abstract void OnInit(CreatureData data);
    protected abstract bool ShouldStartIdle();

    // Idle
    protected void AssignIdle()
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
        isIdle = true;
        onStartedIdle?.Invoke();
    }

    protected void TryStopIdle()
    {
        if (ShouldStartIdle()) return;

        StopIdle();
    }

    protected void StopIdle()
    {
        isIdle = false;
        onStoppedIdle?.Invoke();
    }

    // Movement
    protected virtual void OnStartedMoving()
    {
        StopIdle();
    }

    protected virtual void OnStoppedMoving()
    {
        AssignIdle();
    }
}