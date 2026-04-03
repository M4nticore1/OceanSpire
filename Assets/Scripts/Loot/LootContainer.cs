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

public class LootContainer : MonoBehaviour, IClickable
{
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private Rigidbody rigidBody;

    [Header("Loot")]
    [SerializeField] private List<LootEntry> possibleLoot = new List<LootEntry>();
    [SerializeField] private GameObject[] meshes;
    private List<ItemInstance> containedLoot = new List<ItemInstance>();

    [Header("Moving")]
    [SerializeField] private bool isMovable = true;

    private bool isMoving = false;
    private const float MoveSpeed = 0.4f;
    private float currentMoveSpeedMultiplier = 1f;
    private const float stopMovingSpeed = 10f;

    public Vector3 moveDirection { get; private set; } = Vector3.zero;
    private Vector3 startMoveDirection = Vector3.zero;

    [Header("Flying")]
    [SerializeField] private bool isFlying = false;
    public bool IsFlying => isFlying;

    [SerializeField] private GameObject balloons;
    [SerializeField] private LootContainer[] demolishPossibleContainers;
    [SerializeField] private ParticleSystem demolishParticlesPrefab;
    [SerializeField] private float fallingSpeedForce = 100f;
    [SerializeField] private float fallingDemolishHeightOffset = 0;

    private bool isFalling = false;
    private float targetFallingDemolishHeight = 0f;

    [Header("Spawn Time")]
    [SerializeField] private float spawnMinTime = 0;
    public float SpawnMinTime => spawnMinTime;

    [SerializeField] private float spawnMaxTime = 0;
    public float SpawnMaxTime => spawnMaxTime;

    [Header("Spawn Floor")]
    [SerializeField] private int floorsCountToSpawn = 0;
    public int FloorsCountToSpawn => floorsCountToSpawn;

    [SerializeField] private int minSpawnFloorNumber = 0;
    public int MinSpawnFloorNumber => minSpawnFloorNumber;

    [SerializeField] private int maxSpawnFloorNumber = 0;
    public int MaxSpawnFloorNumber => maxSpawnFloorNumber;

    public int spawnFloorIndex { get; private set; } = 0;

    private const float despawnDistance = 100.0f;

    public const int limitSpawnFloorsCount = 10;

    private const int minDistanceToMoveAroundCity = 30;
    private const int maxDistanceToMoveAroundCity = 35;

    private const float checkPositionFrequency = 1.0f;
    private double lastCheckPositionTime = 0d;

    public void Init(int floorIndex)
    {
        if (isMovable) {
            isMoving = true;
        }

        for (int i = 0; i < possibleLoot.Count; i++) {
            int chance = UnityEngine.Random.Range(0, 100);

            if (chance <= possibleLoot[i].dropChance) {
                int itemAmount = UnityEngine.Random.Range(possibleLoot[i].minAmount, possibleLoot[i].maxAmount);
                containedLoot.Add(new ItemInstance(possibleLoot[i].itemData, itemAmount));
            }
        }

        Vector3 direction = WindManager.Instance.windDirection;
        moveDirection = new Vector3(direction.x, 0, direction.z);
        startMoveDirection = moveDirection;
        spawnFloorIndex = floorIndex;

        if (isFlying) {
            targetFallingDemolishHeight = boxCollider.size.y / 2 - boxCollider.center.y - fallingDemolishHeightOffset;
        }

        CreateMesh();
    }

    public void Tick(float deltaTime)
    {
        Move(deltaTime);
        CheckPosition();

        if (isFalling) {
            rigidBody.AddForce(Vector3.down * fallingSpeedForce, ForceMode.Acceleration);

            if (transform.position.y <= targetFallingDemolishHeight) {
                DemolishFlyingContainer();
            }
        }
    }

    public void StartMoving()
    {
        isMoving = true;
    }

    public void StopMoving()
    {
        isMoving = false;
    }

    public void Click()
    {
        boxCollider.enabled = false;
        rigidBody.isKinematic = false;

        Destroy(balloons.gameObject);

        isFalling = true;
    }

    public bool CanClick()
    {
        return isFlying;
    }

    private void Move(float deltaTime)
    {
        if (!isMovable) return;

        if (isMoving) {
            if (currentMoveSpeedMultiplier < 1f) {
                currentMoveSpeedMultiplier = math.lerp(currentMoveSpeedMultiplier, 1f, stopMovingSpeed * Time.deltaTime);
            }

            Vector3 crossDirection = Vector3.Cross(moveDirection, new Vector3(-transform.position.x, 0, -transform.position.z).normalized);
            float distanceToIsland = new Vector3(transform.position.x, 0, transform.position.z).magnitude;

            if (distanceToIsland > maxDistanceToMoveAroundCity) {
                Vector3 currentMoveDirection = -transform.position.normalized;
            }
            else {
                float alpha = 1 - ((distanceToIsland - minDistanceToMoveAroundCity) / (maxDistanceToMoveAroundCity - minDistanceToMoveAroundCity));
                alpha = math.clamp(alpha, 0, 1);

                float angleOffset = (crossDirection.y >= 0 ? 90 : -90) * alpha;
                Quaternion rotation = Quaternion.Euler(0, -angleOffset, 0);

                moveDirection = rotation * startMoveDirection;
            }
        }
        else {
            currentMoveSpeedMultiplier = math.lerp(currentMoveSpeedMultiplier, 0f, stopMovingSpeed * Time.deltaTime);
        }

        transform.position += moveDirection * currentMoveSpeedMultiplier * MoveSpeed * deltaTime;
    }

    private void CheckPosition()
    {
        if (Time.timeAsDouble > lastCheckPositionTime + checkPositionFrequency)
        {
            float distance = Vector3.Distance(Vector3.zero, transform.position);
            
            if (distance > LootManager.spawnDistance + despawnDistance)
                Destroy(gameObject);

            lastCheckPositionTime = Time.timeAsDouble;
        }
    }

    private void CreateMesh()
    {
        int index = UnityEngine.Random.Range(0, meshes.Length);
        Instantiate(meshes[index], transform);
    }

    private void DemolishFlyingContainer()
    {
        int index = UnityEngine.Random.Range(0, demolishPossibleContainers.Length);

        Vector3 position = new Vector3(transform.position.x, 0, transform.position.z);
        Quaternion rotation = transform.rotation;
        LootContainer container = Instantiate(demolishPossibleContainers[index], position, rotation);
        container.Init(0);

        Instantiate(demolishParticlesPrefab, position, rotation);

        LootManager.instance.RegisterLootContainer(container);
        Destroy(gameObject);
    }

    public List<ItemInstance> TakeItems(float? remainingWeight = null)
    {
        List<ItemInstance> loot = new List<ItemInstance>();

        if (remainingWeight != null)
        {
            for (int i = 0; i < containedLoot.Count; i++)
            {
                ItemInstance currentLoot = containedLoot[i];
                if (remainingWeight.Value < currentLoot.ItemData.Weight) continue;

                ItemData data = currentLoot.ItemData;
                int id = currentLoot.ItemData.ItemId;
                int containedAmount = currentLoot.Amount;

                int amountToCollect = (int)math.min(containedAmount, remainingWeight.Value / data.Weight);

                currentLoot.RemoveAmount(amountToCollect);

                ItemInstance newLoot = new ItemInstance(data, amountToCollect);
                loot.Add(newLoot);
            }
        }
        else
        {
            loot = containedLoot;
        }

        Destroy(gameObject);
        return loot;
    }

    public List<ItemInstance> GetContainedLoot()
    {
        return containedLoot;
    }
}