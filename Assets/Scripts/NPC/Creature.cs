using System;
using UnityEngine;
using UnityEngine.AI;

public abstract class CreatureEntry
{
    public int instanceId { get; private set; } = -1;
    public int id { get; private set; }
    public Vector3 position { get; private set; }
    public Vector3 rotation { get; private set; }

    public CreatureEntry(int id, int instanceId, Vector3 position, Vector3 rotation)
    {
        this.id = id;
        this.instanceId = instanceId;
        this.position = position;
        this.rotation = rotation;
    }
}

public abstract class Creature : MonoBehaviour
{
    [SerializeField] protected NavMeshAgent agent;

    [SerializeField] protected Movement movement;
    public Movement Movement => movement;

    [SerializeField] protected CreatureData creatureData;
    public CreatureData CreatureData => creatureData;

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

    public void Init(CreatureEntry data)
    {
        transform.position = data.position;
        transform.rotation = Quaternion.Euler(data.rotation);

        instanceId.Init(data.instanceId);

        OnInit(data);
        AssignIdle();
    }

    protected abstract void OnInit(CreatureEntry data);
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

    protected void StartIdle()
    {
        isIdle = true;
        onStartedIdle?.Invoke();
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