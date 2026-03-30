using System.Collections.Generic;
using UnityEngine;

public class WanderersManager : MonoBehaviour
{
    public static WanderersManager instance;

    // Prefabs
    [SerializeField] private Human humanPrefab;
    [SerializeField] private Boat boatPrefab;

    // Time
    [SerializeField] private float minCooldownToSpawnWanderer = 10;
    [SerializeField] private float maxCooldownToSpawnWanderer = 10;
    private float currentCooldownToSpawnWanderer = 0;
    private float currentTimeToSpawnWanderer = 0;

    // Positions
    [SerializeField] private BoatDockPoint[] waitingDockPoints;
    [SerializeField] private float spawnDistance = 200f;

    // Spawned
    private List<Human> spawnedWanderers = new List<Human>();
    private List<Boat> spawnedBoats = new List<Boat>();

    private void Awake()
    {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnEnable()
    {
        Human.onWandererAccepted += OnWandererAccepted;
        Human.onWandererRejected += OnWandererRejected;
    }

    private void OnDisable()
    {
        Human.onWandererAccepted -= OnWandererAccepted;
        Human.onWandererRejected -= OnWandererRejected;
    }

    private void Start()
    {
        ApplyRandomCooldown();
    }

    private void Update()
    {
        if (!CanSpawn()) return;

        currentTimeToSpawnWanderer += Time.deltaTime;
        if (currentTimeToSpawnWanderer <= currentCooldownToSpawnWanderer)return;

        SpawnWanderer();
        ResetTimeToSpawn();
        ApplyRandomCooldown();
    }

    public Vector3 GetRandomBorderPosition()
    {
        Vector3 dir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        dir.Normalize();
        Vector3 position = dir * spawnDistance;
        return position;
    }

    private void SpawnWanderer()
    {
        Vector3 position = GetRandomBorderPosition();
        Vector3 rotation = Quaternion.LookRotation(-position.normalized).eulerAngles;

        // Human
        int creatureId = (int)CreatureIdEnum.Human;
        HumanStatus status = HumanStatus.Wanderer;
        HumanEntry humanData = new HumanEntry(creatureId, status, position, rotation);
        Human human = EntityFactory.CreateHuman(humanData);
        spawnedWanderers.Add(human);

        // Boat
        int boatId = (int)BoatIdEnum.BasicBoat;
        float boatHealth = BoatsList.Instance.boats[boatId].Health.MaxHealth;
        BoatEntry boatData = new BoatEntry(boatId, position, rotation, boatHealth);
        Boat boat = BoatFactory.CreateBoat(boatData);
        boat.SetDockPoint(waitingDockPoints[spawnedBoats.Count]);
        spawnedBoats.Add(boat);

        human.BoatRider.EnterBoat(boat);
    }

    private void ResetTimeToSpawn()
    {
        currentTimeToSpawnWanderer = 0f;
    }

    private void ApplyRandomCooldown()
    {
        currentCooldownToSpawnWanderer = GetRandomCooldown();
    }

    private void OnWandererAccepted(Human human)
    {
        RemoveWanderer(human);
        AdjustWandererPositions();
    }

    private void OnWandererRejected(Human human)
    {
        RemoveWanderer(human);
        AdjustWandererPositions();
    }

    private void RemoveWanderer(Human human)
    {
        int index = 0;
        foreach (Human wanderer in spawnedWanderers) {
            if (wanderer == human) {
                spawnedWanderers.RemoveAt(index);
                spawnedBoats.RemoveAt(index);
                break;
            }
            index++;
        }
    }

    private void AdjustWandererPositions()
    {
        for (int i = 0; i < spawnedWanderers.Count; i++) {
            Human wanderer = spawnedWanderers[i];

            Boat boat = wanderer.BoatRider.currentBoat;
            boat.SetDockPoint(waitingDockPoints[i]);
            boat.SetState(BoatStateEnum.ReturningToDock);
        }
    }

    private bool CanSpawn()
    {
        if (spawnedWanderers.Count >= waitingDockPoints.Length) return false;

        return true;
    }

    private float GetRandomCooldown()
    {
        float cooldown = Random.Range(minCooldownToSpawnWanderer, maxCooldownToSpawnWanderer);
        return cooldown;
    }
}