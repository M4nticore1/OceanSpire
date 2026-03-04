using UnityEngine;
using UnityEngine.AI;

public abstract class EntityEntry
{
    public int id = 0;
    public Vector3 position;
    public Vector3 rotation;

    public EntityEntry(int id, Vector3 position, Vector3 rotation)
    {
        this.id = id;
        this.position = position;
        this.rotation = rotation;
    }
}

public abstract class Entity : MonoBehaviour
{
    protected NavMeshAgent agent = null;
    protected EntityMovement movement = null;

    [SerializeField] protected CreatureData creatureData;
    public CreatureData CreatureData => creatureData;

    protected virtual void OnEnable()
    {
        EventBus.onNavMeshBaked += OnNavMeshBaked;
    }

    protected virtual void OnDisable()
    {
        EventBus.onNavMeshBaked -= OnNavMeshBaked;
    }

    public virtual void Init(EntityEntry data)
    {
        agent = GetComponent<NavMeshAgent>();
        movement = GetComponent<EntityMovement>();

        transform.position = data.position;
        transform.rotation = Quaternion.Euler(data.rotation);

        if (CityManager.Instance.bakeNavMeshCoroutine != null) {
            SetNavAgentEnabled(false);
        }
    }

    public void SetNavAgentEnabled(bool value)
    {
        agent.enabled = value;
    }

    private void OnNavMeshBaked()
    {
        if (!agent) return;
        if (agent.enabled == true) return;

        SetNavAgentEnabled(true);
    }
}
