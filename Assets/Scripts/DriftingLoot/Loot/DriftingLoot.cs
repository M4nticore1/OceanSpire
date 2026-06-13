using System;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct LootTableData
{
    public ItemDefinition itemData;
    public int dropChance;
    public int minAmount;
    public int maxAmount;
}

public abstract class DriftingLoot : MonoBehaviour, IClickable
{
    [SerializeField] private DriftingLootDefinition definition;
    public DriftingLootDefinition Definition => definition;

    [SerializeField] private InstanceId instanceId;
    public InstanceId InstanceId => instanceId;

    [SerializeField] private Transform meshSpawnTransform;

    private float currentMoveSpeedMultiplier = 1f;

    public Vector3 moveDirection { get; private set; } = Vector3.zero;
    private Vector3 startMoveDirection = Vector3.zero;

    private const float despawnDistance = 100.0f;

    public const int limitSpawnFloorsCount = 10;

    private const int minDistanceToMoveAroundCity = 30;
    private const int maxDistanceToMoveAroundCity = 35;

    private const float checkPositionFrequency = 1.0f;
    private double lastCheckPositionTime = 0d;

    public bool IsMoving { get; private set; } = true;
    public int MeshId { get; private set; } = 0;
    public bool IsClickable { get; private set; } = true;

    public event Action OnClicked;

    protected virtual void OnEnable()
    {
        
    }

    protected virtual void OnDisable()
    {

    }

    public void Init(DriftingLootData driftingLootData)
    {
        OnInit(driftingLootData);
    }

    public virtual void Tick(float deltaTime)
    {
        Move(deltaTime);
        CheckPosition();
    }

    public void StartMoving()
    {
        IsMoving = true;
    }

    public void StopMoving()
    {
        IsMoving = false;
    }

    public void Click()
    {
        OnClick();

        OnClicked?.Invoke();
    }

    public void SetClickable(bool value)
    {
        IsClickable = value;
    }

    public abstract DriftingLootData CreateData();

    public abstract DriftingLootData CreateRandomData();

    public virtual bool ShouldClick()
    {
        if (!IsClickable) return false;

        return true;
    }

    protected virtual void OnInit(DriftingLootData driftingLootData)
    {
        if (driftingLootData == null) return;

        CreateMesh(driftingLootData.MeshId);
        UpdateMovementDirection();
    }

    protected virtual void OnClick()
    {

    }

    private void Move(float deltaTime)
    {
        if (IsMoving) {
            if (currentMoveSpeedMultiplier < 1f) {
                currentMoveSpeedMultiplier = math.lerp(currentMoveSpeedMultiplier, 1f, definition.StopMovingSpeed * Time.deltaTime);
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
            currentMoveSpeedMultiplier = math.lerp(currentMoveSpeedMultiplier, 0f, definition.StopMovingSpeed * Time.deltaTime);
        }

        transform.position += moveDirection * currentMoveSpeedMultiplier * definition.MovementSpeed * deltaTime;
    }

    private void CheckPosition()
    {
        if (Time.timeAsDouble > lastCheckPositionTime + checkPositionFrequency)
        {
            float distance = Vector3.Distance(Vector3.zero, transform.position);
            
            if (distance > DriftingLootManager.spawnDistance + despawnDistance)
                Destroy(gameObject);

            lastCheckPositionTime = Time.timeAsDouble;
        }
    }

    private void CreateMesh(int id)
    {
        id = id % definition.Meshes.Length;
        MeshId = id;

        var mesh = definition.Meshes[id];
        if (!mesh) return;

        Instantiate(mesh, meshSpawnTransform);
    }

    private void UpdateMovementDirection()
    {
        Vector3 direction = WindManager.Instance.WindDirection;
        moveDirection = new Vector3(direction.x, 0, direction.z);
        startMoveDirection = moveDirection;
    }
}