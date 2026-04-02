using UnityEngine;
using UnityEngine.AI;

public abstract class CreatureEntry
{
    public int id = 0;
    public Vector3 position;
    public Vector3 rotation;

    public CreatureEntry(int id, Vector3 position, Vector3 rotation)
    {
        this.id = id;
        this.position = position;
        this.rotation = rotation;
    }
}

public abstract class Creature : MonoBehaviour
{
    private IslandNavMeshBuilder cityNavMeshManager;
    protected NavMeshAgent agent = null;
    protected EntityMovement movement = null;

    [SerializeField] protected CreatureData creatureData;
    public CreatureData CreatureData => creatureData;

    protected virtual void Awake()
    {
        cityNavMeshManager = FindAnyObjectByType<IslandNavMeshBuilder>();
        agent = GetComponent<NavMeshAgent>();
        movement = GetComponent<EntityMovement>();
    }

    public virtual void Init(CreatureEntry data)
    {
        transform.position = data.position;
        transform.rotation = Quaternion.Euler(data.rotation);

        //if (cityNavMeshManager.bakeNavMeshCoroutine != null) {
        //    SetNavAgentEnabled(false);
        //}
    }
}