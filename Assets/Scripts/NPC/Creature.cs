using System;
using UnityEngine;
using UnityEngine.AI;

public abstract class CreatureEntry
{
    public int instanceId { get; private set; } = -1;
    public int id { get; private set; }
    public Vector3 position { get; private set; }
    public Vector3 rotation { get; private set; }

    public CreatureEntry(int id, Vector3 position, Vector3 rotation)
    {
        this.id = id;
        this.position = position;
        this.rotation = rotation;
    }
}

public abstract class Creature : MonoBehaviour
{
    [SerializeField] protected NavMeshAgent agent;

    [SerializeField] protected EntityMovement movement;
    public EntityMovement Movement => movement;

    [SerializeField] protected CreatureData creatureData;
    public CreatureData CreatureData => creatureData;

    [SerializeField] private InstanceId instanceId;

    private bool isIdle = true;

    public event Action onStartedIdle;
    public event Action onStoppedIdle;

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
    private void AssignIdle()
    {
        if (ShouldStartIdle()) {
            StartIdle();
        }
        else {
            StopIdle();
        }
    }

    private void StartIdle()
    {
        isIdle = true;
        onStartedIdle?.Invoke();
    }

    private void StopIdle()
    {
        isIdle = false;
        onStoppedIdle?.Invoke();
    }
}