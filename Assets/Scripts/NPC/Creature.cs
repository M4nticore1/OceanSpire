using System;
using UnityEngine;
using UnityEngine.AI;

public abstract class Creature : MonoBehaviour
{
    [SerializeField] protected NavMeshAgent agent;

    [SerializeField] protected Movement movement;
    public Movement Movement => movement;

    [SerializeField] protected CreatureDefinition creatureDefinition;
    public CreatureDefinition CreatureDefinition => creatureDefinition;

    [SerializeField] private InstanceId instanceId;

    private bool isIdle = true;

    public event Action onStartedIdle;
    public event Action onStoppedIdle;

    protected virtual void OnEnable()
    {
        movement.onStartedMoving += OnStartedMoving;
        movement.onStoppedMoving += OnStoppedMoving;
    }

    protected virtual void OnDisable()
    {
        movement.onStartedMoving -= OnStartedMoving;
        movement.onStoppedMoving -= OnStoppedMoving;
    }

    public void Init(CreatureDataV1 data)
    {
        transform.position = data.position;
        transform.rotation = Quaternion.Euler(data.rotation);

        instanceId.Init(data.instanceId);

        OnInit(data);
        AssignIdle();
    }

    protected abstract void OnInit(CreatureDataV1 data);
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