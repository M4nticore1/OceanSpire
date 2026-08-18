using System;
using UnityEngine;

public class WanderersManager : MonoBehaviour
{
    public static WanderersManager Instance;

    [Header("Main")]
    [SerializeField] private WandererAdmissionManager wandererAdmissionManager;
    [SerializeField] private CreaturesManager creaturesManager;
    [SerializeField] private BoatDocksManager dockPointsManager;
    [SerializeField] private RadioStationsManager radioStationsManager;
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

    public long? NextWandererTime { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        wandererAdmissionManager.OnWandererAccepted += OnWandererAccepted;
        wandererAdmissionManager.OnWandererRejected += OnWandererRejected;
    }

    private void OnDisable()
    {
        wandererAdmissionManager.OnWandererAccepted -= OnWandererAccepted;
        wandererAdmissionManager.OnWandererRejected -= OnWandererRejected;
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus) {
            OnPlayerReturned();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause) {
            OnPlayerReturned();
        }
    }

    private void Update()
    {
        if (NextWandererTime == null) return;

        var currentTime = GetCurrentTime();
        var nextWandererTimeWithBonus = GetNextWandererTimeWithBonus();

        if (nextWandererTimeWithBonus == null) return;
        if (currentTime < nextWandererTimeWithBonus.Value) return;

        if (CanSpawnWanderer()) {
            TrySpawnAllWanderers();
        }
        else {
            NextWandererTime = null;
        }
    }

    public void Init()
    {
        var wanderersData = new WandererSystemData()
        {
            NextWandererTime = GetRandomNextWandererTime()
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
        var currentTime = GetCurrentTime();
        var cooldown = GetRandomWandererSpawnCooldown();

        return currentTime + cooldown;
    }

    public long? GetNextWandererTimeWithBonus()
    {
        if (NextWandererTime == null) return null;

        var currentTime = GetCurrentTime();
        var remainingTime = NextWandererTime.Value - currentTime;

        if (remainingTime <= 0) return currentTime;

        var bonus = GetWandererCooldownSpeedBonus();
        if (bonus <= 1f) return NextWandererTime;

        var speededRemainingTime = Mathf.CeilToInt((float)remainingTime / bonus);

        return currentTime + speededRemainingTime;
    }

    private bool TrySpawnAllWanderers()
    {
        if (NextWandererTime == null) return false;
        if (!CanSpawnWanderer()) return false;

        var currentTime = GetCurrentTime();
        bool spawnedAny = false;

        while (CanSpawnWanderer()) {
            var nextWandererTimeWithBonus = GetNextWandererTimeWithBonus();

            if (nextWandererTimeWithBonus == null) break;
            if (currentTime < nextWandererTimeWithBonus.Value) break;

            SpawnWanderer();
            spawnedAny = true;

            NextWandererTime = GetRandomNextWandererTime();
            currentTime = GetCurrentTime();
        }

        return spawnedAny;
    }

    private void SpawnWanderer()
    {
        var position = WorldUtils.GetRandomBorderPosition();
        var rotation = Quaternion.LookRotation(-position.normalized).eulerAngles;

        var boat = CreateBoat(position, rotation);
        if (boat == null) {
            Debug.LogError($"[{nameof(WanderersManager)}] Boat creation failed, cancelling wanderer spawn.");
            return;
        }

        var human = CreateWanderer(position, rotation, boat.InstanceId.GetGuid());
        if (human == null) {
            Debug.LogError($"[{nameof(WanderersManager)}] Wanderer creation failed.");
        }
    }

    private void UpdateNextWanderersTime()
    {
        if (!CanSpawnWanderer()) {
            NextWandererTime = null;
            return;
        }

        if (NextWandererTime == null) {
            NextWandererTime = GetRandomNextWandererTime();
            return;
        }

        var currentTime = GetCurrentTime();
        var nextWandererTimeWithBonus = GetNextWandererTimeWithBonus();

        if (nextWandererTimeWithBonus == null) return;
        if (nextWandererTimeWithBonus.Value > currentTime) return;

        TrySpawnAllWanderers();

        if (NextWandererTime == null) {
            NextWandererTime = GetRandomNextWandererTime();
        }
    }

    private void WarpAllWanderers()
    {
        foreach (var wanderer in creaturesManager.Wanderers) {
            if (wanderer == null) continue;

            var boat = wanderer.BoatRider.RidingBoat;
            if (boat == null) continue;

            var dockPoint = boat.DockPoint;
            if (dockPoint == null) continue;

            var position = dockPoint.DockTransform.position;
            var rotation = dockPoint.DockTransform.rotation;

            boat.transform.position = position;
            boat.transform.rotation = rotation;

            boat.Movement.NavAgent.Warp(position);
        }
    }

    private void UpdateDockPoints()
    {
        if (creaturesManager == null) return;
        if (creaturesManager.Wanderers == null) return;
        if (dockPointsManager == null) return;
        if (dockPointsManager.WandererDockPoints == null) return;

        var wanderers = creaturesManager.Wanderers;
        int dockIndex = 0;
        int maxDocks = dockPointsManager.WandererDockPoints.Count;

        for (int i = 0; i < wanderers.Count; i++) {
            if (dockIndex >= maxDocks) {
                Debug.LogWarning($"[{nameof(WanderersManager)}] More active wanderers than available dock points!");
                break;
            }

            var wanderer = wanderers[i];
            if (wanderer == null) continue;

            if (wanderer.IsAccepted) continue;
            if (wanderer.IsRejected) continue;
            if (wanderer.BoatRider == null) continue;

            var ridingBoat = wanderer.BoatRider.RidingBoat;
            if (ridingBoat == null) continue;

            ridingBoat.SetDockPoint(dockPointsManager.GetWandererBoatDock(dockIndex));
            dockIndex++;
        }
    }

    private void OnPlayerReturned()
    {
        if (TrySpawnAllWanderers()) {
            WarpAllWanderers();
        }

        UpdateNextWanderersTime();
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

        if (prefab == null) {
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

            Weapon = WeaponsDataFactory.CreateRandomData(WeaponsDataFactory.GetMinWeaponDamageId(), WeaponsDataFactory.GetMinWeaponDamageId()),
            Skills = SkillsData.CreateByLevelsCount(levelsCount),
            SpawnPosition = new Vector3Data(position)
        };

        return CreatureFactory.CreateHuman(prefab, data);
    }

    private Boat CreateBoat(Vector3 position, Vector3 rotation)
    {
        var dockPoint = GetFirstAvailableDockPoint();
        if (dockPoint == null) {
            Debug.LogError($"[{nameof(WanderersManager)}] No available dock point found for new boat!");
            return null;
        }

        if (boatPrefab == null) {
            Debug.LogError($"[{nameof(WanderersManager)}] Boat prefab is missing!");
            return null;
        }

        var boatData = new BoatData()
        {
            Id = boatPrefab.Definition.BoatId,
            InstanceId = Guid.NewGuid(),
            Position = new Vector3Data(position),
            Rotation = new Vector3Data(rotation),
            DockInstanceId = dockPoint.InstanceId.GetGuid(),
            Status = HumanStatusEnum.Wanderer
        };

        return BoatFactory.CreateBoat(boatPrefab, boatData);
    }

    private BoatDockPoint GetFirstAvailableDockPoint()
    {
        if (dockPointsManager == null) return null;
        if (dockPointsManager.WandererDockPoints == null) return null;

        int activeWanderersCount = 0;

        if (creaturesManager != null && creaturesManager.Wanderers != null) {
            for (int i = 0; i < creaturesManager.Wanderers.Count; i++) {
                var wanderer = creaturesManager.Wanderers[i];

                if (wanderer == null) continue;
                if (wanderer.IsAccepted) continue;
                if (wanderer.IsRejected) continue;

                activeWanderersCount++;
            }
        }

        return dockPointsManager.GetWandererBoatDock(activeWanderersCount);
    }

    private bool CanSpawnWanderer()
    {
        if (creaturesManager == null) return false;
        if (creaturesManager.Wanderers == null) return false;
        if (dockPointsManager == null) return false;
        if (dockPointsManager.WandererDockPoints == null) return false;

        int activeWanderersCount = 0;

        for (int i = 0; i < creaturesManager.Wanderers.Count; i++) {
            var wanderer = creaturesManager.Wanderers[i];

            if (wanderer == null) continue;
            if (wanderer.IsAccepted) continue;
            if (wanderer.IsRejected) continue;

            activeWanderersCount++;
        }

        if (activeWanderersCount >= dockPointsManager.WandererDockPoints.Count) {
            return false;
        }

        return true;
    }

    private int GetRandomWandererSpawnCooldown()
    {
        return UnityEngine.Random.Range(minWandererSpawnCooldown, maxWandererSpawnCooldown + 1);
    }

    private float GetWandererCooldownSpeedBonus()
    {
        if (radioStationsManager == null) return 1f;

        return Mathf.Max(1f, radioStationsManager.currentWandererCooldownSpeedBonus);
    }

    private long GetCurrentTime()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}