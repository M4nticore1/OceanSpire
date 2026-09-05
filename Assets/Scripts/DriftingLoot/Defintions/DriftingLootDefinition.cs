using UnityEngine;

public enum DriftingLootId
{
    Wood,
    Stone,
    Scrap,
    Plastic,
    Barrel,
    FlyingBox,
    WoodCluster
}

[CreateAssetMenu(fileName = "DriftingLootDefinition", menuName = "Drifting Loot/Definition")]
public abstract class DriftingLootDefinition : ScriptableObject
{
    [SerializeField] private DriftingLootId id = DriftingLootId.Wood;
    public DriftingLootId Id => id;

    [Header("Moving")]
    [SerializeField] private float movementSpeed = 0.4f;
    public float MovementSpeed => movementSpeed;

    [SerializeField] private float stopMovingSpeed = 10f;
    public float StopMovingSpeed => stopMovingSpeed;

    [Header("Spawn Time")]
    [SerializeField] private int minSpawnTime = 0;
    public int MinSpawnTime => minSpawnTime;

    [SerializeField] private int maxSpawnTime = 0;
    public int MaxSpawnTime => maxSpawnTime;

    [SerializeField] private GameObject[] meshes;
    public GameObject[] Meshes => meshes;
}