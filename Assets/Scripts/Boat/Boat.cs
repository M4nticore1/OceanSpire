using UnityEngine;
using UnityEngine.AI;

public class Boat : MonoBehaviour
{
    PierBuilding ownedPier;
    NavMeshAgent navAgent;
    ConstructionComponent constructionComponent;

    public int dockIndex { get; private set; } = 0;
    public bool isMoving { get; private set; } = false;
    [SerializeField] private int maxWeight = 0;
    public int MaxWeight { get { return maxWeight; } }

    public static event System.Action<Boat> OnBoadDestroyed;

    private void Initialize(int newBoatIndex)
    {
        navAgent = GetComponent<NavMeshAgent>();
        constructionComponent = GetComponent<ConstructionComponent>();

        dockIndex = newBoatIndex;
    }

    private void SetTarget(Transform transform)
    {
        isMoving = true;
        navAgent.SetDestination(transform.position);
    }

    public void StartBuilding(PierBuilding ownedPier, int newBoatIndex)
    {
        this.ownedPier = ownedPier;
        Initialize(newBoatIndex);
    }
}
