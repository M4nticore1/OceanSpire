using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class RaidEndedResult
{
    public bool IsRepeled;
    public List<ItemInstance> Losses;
}

public class RaidManager : MonoBehaviour
{
    public static RaidManager Instance;

    [Header("Main")]
    [SerializeField] private BuildingsManager buildingsManager;
    [SerializeField] private CreaturesManager creaturesManager;
    [SerializeField] private BoatsManager boatsManager;
    [SerializeField] private BoatDocksManager boatDocksManager;
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
    [SerializeField] private float tryFindNextBuildingFrequency = 5f;

    [field: SerializeField] public float RaidCooldownTime { get; private set; } = 0f;
    [field: SerializeField] public float TimeSinceLastRaid { get; private set; } = 0f;
    [field: SerializeField] public float CurrentTryFindNextBuildingTime { get; private set; } = 0f;

    [Header("Spawn")]
    [SerializeField] private int maxRaidersCount = 25;
    [SerializeField] private float raidersCountPerFloor = 2f;
    [SerializeField] private float raidersPerBuilding = 0.5f;
    [SerializeField] private float minSpawnAngleOffset = 5f;
    [SerializeField] private float maxSpawnAngleOffset = 10f;
    [SerializeField] private float spawnDistance = 145f;

    [field: SerializeField] public bool IsRaidExist { get; private set; } = false;
    [field: SerializeField] public bool IsUnderRaid { get; private set; } = false;

    public event Action OnRaidStarted;
    public event Action<RaidEndedResult> OnRaidEnded;

    private List<WeaponDefinition> reusableWeaponList = new List<WeaponDefinition>();

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
        Human.OnHumanDied += HandleHumanDied;
        Human.OnHumanEnteredBoat += HandleEnteredBoat;
        Human.OnHumanExitedBoat += HandleExitedBoat;
    }

    private void OnDisable()
    {
        Human.OnHumanDied -= HandleHumanDied;
        Human.OnHumanEnteredBoat -= HandleEnteredBoat;
        Human.OnHumanExitedBoat -= HandleExitedBoat;
    }

    private void Start()
    {
        CurrentTryFindNextBuildingTime = tryFindNextBuildingFrequency;
    }

    private void Update()
    {
        if (!IsRaidExist) {
            if (TimeSinceLastRaid < RaidCooldownTime) {
                TimeSinceLastRaid += Time.deltaTime;
            }
            else {
                CurrentTryFindNextBuildingTime += Time.deltaTime;

                if (CurrentTryFindNextBuildingTime >= tryFindNextBuildingFrequency) {
                    if (TryCreateRaid()) {
                        TimeSinceLastRaid = 0;
                        RaidCooldownTime = CalculateRandomCooldown();
                        CurrentTryFindNextBuildingTime = tryFindNextBuildingFrequency;
                    }
                    else {
                        CurrentTryFindNextBuildingTime = 0f;
                    }
                }
            }
        }
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
            Debug.LogError($"[{nameof(RaidManager)}] Raid Data is not valid!");
            Init();
            return;
        }

        IsRaidExist = raidData.RaidExist;
        RaidCooldownTime = raidData.RaidCooldownTime;
        TimeSinceLastRaid = raidData.TimeSinceLastRaid;
        inventory.Init(raidData.Inventory);

        UpdateRaidExist();
        UpdateUnderRaid();
        UpdateBoatDocks();

        DestroyExtraBoats();
        DestroyDiedRaiders();
    }

    public void AddLose(ItemInstance item)
    {
        if (item == null) {
            Debug.LogError($"[{nameof(RaidManager)}] Item is not valid!");
            return;
        }

        if (inventory == null) {
            Debug.LogError($"[{nameof(RaidManager)}] Inventory is not valid!");
            return;
        }

        if (item.Amount <= 0) return;

        inventory.AddItemAmount(item);
    }

    public void AddLosses(List<ItemInstance> items)
    {
        foreach (var item in items) {
            if (item == null) continue;

            AddLose(item);
        }
    }

    public Building CalculateNextRaidBuilding()
    {
        if (buildingsManager == null) return null;

        Building building = null;
        List<Building> path;

        if (PathFinder.TryFindBuildingPath(null, b =>
            b != null && !b.ConstructionComponent.GetUnderConstruction() &&
            b.RaidersHandler.Interactors.Count < b.LevelDefinition.MaxHumansCount &&
            b.CanBeRaided(), out path)) {
            if (path != null && path.Count > 0)
                building = path[path.Count - 1];
        }

        if (building == null && PathFinder.TryFindBuildingPath(null, b =>
            b != null && !b.ConstructionComponent.GetUnderConstruction() &&
            b.CanBeRaided(), out path)) {
            if (path != null && path.Count > 0)
                building = path[path.Count - 1];
        }

        return building;
    }

    public float CalculateRandomCooldown()
    {
        return UnityEngine.Random.Range(minRaidCooldown, maxRaidCooldown);
    }

    private void StartRaid()
    {
        IsUnderRaid = true;
        OnRaidStarted?.Invoke();
    }

    private void EndRaid(bool isRepeled)
    {
        var result = new RaidEndedResult()
        {
            IsRepeled = isRepeled,
            Losses = inventory.Items.ToList(),
        };

        DestroyExtraBoats();
        DestroyDiedRaiders();
        RemoveCityLoot();
        ClearLosses();

        IsUnderRaid = false;
        IsRaidExist = false;

        OnRaidEnded?.Invoke(result);
    }

    private void UpdateUnderRaid()
    {
        if (ShouldStartRaid()) {
            StartRaid();
        }
        else if (ShouldEndRaid()) {
            EndRaid(true);
        }
    }

    private void UpdateRaidExist()
    {
        if (ShouldSetRaidExistTrue()) {
            IsRaidExist = true;
        }
        else if (ShouldSetRaidExistFalse()) {
            IsRaidExist = false;
        }
    }

    private void UpdateBoatDocks()
    {
        var raiderBoats = boatsManager.RaiderBoats;
        if (raiderBoats == null) return;

        foreach (var boat in raiderBoats) {
            if (boat == null) continue;

            var dockPoint = BoatDockUtils.GetNearestFreeDockPoint(boatDocksManager.RaiderDockPoints, boat.transform.position);
            if (dockPoint == null) continue;

            boat.SetDockPoint(dockPoint);
        }
    }

    private bool TryCreateRaid()
    {
        var raidersCount = GetRandomRaidersCount();
        if (raidersCount <= 0) return false;

        if (raiderPrefabs == null || raiderPrefabs.Length == 0) {
            Debug.LogError($"[{nameof(RaidManager)}] Raider prefabs are not assigned!");
            return false;
        }

        var dir = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));
        dir.Normalize();

        for (int i = 0; i < raidersCount; i++) {
            float angle = UnityEngine.Random.Range(minSpawnAngleOffset, maxSpawnAngleOffset);
            dir = Quaternion.Euler(0f, angle, 0f) * dir;

            var position = dir * spawnDistance;
            var rotation = Quaternion.LookRotation(-position.normalized);

            var boat = CreateBoat(position, rotation);
            if (boat == null) {
                Debug.LogError($"[{nameof(RaidManager)}] Boat creation failed during raid initialization.");
                continue;
            }

            var raider = CreateRaider(position, rotation.eulerAngles, boat.InstanceId.GetGuid());
            if (raider == null) {
                Debug.LogError($"[{nameof(RaidManager)}] Raider creation failed during raid initialization.");
                continue;
            }
        }

        IsRaidExist = true;
        return true;
    }

    private void DestroyExtraBoats()
    {
        var raiders = creaturesManager.Raiders;
        if (raiders == null) return;

        var raiderBoats = boatsManager.RaiderBoats;
        if (raiderBoats == null) return;

        var extraCount = Mathf.Max(raiderBoats.Count - raiders.Count, 0);

        for (int i = 0; i < extraCount; i++) {
            var boat = raiderBoats[raiderBoats.Count - i - 1];
            if (boat == null) continue;

            Destroy(boat.gameObject);
        }
    }

    private void DestroyDiedRaiders()
    {
        var raiders = creaturesManager.Raiders;
        if (raiders == null) return;

        for (int i = 0; i < raiders.Count; i++) {
            var raider = raiders[i];
            if (raider == null) continue;
            if (raider.HealthComponent.IsAlive) continue;

            Destroy(raider.gameObject);
        }
    }

    private void RemoveCityLoot()
    {
        if (inventory == null) return;
        if (cityStorage == null) return;
        if (cityStorage.Inventory == null) return;

        for (int i = 0; i < inventory.Items.Count; i++) {
            var item = inventory.TryGetItemByIndex(i);
            if (item == null) continue;
            if (item.Definition == null) continue;

            cityStorage.Inventory.RemoveItemAmount(item.Definition.ItemId, item.Amount);
        }
    }

    private void ClearLosses()
    {
        if (inventory == null) return;

        for (int i = inventory.Items.Count - 1; i >= 0; i--) {
            var item = inventory.TryGetItemByIndex(i);
            if (item == null) continue;
            if (item.Definition == null) continue;

            inventory.RemoveItem(item);
        }
    }

    private void HandleEnteredBoat(Human human)
    {
        if (human == null) return;
        if (human.GetComponent<Raider>() == null) return;
        if (!ShouldEndRaid()) return;

        EndRaid(false);
    }

    private void HandleExitedBoat(Human human)
    {
        if (human == null) return;
        if (human.GetComponent<Raider>() == null) return;
        if (!TryStartRaid()) return;

        ClearLosses();
    }

    private void HandleHumanDied(Human human)
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
        if (creaturesManager == null) return false;
        if (creaturesManager.Raiders == null) return false;

        foreach (var raider in creaturesManager.Raiders) {
            if (raider == null) continue;
            if (raider.HealthComponent == null || !raider.HealthComponent.IsAlive) continue;
            if (raider.IsRaidFinished && raider.BoatRider != null && raider.BoatRider.RidingBoat != null) continue;
            if (raider.BoatRider != null && raider.BoatRider.RidingBoat != null) continue;

            return true;
        }

        return false;
    }

    private bool ShouldEndRaid()
    {
        if (!IsUnderRaid) return false;
        if (creaturesManager == null) return true;

        foreach (var raider in creaturesManager.Raiders) {
            if (raider == null) continue;

            bool isAlive = raider.HealthComponent != null && raider.HealthComponent.IsAlive;
            bool isInBoat = raider.BoatRider != null && raider.BoatRider.RidingBoat != null;

            if (isAlive && !raider.IsRaidFinished) {
                return false;
            }

            if (isAlive && !raider.IsRaidFinished && isInBoat) {
                return false;
            }

            if (isAlive && !isInBoat) {
                return false;
            }
        }

        return true;
    }

    private bool ShouldSetRaidExistTrue()
    {
        var raiders = creaturesManager.Raiders;
        foreach (var raider in raiders) {
            if (raider == null) continue;
            if (!raider.IsRaidFinished) return true;
        }

        return false;
    }

    private bool ShouldSetRaidExistFalse()
    {
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

        return CreatureFactory.CreateHuman(prefab, data);
    }

    private Boat CreateBoat(Vector3 position, Quaternion rotation)
    {
        if (boatDocksManager == null) return null;
        if (boatPrefab == null) return null;

        var dockPoint = BoatDockUtils.GetNearestFreeDockPoint(boatDocksManager.RaiderDockPoints, position);
        if (dockPoint == null) return null;

        var data = new BoatData()
        {
            Id = boatPrefab.Definition.BoatId,
            Position = new Vector3Data(position),
            Rotation = new Vector3Data(rotation.eulerAngles),
            DockInstanceId = dockPoint.InstanceId.GetGuid(),
            Status = BoatStatusEnum.Raider
        };

        return BoatFactory.CreateBoat(boatPrefab, data);
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

        for (int i = 0; i < weapons.Length; i++) {
            var weapon = weapons[i];
            if (weapon == null) continue;

            if (weapon.Power < lowBound) continue;
            if (weapon.Power > highBound) continue;

            reusableWeaponList.Add(weapon);
        }

        if (reusableWeaponList.Count > 0) {
            return reusableWeaponList[UnityEngine.Random.Range(0, reusableWeaponList.Count)];
        }

        var bestMatch = weapons[0];
        float minDiff = Mathf.Abs(bestMatch.Power - targetPower);

        for (int i = 1; i < weapons.Length; i++) {
            var weapon = weapons[i];
            if (weapon == null) continue;

            float diff = Mathf.Abs(weapon.Power - targetPower);
            if (diff >= minDiff) continue;

            minDiff = diff;
            bestMatch = weapon;
        }

        return bestMatch;
    }

    private int GetRandomRaidersCount()
    {
        if (buildingsManager == null) return 0;
        if (buildingsManager.BuiltFloors == null) return 0;

        var floorRaidersCount = (int)(buildingsManager.BuiltFloors.Count * raidersCountPerFloor);
        var buildingRaidersCount = (int)(buildingsManager.GetAvalableRaidableBuildings().Count * raidersPerBuilding);

        return Mathf.Min(floorRaidersCount, buildingRaidersCount, maxRaidersCount);
    }
}