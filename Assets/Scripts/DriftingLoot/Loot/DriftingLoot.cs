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
    public static event Action<DriftingLoot> OnLootClicked;

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

        if (!movement.CanAgentReachTarget(driftingLootData.Destination.Vector3())) {
            Debug.Log($"[{nameof(DriftingLoot)}] Drifting Loot {name} can't reach position {driftingLootData.Destination.Vector3()} from position {transform.position}");
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
        OnLootClicked?.Invoke(this);
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
        var currentPosition = new Vector3(transform.position.x, 0, transform.position.z);
        var targetPosition = new Vector3(movement.TargetPosition.x, 0, movement.TargetPosition.z);
        if (Vector3.Distance(currentPosition, targetPosition) > movement.NavAgent.stoppingDistance) return;

        Destroy(gameObject);
    }

    private void OnReachedDestination()
    {
        Destroy(gameObject);
    }
}