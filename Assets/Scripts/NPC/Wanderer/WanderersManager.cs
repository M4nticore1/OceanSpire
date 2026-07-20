using System;
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
    [SerializeField] private int minWandererSpawnCooldown = 1800;
    [SerializeField] private int maxWandererSpawnCooldown = 3600;

    [Header("Accept")]
    [SerializeField] private SpawnArea spawnArea;

    public long? NextWandererTime { get; private set; } = null;

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

    private void OnApplicationFocus(bool focus)
    {
        if (focus == false) return;

        if (TrySpawnAllWanderers()) {
            WarpAllWanderers();
        }

        UpdateNextWanderersTime();
    }

    private void Update()
    {
        Debug.Log(NextWandererTime != null ? NextWandererTime - DateTimeOffset.UtcNow.ToUnixTimeSeconds() : null);

        if (NextWandererTime == null) return;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (currentTime < NextWandererTime) return;

        if (!CanSpawnWanderer()) {
            NextWandererTime = null;
            return;
        }

        TrySpawnAllWanderers();
        UpdateNextWanderersTime();
    }

    public void Init()
    {
        var wanderersData = new WandererSystemData()
        {
            NextWandererTime = GetRandomNextWandererTime(),
        };

        Init(wanderersData);
    }

    public void Init(WandererSystemData wanderersData)
    {
        if (wanderersData == null) {
            Debug.LogError($"[{nameof(WanderersManager)}] Wanderers Data is not valid");
            Init();
            return;
        }

        NextWandererTime = wanderersData.NextWandererTime;

        TrySpawnAllWanderers();
        WarpAllWanderers();
        UpdateNextWanderersTime();
    }

    public long GetRandomNextWandererTime()
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var cooldown = UnityEngine.Random.Range(minWandererSpawnCooldown, maxWandererSpawnCooldown + 1);

        return currentTime + cooldown;
    }

    private bool TrySpawnAllWanderers()
    {
        if (NextWandererTime == null) return false;
        if (!CanSpawnWanderer()) return false;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        long simulatedTime = NextWandererTime.Value;
        bool spawnedAny = false;

        while (currentTime >= simulatedTime && CanSpawnWanderer()) {
            SpawnWanderer();
            spawnedAny = true;

            var cooldown = UnityEngine.Random.Range(minWandererSpawnCooldown, maxWandererSpawnCooldown + 1);
            simulatedTime += cooldown;
        }

        if (spawnedAny) {
            NextWandererTime = simulatedTime;
        }

        return spawnedAny;
    }

    private void SpawnWanderer()
    {
        var position = WorldUtils.GetRandomBorderPosition();
        var rotation = Quaternion.LookRotation(-position.normalized).eulerAngles;

        var boat = CreateBoat(position, rotation);
        if (!boat) {
            Debug.LogError($"[{nameof(WanderersManager)}] Boat creation failed, cancelling wanderer spawn.");
            return;
        }

        var human = CreateWanderer(position, rotation, boat.InstanceId.GetGuid());
        if (!human) {
            Debug.LogError($"[{nameof(WanderersManager)}] Wanderer creation failed.");
        }
    }

    private void UpdateNextWanderersTime()
    {
        if (!CanSpawnWanderer()) {
            NextWandererTime = null;
            return;
        }

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (NextWandererTime != null && NextWandererTime.Value > currentTime) {
            return;
        }

        if (NextWandererTime == null) {
            NextWandererTime = GetRandomNextWandererTime();
            return;
        }

        long timeDelta = NextWandererTime.Value - currentTime;
        var cooldown = UnityEngine.Random.Range(minWandererSpawnCooldown, maxWandererSpawnCooldown + 1);
        NextWandererTime = currentTime + cooldown + timeDelta;
    }

    private void WarpAllWanderers()
    {
        foreach (var wanderer in creaturesManager.Wanderers) {
            if (!wanderer) continue;

            var boat = wanderer.BoatRider.RidingBoat;
            if (!boat) continue;

            var dockPoint = boat.DockPoint;
            if (!dockPoint) continue;

            var position = dockPoint.DockTransform.position;
            var rotation = dockPoint.DockTransform.rotation;

            boat.transform.position = position;
            boat.transform.rotation = rotation;

            boat.Movement.NavAgent.Warp(position);
        }
    }

    private void UpdateDockPoints()
    {
        if (!creaturesManager) return;
        if (creaturesManager.Wanderers == null) return;
        if (!dockPointsManager) return;
        if (dockPointsManager.WandererDockPoints == null) return;

        var wanderers = creaturesManager.Wanderers;
        int dockIndex = 0;
        int maxDocks = dockPointsManager.WandererDockPoints.Length;

        for (int i = 0; i < wanderers.Count; i++) {
            if (dockIndex >= maxDocks) {
                Debug.LogWarning($"[{nameof(WanderersManager)}] More active wanderers than available dock points!");
                break;
            }

            var wanderer = wanderers[i];
            if (!wanderer) continue;

            if (wanderer.IsAccepted) continue;
            if (wanderer.IsRejected) continue;
            if (!wanderer.BoatRider) continue;

            var ridingBoat = wanderer.BoatRider.RidingBoat;
            if (!ridingBoat) continue;

            ridingBoat.SetDockPoint(dockPointsManager.WandererDockPoints[dockIndex]);
            dockIndex++;
        }
    }

    private void OnWandererAccepted(Human human)
    {
        human.transform.position = spawnArea.GetRandomSpawnPosition();

        UpdateDockPoints();
        UpdateNextWanderersTime();
    }

    private void OnWandererRejected(Human human)
    {
        UpdateDockPoints();
        UpdateNextWanderersTime();
    }

    private Human CreateWanderer(Vector3 position, Vector3 rotation, Guid boatInstanceId)
    {
        if (wandererPrefabs == null || wandererPrefabs.Length == 0) {
            Debug.LogError($"[{nameof(WanderersManager)}] No wanderer prefabs assigned!");
            return null;
        }

        var selectedPrefab = wandererPrefabs[UnityEngine.Random.Range(0, wandererPrefabs.Length)];
        var prefab = selectedPrefab as Human;
        if (!prefab) {
            Debug.LogError($"[{nameof(WanderersManager)}] Selected prefab is not a Human type!");
            return null;
        }

        var levelsCount = Mathf.Max(1, SkillsData.GetLevelsCountByGameStage());

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
            Skills = SkillsData.CreateByLevelsCount(levelsCount),
            SpawnPosition = new Vector3Data(position)
        };

        return CreatureFactory.CreateHuman(prefab, position, Quaternion.Euler(rotation), data);
    }

    private Boat CreateBoat(Vector3 position, Vector3 rotation)
    {
        var dockPoint = GetFirstAvailableDockPoint();
        if (!dockPoint) {
            Debug.LogError($"[{nameof(WanderersManager)}] No available dock point found for new boat!");
            return null;
        }

        if (!boatPrefab) {
            Debug.LogError($"[{nameof(WanderersManager)}] Boat prefab is missing!");
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

        return BoatFactory.CreateBoat(boatPrefab, position, Quaternion.Euler(rotation), boatData);
    }

    private BoatDockPoint GetFirstAvailableDockPoint()
    {
        if (!dockPointsManager) return null;
        if (dockPointsManager.WandererDockPoints == null) return null;

        int activeWanderersCount = 0;
        if (creaturesManager && creaturesManager.Wanderers != null) {
            for (int i = 0; i < creaturesManager.Wanderers.Count; i++) {
                var wanderer = creaturesManager.Wanderers[i];
                if (!wanderer) continue;
                if (wanderer.IsAccepted) continue;
                if (wanderer.IsRejected) continue;

                activeWanderersCount++;
            }
        }

        if (activeWanderersCount < dockPointsManager.WandererDockPoints.Length) {
            return dockPointsManager.WandererDockPoints[activeWanderersCount];
        }

        return null;
    }

    private bool CanSpawnWanderer()
    {
        if (!creaturesManager) return false;
        if (creaturesManager.Wanderers == null) return false;
        if (!dockPointsManager) return false;
        if (dockPointsManager.WandererDockPoints == null) return false;

        int activeWanderersCount = 0;
        for (int i = 0; i < creaturesManager.Wanderers.Count; i++) {
            var wanderer = creaturesManager.Wanderers[i];
            if (!wanderer) continue;
            if (wanderer.IsAccepted) continue;
            if (wanderer.IsRejected) continue;

            activeWanderersCount++;
        }

        if (activeWanderersCount >= dockPointsManager.WandererDockPoints.Length) return false;

        return true;
    }
}