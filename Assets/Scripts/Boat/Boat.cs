using UnityEngine;
using UnityEngine.AI;

public class Boat : MonoBehaviour
{
    PierBuilding ownedPier;
    NavMeshAgent navAgent;
    ConstructionComponent constructionComponent;

    [SerializeField] private BoatData boatData = null;
    public BoatData BoatData => boatData;

    public float currentHealth { get; private set; } = 0;
    public int dockIndex { get; private set; } = 0;
    public bool isMoving { get; private set; } = false;

    public static event System.Action<Boat> OnBoadDestroyed;

    public void Initialize(PierBuilding ownedPier, int dockIndex, bool isMoving = false, float? health = null, float? positionX = null, float? positionZ = null)
    {
        Debug.Log("Initialize");
        navAgent = GetComponent<NavMeshAgent>();
        constructionComponent = GetComponent<ConstructionComponent>();

        this.ownedPier = ownedPier;
        this.dockIndex = dockIndex;

        constructionComponent.StartConstructing();
    }

    public void Demolish(bool isFXDemolish = true)
    {
        Destroy(gameObject);
    }

    private void SetTarget(Transform transform)
    {
        isMoving = true;
        navAgent.SetDestination(transform.position);
    }
}
