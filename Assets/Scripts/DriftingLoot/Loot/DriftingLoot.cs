using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LootTableData
{
    public ItemDefinition itemData;
    public float dropChance;
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

    public Vector3 Destination { get; private set; } = Vector3.zero;

    public int MeshId { get; private set; } = 0;
    public GameObject SpawnedMesh { get; private set; }

    public bool IsClickable { get; private set; } = true;

    public event Action OnClicked;
    public static event Action<DriftingLoot> OnGlobalClicked;

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
        if (driftingLootData != null) {
            OnInit(driftingLootData);
        }
        else {
            Debug.LogError($"[{nameof(DriftingLoot)}] Drifting Loot Data is not valid");
            OnInit();
        }
    }

    protected abstract void OnInit();

    protected virtual void OnInit(DriftingLootData driftingLootData)
    {
        if (driftingLootData == null) {
            Debug.LogError("driftingLootData is not valid");
            return;
        }

        if (driftingLootData.Destination.Vector3() == Vector3.zero) {
            Destroy(gameObject);
            return;
        }

        if (!movement.CanAgentReachTarget(driftingLootData.Destination.Vector3())) {
            Destroy(gameObject);
            return;
        }

        instanceId.SetGuid(driftingLootData.InstanceId);
        movement.NavAgent.Warp(transform.position);

        Destination = driftingLootData.Destination.Vector3();
        movement.TryMoveTo(Destination);

        CreateMesh(driftingLootData);
    }

    public virtual void Tick(float deltaTime)
    {
        TryDestroy();
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
        movement.TryMoveTo(Destination);
    }

    public void StopMoving()
    {
        movement.TryStopMoving();
    }

    public void Click()
    {
        OnClick();

        OnClicked?.Invoke();
        OnGlobalClicked?.Invoke(this);
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

    private void TryDestroy()
    {
        if (!movement.IsDestinationReached()) return;

        Destroy(gameObject);
    }

    private void OnReachedDestination()
    {
        Destroy(gameObject);
    }
}