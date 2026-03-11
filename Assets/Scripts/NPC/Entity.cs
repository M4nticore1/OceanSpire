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
    private IslandNavMeshBuilder cityNavMeshManager;
    protected NavMeshAgent agent = null;
    protected EntityMovement movement = null;
    protected EntityCityNavigator cityNavigator = null;

    [SerializeField] protected CreatureData creatureData;
    public CreatureData CreatureData => creatureData;

    protected virtual void Awake()
    {
        cityNavMeshManager = FindAnyObjectByType<IslandNavMeshBuilder>();
        agent = GetComponent<NavMeshAgent>();
        movement = GetComponent<EntityMovement>();
        cityNavigator = GetComponent<EntityCityNavigator>();
    }

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
        transform.position = data.position;
        transform.rotation = Quaternion.Euler(data.rotation);

        //if (cityNavMeshManager.bakeNavMeshCoroutine != null) {
        //    SetNavAgentEnabled(false);
        //}
    }

    private void OnNavMeshBaked()
    {
        if (cityNavigator.IsRidingOnElevator) return;

        movement.SetAgentEnabled(true);
    }
}
