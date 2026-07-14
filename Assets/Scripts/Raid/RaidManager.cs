using System;
using System.Collections.Generic;
using UnityEngine;

public struct RaidEndedResult
{
    public bool IsRepeled;
}

public class RaidManager : MonoBehaviour
{
    public static RaidManager Instance;

    [Header("Main")]
    [SerializeField] private BuildingsManager buildingsManager;
    [SerializeField] private CreaturesManager creaturesManager;
    [SerializeField] private BoatsManager boatsManager;
    [SerializeField] private DockPointsManager boatDocksManager;
    [SerializeField] private CityStorage cityStorage;
    [SerializeField] private CreaturesList creaturesList;
    [SerializeField] private BoatsList boatsList;
    [SerializeField] private HumanNamesList humanNamesList;

    [SerializeField] private Inventory inventory;
    public Inventory Inventory => inventory;

    [Header("Prefabs")]
    [SerializeField] private Raider[] raiderPrefabs;
    [SerializeField] private Boat boatPrefab;

    [Header("Weapon")]
    [SerializeField] private WeaponDefinition[] weapons;
    [SerializeField] private float weaponDamageThreshold = 0.1f;

    [Header("Cooldown")]
    [SerializeField] private float minRaidCooldown = 10f;
    [SerializeField] private float maxRaidCooldown = 20f;
    [SerializeField] private float tryFindNextBuildingFrequency = 60f;

    [field: SerializeField] public float RaidCooldownTime { get; private set; } = 0f;
    [field: SerializeField] public float TimeSinceLastRaid { get; private set; } = 0f;
    public float CurrentTryFindNextBuildingTime { get; private set; } = 0f;

    [Header("Spawn")]
    [SerializeField] private int maxRaidersCount = 25;
    [SerializeField] private int raidersCountPerFloor = 2;
    [SerializeField] private float minSpawnAngleOffset = 5f;
    [SerializeField] private float maxSpawnAngleOffset = 10f;
    [SerializeField] private float spawnDistance = 145f;

    public bool IsRaidExist { get; private set; } = false;
    public bool IsUnderRaid { get; private set; } = false;

    public event Action OnRaidStarted;
    public event Action<RaidEndedResult> OnRaidEnded;

    private List<WeaponDefinition> reusableWeaponList = new List<WeaponDefinition>();

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
        Human.OnHumanDied += OnHumanDied;
        Human.OnEnteredBoat += OnEnteredBoat;
        Human.OnExitedBoat += OnExitedBoat;
    }

    private void OnDisable()
    {
        Human.OnHumanDied -= OnHumanDied;
        Human.OnEnteredBoat -= OnEnteredBoat;
        Human.OnExitedBoat -= OnExitedBoat;
    }

    private void Start()
    {
        CurrentTryFindNextBuildingTime = tryFindNextBuildingFrequency;
    }

    public void Init()
    {
        var raidData = RaidData.Default();
        raidData.RaidCooldownTime = (int)CalculateRandomCooldown();
        Init(raidData);
    }

    public void Init(RaidData raidData)
    {
        if (raidData == null) {
            Debug.LogError($"[{nameof(RaidManager)}] RaidData is null!");
            Init();
            return;
        }

        IsRaidExist = raidData.RaidExist;
        RaidCooldownTime = raidData.RaidCooldownTime;
        TimeSinceLastRaid = raidData.TimeSinceLastRaid;

        if (raidData.UnderRaid) {
            StartRaid();
        }
    }

    private void Update()
    {
        if (IsRaidExist) return;

        if (TimeSinceLastRaid < RaidCooldownTime)
            TimeSinceLastRaid += Time.deltaTime;

        if (TimeSinceLastRaid < RaidCooldownTime) return;

        CurrentTryFindNextBuildingTime += Time.deltaTime;
        if (CurrentTryFindNextBuildingTime < tryFindNextBuildingFrequency) return;

        if (!CalculateNextRaidBuilding()) {
            CurrentTryFindNextBuildingTime = 0f;
            return;
        }

        TimeSinceLastRaid = 0;
        RaidCooldownTime = CalculateRandomCooldown();
        CurrentTryFindNextBuildingTime = tryFindNextBuildingFrequency;

        CreateRaid();
    }

    public void AddLose(ItemInstance item)
    {
        if (inventory != null) {
            inventory.AddItem(item);
        }
    }

    public Building CalculateNextRaidBuilding()
    {
        if (buildingsManager == null) return null;

        Building building = null;
        List<Building> path;

        if (PathFinder.TryFindBuildingPath(null, b =>
            b != null &&
            !b.ConstructionComponent.GetUnderConstruction() &&
            b.RaidComponent.Raiders.Count < b.LevelDefinition.MaxHumansCount &&
            HasRaidableComponent(b), out path)) {
            if (path != null && path.Count > 0)
                building = path[path.Count - 1];
        }

        if (!building && PathFinder.TryFindBuildingPath(null, b =>
            b != null &&
            !b.ConstructionComponent.GetUnderConstruction() &&
            HasRaidableComponent(b), out path)) {
            if (path != null && path.Count > 0)
                building = path[path.Count - 1];
        }

        return building;
    }

    private bool HasRaidableComponent(Building b)
    {
        var raidables = b.GetComponents<IRaidable>();
        for (int i = 0; i < raidables.Length; i++) {
            if (raidables[i].CanBeRaided()) return true;
        }
        return false;
    }

    public float CalculateRandomCooldown()
    {
        return UnityEngine.Random.Range(minRaidCooldown, maxRaidCooldown);
    }

    private void CreateRaid()
    {
        var raidersCount = GetRandomRaidersCount();
        if (raidersCount <= 0) return;

        if (raiderPrefabs == null || raiderPrefabs.Length == 0) {
            Debug.LogError($"[{nameof(RaidManager)}] Raider prefabs are not assigned!");
            return;
        }

        var dir = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));
        dir.Normalize();

        for (int i = 0; i < raidersCount; i++) {
            float angle = UnityEngine.Random.Range(minSpawnAngleOffset, maxSpawnAngleOffset);
            dir = Quaternion.Euler(0f, angle, 0f) * dir;

            var position = dir * spawnDistance;
            var rotation = Quaternion.LookRotation(-position.normalized);

            var boat = CreateBoat(position, rotation);
            if (!boat) {
                Debug.LogError("Boat creation failed during raid initialization.");
                continue;
            }

            var raider = CreateRaider(position, rotation.eulerAngles, boat.InstanceId.GetGuid());
            if (!raider) {
                Debug.LogError("Raider creation failed during raid initialization.");
                continue;
            }
        }

        IsRaidExist = true;
    }

    private void StartRaid()
    {
        IsUnderRaid = true;
        OnRaidStarted?.Invoke();
    }

    private void EndRaid(bool isRepeled)
    {
        DestroyEmptyBoats();
        RemoveCityLoot();
        ClearLosses();

        IsUnderRaid = false;
        IsRaidExist = false;

        RaidEndedResult result = new RaidEndedResult() { IsRepeled = isRepeled };
        OnRaidEnded?.Invoke(result);
    }

    private void DestroyEmptyBoats()
    {
        if (boatsManager == null || boatsManager.RaiderBoats == null) return;

        for (int i = boatsManager.RaiderBoats.Count - 1; i >= 0; i--) {
            var boat = boatsManager.RaiderBoats[i];
            if (boat == null) continue;
            if (boat.CurrentRider != null) continue;

            Destroy(boat.gameObject);
        }
    }

    private void RemoveCityLoot()
    {
        if (inventory == null || cityStorage == null || cityStorage.Inventory == null) return;

        for (int i = 0; i < inventory.Items.Count; i++) {
            var item = inventory.TryGetItemByIndex(i);
            if (item == null || item.Definition == null) continue;

            cityStorage.Inventory.RemoveItem(item.Definition.ItemId, item.Amount);
        }
    }

    private void ClearLosses()
    {
        if (inventory == null) return;

        for (int i = inventory.Items.Count - 1; i >= 0; i--) {
            ItemInstance item = inventory.TryGetItemByIndex(i);
            if (item == null || item.Definition == null) continue;

            inventory.RemoveItem(item.Definition.ItemId, item.Amount);
        }
    }

    private void OnEnteredBoat(Human human)
    {
        if (human == null) return;
        if (human.GetComponent<Raider>() == null) return;
        if (!ShouldEndRaid()) return;

        EndRaid(false);
    }

    private void OnExitedBoat(Human human)
    {
        if (human == null) return;
        if (human.GetComponent<Raider>() == null) return;
        if (!TryStartRaid()) return;

        ClearLosses();
    }

    private void OnHumanDied(Human human)
    {
        if (human == null) return;
        if (human.GetComponent<Raider>() == null) return;
        if (!ShouldEndRaid()) return;

        EndRaid(true);
    }

    private bool TryStartRaid()
    {
        if (!ShouldStartRaid()) return false;

        StartRaid();
        return true;
    }

    private bool ShouldStartRaid()
    {
        if (IsUnderRaid) return false;
        if (creaturesManager == null || creaturesManager.Raiders == null) return false;

        foreach (var raider in creaturesManager.Raiders) {
            if (raider != null && !raider.IsRaidFinished && raider.HealthComponent.IsAlive) {
                return true;
            }
        }

        return false;
    }

    private bool ShouldEndRaid()
    {
        if (!IsUnderRaid) return false;
        if (creaturesManager == null || creaturesManager.Raiders == null) return true;

        // Рейд продолжается, пока на карте есть ХОТЯ БЫ ОДИН ЖИВОЙ рейдер, который ЕЩЕ НЕ В ЛОДКЕ
        foreach (var raider in creaturesManager.Raiders) {
            if (raider == null) continue;

            bool isAlive = raider.HealthComponent != null && raider.HealthComponent.IsAlive;
            bool isInBoat = raider.BoatRider != null && raider.BoatRider.RidingBoat != null;

            if (isAlive && !isInBoat) {
                return false; // Нашли активного рейдера на суше -> рейд продолжается
            }
        }

        return true;
    }

    private Human CreateRaider(Vector3 position, Vector3 rotation, Guid boatInstanceId)
    {
        var prefab = raiderPrefabs[UnityEngine.Random.Range(0, raiderPrefabs.Length)];
        if (prefab == null) return null;

        var weaponDef = GetRandomWeaponDefinition();
        ItemID? weaponId = weaponDef != null ? weaponDef.ItemId : null;

        var data = new RaiderData()
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

            Weapon = new EquipmentData()
            {
                EquipmentId = weaponId
            },

            Skills = SkillsData.CreateByLevelsCount(SkillsData.GetLevelsCountByGameStage()),
            SpawnPosition = new Vector3Data(position)
        };

        return CreatureFactory.CreateHuman(prefab, position, Quaternion.Euler(rotation), data);
    }

    private Boat CreateBoat(Vector3 position, Quaternion rotation)
    {
        if (boatDocksManager == null || boatPrefab == null) return null;

        var dockPoint = BoatDockUtils.GetNearestFreeDockPoint(boatDocksManager.RaiderDockPoints, position);
        if (!dockPoint) return null;

        var data = new BoatData()
        {
            Id = boatPrefab.Definition.BoatId,
            Position = new Vector3Data(position),
            Rotation = new Vector3Data(rotation.eulerAngles),
            DockInstanceId = dockPoint.InstanceId.GetGuid(),
            Status = HumanStatusEnum.Raider
        };

        return BoatFactory.CreateBoat(boatPrefab, position, rotation, data);
    }

    private WeaponDefinition GetRandomWeaponDefinition()
    {
        if (weapons == null || weapons.Length == 0) return null;

        var gameStage = GameStageSystem.CalculateGameStagePercent();
        var minDamage = EquipmentUtils.GetMinDamage(weapons);
        var maxDamage = EquipmentUtils.GetMaxDamage(weapons);

        float targetPower = Mathf.Lerp(minDamage, maxDamage, gameStage);

        float lowBound = Mathf.Lerp(minDamage, maxDamage, gameStage - weaponDamageThreshold);
        float highBound = Mathf.Lerp(minDamage, maxDamage, gameStage + weaponDamageThreshold);

        reusableWeaponList.Clear();

        // Оптимизированный сбор без LINQ-аллокаций в куче
        for (int i = 0; i < weapons.Length; i++) {
            var w = weapons[i];
            if (w != null && w.Power >= lowBound && w.Power <= highBound) {
                reusableWeaponList.Add(w);
            }
        }

        if (reusableWeaponList.Count > 0) {
            return reusableWeaponList[UnityEngine.Random.Range(0, reusableWeaponList.Count)];
        }

        // Запасной выбор ближайшего по силе оружия без LINQ-сортировки
        WeaponDefinition bestMatch = weapons[0];
        float minDiff = Mathf.Abs(bestMatch.Power - targetPower);

        for (int i = 1; i < weapons.Length; i++) {
            var w = weapons[i];
            if (w == null) continue;
            float diff = Mathf.Abs(w.Power - targetPower);
            if (diff < minDiff) {
                minDiff = diff;
                bestMatch = w;
            }
        }

        return bestMatch;
    }

    private int GetRandomRaidersCount()
    {
        if (buildingsManager == null || buildingsManager.BuiltFloors == null) return 1;

        var floorRaidersCount = buildingsManager.BuiltFloors.Count * raidersCountPerFloor;
        return Mathf.Min(floorRaidersCount, maxRaidersCount);
    }
}