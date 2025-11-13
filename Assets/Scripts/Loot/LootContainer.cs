using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct LootEntry
{
    public ItemData itemData;
    public int dropChance;
    public int minAmount;
    public int maxAmount;
}

[System.Serializable]
public enum TransportMethod
{
    Floating,
    Flying
}

public class LootContainer : MonoBehaviour
{
    private GameManager gameManager = null;

    [Header("Loot")]
    [SerializeField] private List<LootEntry> possibleLoot = new List<LootEntry>();
    private List<ItemInstance> containedLoot = new List<ItemInstance>();

    [Header("Moving")]
    [SerializeField] private bool isMovable = true;
    [SerializeField] private TransportMethod transportMethod = TransportMethod.Floating;
    [SerializeField] private float moveSpeed = 0.0f;

    [HideInInspector] public Vector3 moveDirection = Vector3.zero;
    private Vector3 startMoveDirection = Vector3.zero;
    private float startDirectionSum = 0;

    [Header("Spawn")]
    public int floorsCountToSpawn = 0;
    public int minSpawnFloorNumber = 0;
    public int maxSpawnFloorNumber = 0;
    public int spawnFloorNumber { get; private set; } = 0;
    public float spawnMinTime = 0;
    public float spawnMaxTime = 0;
    [HideInInspector] public float spawnTime = 0.0f;

    private float checkPositionTime = 0.0f;
    private const float checkPositionRate = 3.0f;

    private const float despawnDistance = 100.0f;

    public const int limitSpawnFloorsCount = 10;

    private const int minDistanceToMoveAroundCity = 30;
    private const int maxDistanceToMoveAroundCity = 35;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }

    private void Start()
    {
        InitializeLootContainer();
    }

    public void Tick(float deltaTime)
    {
        Move(deltaTime);
        CheckPosition();
    }

    private void InitializeLootContainer()
    {
        checkPositionTime = Time.time;
        //spawnFloorNumber = (int)(transform.position.y / CityManager.floorHeight);
        //Vector3 direction = new Vector3(-transform.position.x, 0, -transform.position.z).normalized;
        //float directionOffset = Random.Range(30.0f, 30.0f);
        ////directionOffset *= direction.y > gameManager.windDirection.y ? 1 : -1;
        //Quaternion directionRotation = Quaternion.Euler(0, directionOffset, 0);
        //moveDirection = directionRotation * direction;

        for (int i = 0; i < possibleLoot.Count; i++)
        {
            int chance = UnityEngine.Random.Range(0, 100);

            if (chance <= possibleLoot[i].dropChance)
            {
                int itemAmount = (int)UnityEngine.Random.Range(possibleLoot[i].minAmount, possibleLoot[i].maxAmount);
                containedLoot.Add(new ItemInstance(possibleLoot[i].itemData, itemAmount));
            }
        }

        startMoveDirection = moveDirection;
        startDirectionSum = startMoveDirection.x + startMoveDirection.y + startMoveDirection.z;
    }

    private void Move(float deltaTime)
    {
        if (isMovable)
        {
            Vector3 crossDirection = Vector3.Cross(moveDirection, new Vector3(-transform.position.x, 0, -transform.position.z).normalized);

            float distanceToIsland = transform.position.magnitude;
            if (distanceToIsland <= maxDistanceToMoveAroundCity)
            {
                float alpha = 1 - ((distanceToIsland - minDistanceToMoveAroundCity) / (maxDistanceToMoveAroundCity - minDistanceToMoveAroundCity));
                alpha = math.clamp(alpha, 0, 1);

                float angleOffset = (crossDirection.y >= 0 ? 90 : -90) * alpha;
                Quaternion rotation = Quaternion.Euler(0, -angleOffset, 0);

                moveDirection = rotation * startMoveDirection;
            }
            else
            {
                Vector3 currentMoveDirection = -transform.position.normalized;

                float dot = Vector3.Dot(currentMoveDirection, startMoveDirection.normalized);

                //Debug.Log(dot);

                // если сильно отклонился (например, более 60°) и далеко от острова
                if (dot < 0.9f)
                {
                    moveDirection = Vector3.Lerp(moveDirection, startMoveDirection, deltaTime * 10.5f);
                }
            }

            transform.position += moveDirection * moveSpeed * deltaTime;
        }
    }

    private void CheckPosition()
    {
        float distance = Vector3.Distance(Vector3.zero, transform.position);

        if (distance > LootManager.lootContainersSpawnDistance + despawnDistance)
        {
            Destroy(this.gameObject);
        }
    }

    public List<ItemInstance> TakeItems()
    {
        Destroy(gameObject);
        List<ItemInstance> loot = containedLoot;
        return loot;
    }

    public List<ItemInstance> GetContainedLoot()
    {
        return containedLoot;
    }
}
