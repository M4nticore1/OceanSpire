using UnityEngine;
using UnityEngine.AI;

public abstract class CreatureEntry
{
    public int id = 0;
    public Vector3 position;
    public Vector3 rotation;
}

public abstract class Creature : MonoBehaviour
{
    protected NavMeshAgent agent = null;
    protected EntityMovement movement = null;

    [SerializeField] protected CreatureData creatureData;
    public CreatureData CreatureData => creatureData;

    public virtual void Init(CreatureEntry data)
    {
        agent = GetComponent<NavMeshAgent>();
        movement = GetComponent<EntityMovement>();

        transform.position = data.position;
        transform.rotation = Quaternion.Euler(data.rotation);
    }

    public void SetNavAgentEnabled(bool value)
    {
        agent.enabled = value;
    }
}
