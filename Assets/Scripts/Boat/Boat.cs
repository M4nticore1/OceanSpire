using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.AI;

public class Boat : MonoBehaviour
{
    public PierBuilding ownedPier { get; private set; } = null;
    private NavMeshAgent navAgent = null;
    private LootManager lootManager = null;
    public ConstructionComponent constructionComponent { get; private set; } = null;

    [SerializeField] private BoatData boatData = null;
    public BoatData BoatData => boatData;

    [SerializeField] private Transform seatSlot = null;
    public Transform SeatSlot => seatSlot;

    private float applyPositionDistance = 0.5f;

    public float currentHealth { get; private set; } = 0;
    public int dockIndex { get; private set; } = 0;
    public bool isMoving { get; private set; } = false;
    public bool isReturningToDock { get; private set; } = false;
    public bool isDocked { get; private set; } = false;

    private Vector3 targetPosition = Vector3.zero;

    public static event System.Action<Boat> OnBoadDestroyed;
    public static event System.Action<Boat> onBoatDocked;

    private void Start()
    {
        
    }

    private void Update()
    {
        if (isMoving)
        {
            Debug.Log(transform.position);
            Debug.Log(targetPosition);
            float distance = math.distance(transform.position, targetPosition);
            if (distance <= applyPositionDistance)
            {
                if (isReturningToDock)
                    ToDock();
                else
                    StopMoving();
            }
        }
    }

    public void Initialize(PierBuilding ownedPier, bool isUnderConstruction, int dockIndex, bool isDocked = true, bool isReturningToDock = false, float? health = null)
    {
        Debug.Log("Initialize");
        navAgent = GetComponent<NavMeshAgent>();
        constructionComponent = GetComponent<ConstructionComponent>();

        this.ownedPier = ownedPier;
        this.dockIndex = dockIndex;
        this.isDocked = isDocked;
        this.isReturningToDock = isReturningToDock;

        if (isDocked)
            ToDock();
        else if (isReturningToDock)
            ReturnToDock();

        constructionComponent.InitializeConstruction(isUnderConstruction);
    }

    public void ReturnToDock()
    {
        SetTargetPosition(GetOwnedDockPosition());
        isReturningToDock = true;
    }

    private void SetTargetPosition(Vector3 position)
    {
        navAgent.SetDestination(position);
        isMoving = true;
        targetPosition = position;
    }

    private void ToDock()
    {
        isDocked = true;
        StopMoving();
        onBoatDocked?.Invoke(this);
    }

    private void StopMoving()
    {
        isMoving = false;
        isReturningToDock = false;
        navAgent.isStopped = true;
    }

    public void EnterBoat(Entity entity)
    {
        ReturnToDock();
        //SetTargetPosition(FindNearestLootPosition());
    }

    public void Demolish(bool isFXDemolish = true)
    {
        Destroy(gameObject);
    }

    private Vector3 FindNearestLootPosition()
    {
        return Vector3.zero;
    }

    private Vector3 GetOwnedDockPosition()
    {
        PierConstruction pierConstruction = ownedPier.constructionComponent.spawnedConstruction.GetComponent<PierConstruction>();
        return pierConstruction.BoatDockPositions[dockIndex].position;
    }
}
