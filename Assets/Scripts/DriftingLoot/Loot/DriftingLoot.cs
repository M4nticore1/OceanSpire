using System;
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

    [SerializeField] private Movement movement;
    public Movement Movement => movement;

    [SerializeField] private Transform meshSpawnTransform;

    public int MeshId { get; private set; } = 0;
    public GameObject SpawnedMesh { get; private set; }

    public bool IsClickable { get; private set; } = true;

    public event Action OnClicked;

    protected virtual void OnEnable()
    {
        movement.OnReachedDestination += OnReachedDestination;
    }

    protected virtual void OnDisable()
    {
        movement.OnReachedDestination -= OnReachedDestination;
    }

    public void Init(DriftingLootData driftingLootData)
    {
        OnInit(driftingLootData);
    }

    protected virtual void OnInit(DriftingLootData driftingLootData)
    {
        if (driftingLootData == null) {
            Debug.LogError("driftingLootData is not valid");
            return;
        }

        CreateMesh(driftingLootData);
        UpdateDestination();
        //UpdateMovementDirection();
    }

    public virtual void Tick(float deltaTime)
    {
        //Move(deltaTime);
        //TryDestroy();
    }

    public abstract DriftingLootData CreateData();

    public abstract DriftingLootData CreateRandomData();

    public virtual bool ShouldClick()
    {
        if (!IsClickable) return false;

        return true;
    }

    protected virtual void OnClick()
    {

    }

    public void StartMoving()
    {
        UpdateDestination();
    }

    public void StopMoving()
    {
        movement.TryStopMoving();
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

    private void CreateMesh(DriftingLootData driftingLootData)
    {
        if (definition.Meshes.Length <= 0) return;

        var meshId = driftingLootData.MeshId;
        meshId = meshId % definition.Meshes.Length;
        MeshId = meshId;

        var meshPrefab = definition.Meshes[meshId];
        if (!meshPrefab) return;

        SpawnedMesh = Instantiate(meshPrefab, meshSpawnTransform);

        var rotation = Quaternion.Euler(driftingLootData.MeshRotation.Vector3());
        SpawnedMesh.transform.rotation = rotation;
    }

    private void UpdateDestination()
    {
        var windDir = WindManager.Instance.WindDirection;
        var dir = new Vector3(windDir.x, 0, windDir.z).normalized;
        var destination = WorldUtils.GetBorderPosition(dir);
        movement.TryMoveTo(destination);
    }

    private void OnReachedDestination()
    {
        Destroy(gameObject);
    }
}