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

    private bool isIdle = true;

    public event Action onStartedIdle;
    public event Action onStoppedIdle;

    protected virtual void OnEnable()
    {
        movement.OnMovementStarted += OnStartedMoving;
        movement.OnMovementStopped += OnStoppedMoving;
    }

    protected virtual void OnDisable()
    {
        movement.OnMovementStarted -= OnStartedMoving;
        movement.OnMovementStopped -= OnStoppedMoving;
    }

    public void Init(CreatureData creatureData)
    {
        StartCoroutine(InitNextFrame());
        OnInit(creatureData);
        AssignIdle();
    }

    protected virtual void OnInit(CreatureData data)
    {
        transform.position = data.Position.Vector3();
        transform.rotation = Quaternion.Euler(data.Rotation.Vector3());

        instanceId.Register(data.InstanceId);
    }

    protected virtual void OnInitedNextFrame()
    {
        
    }

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

    private IEnumerator InitNextFrame()
    {
        yield return new WaitForEndOfFrame();

        OnInitedNextFrame();
    }
}