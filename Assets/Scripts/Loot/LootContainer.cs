using System.Collections.Generic;
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

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }

    private void Start()
    {
        InitializeLootContainer();
    }

    private void Update()
    {
        Move();
        CheckPosition();
    }

    private void InitializeLootContainer()
    {
        checkPositionTime = Time.time;
        spawnFloorNumber = (int)(transform.position.y / CityManager.floorHeight);

        for (int i = 0; i < possibleLoot.Count; i++)
        {
            int chance = Random.Range(0, 100);

            if (chance <= possibleLoot[i].dropChance)
            {
                int itemAmount = (int)Random.Range(possibleLoot[i].minAmount, possibleLoot[i].maxAmount);
                containedLoot.Add(new ItemInstance(possibleLoot[i].itemData, itemAmount, possibleLoot[i].maxAmount));
            }
        }
    }

    private void Move()
    {
        if (isMovable)
        {
            transform.position += new Vector3(gameManager.windDirection.x, 0, gameManager.windDirection.y) * gameManager.windSpeed * Time.deltaTime;
        }
    }

    private void CheckPosition()
    {
        if (Time.time > checkPositionTime + checkPositionRate)
        {
            float distance = Vector3.Distance(Vector3.zero, transform.position);

            if (distance > GameManager.lootContainersSpawnDistance + despawnDistance)
            {
                Destroy(this.gameObject);
            }
        }
    }

    public void TakeItems()
    {
        Destroy(gameObject);
    }

    public List<ItemInstance> GetContainedLoot()
    {
        return containedLoot;
    }
}
