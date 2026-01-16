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
    [field: SerializeField] public LootContainerData containerData { get; private set; } = null;
    private GameManager gameManager = null;

    [Header("Loot")]
    [SerializeField] private List<LootEntry> possibleLoot = new List<LootEntry>();
    private List<ItemInstance> containedLoot = new List<ItemInstance>();

    [Header("Moving")]
    [SerializeField] private bool isMovable = true;
    private bool isMoving = false;
    public TransportMethod currentTransportMethod = TransportMethod.Floating;
    [SerializeField] private float moveSpeed = 0.0f;
    private float currentMoveSpeedMultiplier = 1f;
    private const float stopMovingSpeed = 1f;

    [HideInInspector] public Vector3 moveDirection = Vector3.zero;
    private Vector3 startMoveDirection = Vector3.zero;
    private float windSpeed = 0;

    [Header("Spawn")]
    [SerializeField] private int floorsCountToSpawn = 0;
    public int FloorsCountToSpawn => floorsCountToSpawn;
    public int minSpawnFloorNumber = 0;
    public int maxSpawnFloorNumber = 0;
    public int currentFloorIndex { get; private set; } = 0;
    public float spawnMinTime = 0;
    public float spawnMaxTime = 0;

    private float checkPositionTime = 0.0f;
    private const float checkPositionRate = 3.0f;

    private const float despawnDistance = 100.0f;

    public const int limitSpawnFloorsCount = 10;

    private const int minDistanceToMoveAroundCity = 30;
    private const int maxDistanceToMoveAroundCity = 35;

    private const float checkPositionFrequency = 1.0f;
    private double lastCheckPositionTime = 0d;

    private bool isInitialized = false;

    public static System.Action<LootContainer> OnLootEntered;
    public static System.Action<LootContainer> OnLootExited;

    public void InitializeContainer(GameManager gameManager, int floorIndex)
    {
        this.gameManager = gameManager;
        checkPositionTime = Time.time;
        if (isMovable)
            isMoving = true;

        for (int i = 0; i < possibleLoot.Count; i++) {
            int chance = UnityEngine.Random.Range(0, 100);

            if (chance <= possibleLoot[i].dropChance) {
                int itemAmount = UnityEngine.Random.Range(possibleLoot[i].minAmount, possibleLoot[i].maxAmount);
                containedLoot.Add(new ItemInstance(possibleLoot[i].itemData, itemAmount));
            }
        }

        if (gameManager)
            this.moveDirection = new Vector3(gameManager.windDirection.x, 0, gameManager.windDirection.y).normalized;
        else {
            this.moveDirection = -transform.position.normalized;
            Debug.LogError("gameManager is NULL");
        }

        startMoveDirection = this.moveDirection;
        currentFloorIndex = floorIndex;
        if (floorIndex > 0)
            currentTransportMethod = TransportMethod.Flying;
        else
            currentTransportMethod = TransportMethod.Floating;

        isInitialized = true;
    }

    public void Tick(float deltaTime)
    {
        Move(deltaTime);
        CheckPosition();
    }

    private void Move(float deltaTime)
    {
        if (isMovable)
        {
            if (isMoving)
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
                    if (dot < 0.9f)
                        moveDirection = Vector3.Lerp(moveDirection, startMoveDirection, deltaTime * 10.5f);
                }
            }
            else
            {
                currentMoveSpeedMultiplier = math.lerp(currentMoveSpeedMultiplier, 0f, stopMovingSpeed);
            }

            transform.position += moveDirection * currentMoveSpeedMultiplier * deltaTime;
        }
    }

    private void CheckPosition()
    {
        if (Time.timeAsDouble > lastCheckPositionTime + checkPositionFrequency)
        {
            float distance = Vector3.Distance(Vector3.zero, transform.position);

            if (distance <= GameManager.triggerLootContainerRadius)
                OnLootEntered?.Invoke(this);
            else if (distance > LootManager.spawnDistance + despawnDistance)
                Destroy(this.gameObject);
            else if (distance > GameManager.triggerLootContainerRadius)
                OnLootExited?.Invoke(this);

            lastCheckPositionTime = Time.timeAsDouble;
        }
    }

    public void StartCollecting(float remainingWeight)
    {
        StopMoving();
    }

    private void StopMoving()
    {
        isMoving = false;
    }

    public List<ItemInstance> TakeItems(float? remainingWeight = null)
    {
        List<ItemInstance> loot = new List<ItemInstance>();

        if (remainingWeight != null)
        {
            bool isNeededToDestroy = true;
            for (int i = 0; i < containedLoot.Count; i++)
            {
                ItemInstance currentLoot = containedLoot[i];
                if (remainingWeight.Value < currentLoot.ItemData.Weight) {
                    isNeededToDestroy = false;
                    continue; }

                ItemData data = currentLoot.ItemData;
                int id = currentLoot.ItemData.ItemId;
                int containedAmount = currentLoot.Amount;

                int amountToCollect = (int)math.min(containedAmount, remainingWeight.Value / data.Weight);

                currentLoot.SubtractAmount(amountToCollect);

                ItemInstance newLoot = new ItemInstance(data, amountToCollect);
                loot.Add(newLoot);
            }

            if (isNeededToDestroy)
                Destroy(gameObject);
        }
        else
        {
            loot = containedLoot;
            Destroy(gameObject);
        }
        return loot;
    }

    public List<ItemInstance> GetContainedLoot()
    {
        return containedLoot;
    }
}
