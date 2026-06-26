using System;
using UnityEngine;

public class FlyingDriftingLoot : DriftingLoot, IClickable
{
    public FlyingDriftingLootDefinition FlyingDefinition => Definition as FlyingDriftingLootDefinition;

    [Header("Flying")]
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private Rigidbody rigidBody;

    [SerializeField] private GameObject balloons;
    [SerializeField] private ParticleSystem demolishParticlesPrefab;
    [SerializeField] private float fallingSpeedForce = 100f;
    [SerializeField] private float fallingDemolishHeightOffset = 0;

    public bool IsFalling { get; private set; } = false;
    private float targetFallingDemolishHeight = 0f;

    public static event Action<FlyingDriftingLoot> OnFlyingLootStartedFalling;
    public static event Action<FlyingDriftingLoot> onContainerLanded;

    protected override void OnDisable()
    {
        base.OnDisable();

        DriftingLootManager.Instance.UnregisterFlyingDriftingLoot(this);
    }

    protected override void OnInit(DriftingLootData driftingLootData)
    {
        base.OnInit(driftingLootData);

        var flyingDriftingLootData = driftingLootData as FlyingDriftingLootData;

        if (flyingDriftingLootData == null) {
            Debug.Log($"flyingDriftingLootData is not valid");
            Destroy(gameObject);
            return;
        }

        Movement.NavAgent.baseOffset = flyingDriftingLootData.Position.Y;

        TrySetFalling(flyingDriftingLootData.IsFalling);

        targetFallingDemolishHeight = boxCollider.size.y / 2 - boxCollider.center.y - fallingDemolishHeightOffset;
        DriftingLootManager.Instance.RegisterFlyingDriftingLoot(this);
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        if (!IsFalling) return;

        rigidBody.AddForce(Vector3.down * fallingSpeedForce, ForceMode.Acceleration);

        if (transform.position.y > targetFallingDemolishHeight) return;

        DemolishFlyingContainer();
    }

    public override DriftingLootData CreateData()
    {
        return FlyingDriftingLootData.Create(this);
    }

    public override DriftingLootData CreateRandomData()
    {
        return new FlyingDriftingLootData()
        {
            Id = (int)Definition.Id,
            MeshId = UnityEngine.Random.Range(0, Definition.Meshes.Length),
        };
    }

    protected override void OnClick()
    {
        base.OnClick();

        TrySetFalling(true);
    }

    private void DemolishFlyingContainer()
    {
        int index = UnityEngine.Random.Range(0, FlyingDefinition.DemolishDriftingLootTable.Length);
        if (FlyingDefinition.DemolishDriftingLootTable.Length <= index) {
            Debug.Log($"Length of DemolishDriftingLootTable is more than {index}");
            return;
        }

        var prefab = FlyingDefinition.DemolishDriftingLootTable[index];
        if (!prefab) {
            Debug.Log($"Demolish Drifting Loot prefab not found by index {index}");
            return;
        }

        var position = new Vector3(transform.position.x, 0, transform.position.z);
        var rotation = transform.rotation;

        var driftingLootData = new SwimmingDriftingLootData()
        {
            Id = (int)prefab.Definition.Id,
            Position = new Vector3Data(position),
            Rotation = new Vector3Data(rotation.eulerAngles),
        };

        var container = DriftingLootFactory.CreateDriftingLoot(prefab, driftingLootData);
        Instantiate(demolishParticlesPrefab, position, rotation);

        Destroy(gameObject);
        onContainerLanded?.Invoke(this);
    }

    private void TrySetFalling(bool value)
    {
        if (value == IsFalling) return;

        IsFalling = value;
        boxCollider.enabled = !value;
        rigidBody.useGravity = value;
        rigidBody.isKinematic = !value;
        Movement.NavAgent.enabled = !value;

        if (value) {
            Destroy(balloons.gameObject);
            OnFlyingLootStartedFalling?.Invoke(this);
        }
    }
}