using System;
using System.Linq;
using UnityEngine;

public class WanderersManager : MonoBehaviour
{
    public static WanderersManager Instance;

    [Header("Main")]
    [SerializeField] private CreaturesManager creaturesManager;
    [SerializeField] private DockPointsManager dockPointsManager;
    [SerializeField] private CreaturesList creaturesList;
    [SerializeField] private BoatsList boatsList;
    [SerializeField] private HumanNamesList humanNamesList;

    [Header("Prefabs")]
    [SerializeField] private Creature[] wandererPrefabs;
    [SerializeField] private Boat boatPrefab;

    [Header("Cooldown")]
    [SerializeField] private float minWandererSpawnCooldown = 10;
    [SerializeField] private float maxWandererSpawnCooldown = 10;

    public float CurrentWandererSpawnCooldown { get; private set; } = 0;
    public float CurrentWandererSpawnTime { get; private set; } = 0;

    private void Awake()
    {
        if (Instance) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        WandererAdmissionSystem.OnWandererAccepted += OnWandererAccepted;
        WandererAdmissionSystem.OnWandererRejected += OnWandererRejected;
    }

    private void OnDisable()
    {
        WandererAdmissionSystem.OnWandererAccepted -= OnWandererAccepted;
        WandererAdmissionSystem.OnWandererRejected -= OnWandererRejected;
    }

    private void Start()
    {
        ApplyRandomCooldown();
    }

    public void Init(WanderersData wanderersData)
    {
        CurrentWandererSpawnCooldown = wanderersData.Cooldown;
        CurrentWandererSpawnTime = wanderersData.TimeSinceLastSpawn;
    }

    private void Update()
    {
        if (!CanSpawn()) return;

        CurrentWandererSpawnTime += Time.deltaTime;
        if (CurrentWandererSpawnTime <= CurrentWandererSpawnCooldown)return;

        SpawnWanderer();
        ResetTimeToSpawn();
        ApplyRandomCooldown();
    }

    public float CalculateRandomCooldown()
    {
        float cooldown = UnityEngine.Random.Range(minWandererSpawnCooldown, maxWandererSpawnCooldown);
        return cooldown;
    }

    private void SpawnWanderer()
    {
        Vector3 position = WorldUtils.GetRandomBorderPosition();
        Vector3 rotation = Quaternion.LookRotation(-position.normalized).eulerAngles;

        var boat = CreateBoat(position, rotation);
        if (!boat) {
            Debug.LogError("boat is not valid");
            return;
        }

        var human = CreateWanderer(position, rotation, boat.InstanceId.GetGuid());
    }

    private void ResetTimeToSpawn()
    {
        CurrentWandererSpawnTime = 0f;
    }

    private void ApplyRandomCooldown()
    {
        CurrentWandererSpawnCooldown = CalculateRandomCooldown();
    }

    private void OnWandererAccepted(Human human)
    {
        UpdateDockPoints();
    }

    private void OnWandererRejected(Human human)
    {
        UpdateDockPoints();
    }

    private void UpdateDockPoints()
    {
        var wanderers = creaturesManager.Wanderers;
        int dockIndex = 0;

        for (int i = 0; i < wanderers.Count; i++) {
            var wanderer = wanderers[i];

            if (!wanderer) {
                Debug.LogError($"Wanderer not found by index {i}");
                continue;
            }

            if (wanderer.IsAccepted) continue;
            if (wanderer.IsRejected) continue;

            var ridingBoat = wanderer.BoatRider.RidingBoat;
            if (!ridingBoat) {
                Debug.LogError($"Riding Boat not found at {wanderer.BoatRider.name}");
                continue;
            }

            ridingBoat.SetDockPoint(dockPointsManager.WandererDockPoints[dockIndex]);
            dockIndex++;
        }
    }

    private Human CreateWanderer(Vector3 position, Vector3 rotation, Guid boatInstanceId)
    {
        var prefab = wandererPrefabs[UnityEngine.Random.Range(0, wandererPrefabs.Length)] as Human;
        var levelsCount = Mathf.Max(1, SkillsFactory.GetLevelsCount());

        var data = new WandererData()
        {
            Id = prefab.Definition.CreatureId,
            Position = new Vector3Data(position),
            Rotation = new Vector3Data(rotation),

            Name = new NameData()
            {
                FirstNameId = prefab.GenderComponent.IsMale ? humanNamesList.GetRandomMaleFirstNameId() : humanNamesList.GetRandomFemaleFirstNameId(),
                LastNameId = prefab.GenderComponent.IsMale ? humanNamesList.GetRandomMaleLastNameId() : humanNamesList.GetRandomFemaleLastNameId(),
            },

            Health = new HealthData()
            {
                CurrentHealth = prefab.HealthComponent.MaxHealth
            },

            BoatRider = new BoatRiderData()
            {
                RidingBoatInstanceId = boatInstanceId,
            },

            Weapon = EquipmentData.Default(),
            Skills = SkillsFactory.CreateRandomSkillsData(levelsCount),
            SpawnPosition = new Vector3Data(position)
        };

        var human = CreatureFactory.CreateHuman(prefab, position, Quaternion.Euler(rotation), data);

        return human;
    }

    private Boat CreateBoat(Vector3 position, Vector3 rotation)
    {
        var dockPoint = GetDockPoint();
        if (!dockPoint) {
            Debug.LogError("dockPoint is not valid");
            return null;
        }

        var boatData = new BoatData()
        {
            Id = boatPrefab.Definition.BoatId,
            Position = new Vector3Data(position),
            Rotation = new Vector3Data(rotation),
            DockInstanceId = dockPoint.InstanceId.GetGuid(),
            Status = HumanStatusEnum.Wanderer
        };

        var boat = BoatFactory.CreateBoat(boatPrefab, position, Quaternion.Euler(rotation), boatData);

        return boat;
    }

    private BoatDockPoint GetDockPoint()
    {
        return dockPointsManager.WandererDockPoints[creaturesManager.Wanderers.Count()];
    }

    private bool CanSpawn()
    {
        if (creaturesManager.Wanderers.Count >= dockPointsManager.WandererDockPoints.Length) return false;

        return true;
    }
}