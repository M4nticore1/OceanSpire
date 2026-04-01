using System.Collections.Generic;
using UnityEngine;

public class WanderersManager : MonoBehaviour
{
    public static WanderersManager instance;

    [Header("Prefabs")]
    [SerializeField] private Human humanPrefab;
    [SerializeField] private Boat boatPrefab;

    [Header("Cooldown")]
    [SerializeField] private float minWandererSpawnCooldown = 10;
    [SerializeField] private float maxWandererSpawnCooldown = 10;
    private float currentWandererSpawnCooldown = 0;
    private float currentWandererSpawnTime = 0;

    [Header("Positions")]
    [SerializeField] private BoatDockPoint[] waitingDockPoints;

    private List<Human> spawnedWanderers = new List<Human>();
    private List<Boat> spawnedBoats = new List<Boat>();

    private void Awake()
    {
        if (instance) {
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

        currentWandererSpawnTime += Time.deltaTime;
        if (currentWandererSpawnTime <= currentWandererSpawnCooldown)return;

        SpawnWanderer();
        ResetTimeToSpawn();
        ApplyRandomCooldown();
    }

    private void SpawnWanderer()
    {
        Vector3 position = WorldUtils.GetRandomBorderPosition();
        Vector3 rotation = Quaternion.LookRotation(-position.normalized).eulerAngles;

        // Human
        int creatureId = (int)CreatureIdEnum.Human;
        HumanStatus status = HumanStatus.Wanderer;
        HumanEntry humanData = new HumanEntry(creatureId, status, position, rotation);
        Human human = CreatureFactory.CreateHuman(humanData);
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
        currentWandererSpawnTime = 0f;
    }

    private void ApplyRandomCooldown()
    {
        currentWandererSpawnCooldown = GetRandomCooldown();
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
            boat.SetState(BoatStateEnum.MovingToDock);
        }
    }

    private bool CanSpawn()
    {
        if (spawnedWanderers.Count >= waitingDockPoints.Length) return false;

        return true;
    }

    private float GetRandomCooldown()
    {
        float cooldown = Random.Range(minWandererSpawnCooldown, maxWandererSpawnCooldown);
        return cooldown;
    }
}