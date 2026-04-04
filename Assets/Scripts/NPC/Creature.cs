using System;
using UnityEngine;
using UnityEngine.AI;

public abstract class CreatureEntry
{
    public int instanceId { get; private set; }
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
    [SerializeField] private Health health;

    protected NavMeshAgent agent = null;
    protected EntityMovement movement = null;

    [SerializeField] protected CreatureData creatureData;
    public CreatureData CreatureData => creatureData;

    public int instanceId { get; private set; } = 0;

    public static event Action<Creature> onCreatureDeath;

    private void OnEnable()
    {
        if (health) {
            health.onDeath += Die;
        }
    }

    private void OnDisable()
    {
        if (health) {
            health.onDeath -= Die;
        }
    }

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        movement = GetComponent<EntityMovement>();
    }

    public virtual void Init(CreatureEntry data)
    {
        instanceId = data.instanceId;
        transform.position = data.position;
        transform.rotation = Quaternion.Euler(data.rotation);
    }

    private void Die()
    {
        onCreatureDeath?.Invoke(this);
    }
}