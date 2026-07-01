using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct RaidEndedResult
{
    public bool IsRepeled;
}

public class RaidManager : MonoBehaviour
{
    public static RaidManager Instance;

    [Header("Main")]
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
    [SerializeField] private Creature[] raiderPrefabs;
    [SerializeField] private Boat boatPrefab;

    [Header("Weapon")]
    [SerializeField] private WeaponDefinition[] weapons;
    [SerializeField] private float weaponDamageThreshold = 0.1f;

    [Header("Cooldown")]
    [SerializeField] private float minRaidCooldown = 10f;
    [SerializeField] private float maxRaidCooldown = 20f;
    public float CurrentRaidCooldown { get; private set; } = 0f;
    public float CurrentRaidCooldownTime { get; private set; } = 0f;

    [Header("Spawn")]
    [SerializeField] private float minRaiderCountMultiplier = 0.5f;
    [SerializeField] private float maxRaiderCountMultiplier = 1f;
    [SerializeField] private float minSpawnAngleOffset = 5f;
    [SerializeField] private float maxSpawnAngleOffset = 10f;
    [SerializeField] private float spawnDistance = 145f;

    public bool IsRaidExist { get; private set; } = false;
    public bool IsUnderRaid { get; private set; } = false;

    public event System.Action OnRaidStarted;
    public event System.Action<RaidEndedResult> OnRaidEnded;

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

    public void Init()
    {
        var raidData = RaidData.Default();
        raidData.RaidCooldown = (int)CalculateRandomCooldown();

        Init(raidData);
    }

    public void Init(RaidData raidData)
    {
        if (raidData == null) {
            Debug.LogError("raidData is not valid");
            return;
        }

        IsRaidExist = raidData.RaidExist;
        CurrentRaidCooldown = raidData.RaidCooldown;
        CurrentRaidCooldownTime = raidData.TimeSinceLastRaid;

        if (raidData.UnderRaid) {
            StartRaid();
        }
    }

    private void Update()
    {
        if (IsRaidExist) return;

        if (CurrentRaidCooldownTime < CurrentRaidCooldown)
            CurrentRaidCooldownTime += Time.deltaTime;

        if (CurrentRaidCooldownTime < CurrentRaidCooldown) return;

        ResetCurrentRaidTime();
        ApplyRandomCooldown();

        if (!CalculateNextRaidBuilding()) return;

        CreateRaid();
    }

    public void AddLose(ItemInstance lose)
    {
        int id = lose.Definition.ItemId;
        int amount = lose.Amount;
        inventory.AddItem(id, amount);
    }

    public Building CalculateNextRaidBuilding()
    {
        Building building = null;
        List<Building> path;

        if (PathFinder.TryFindBuildingPath(null,
            b => b.BuildingData.IsRaidable &&
            b.RaidComponent.Raiders.Count < b.LevelData.MaxHumansCount &&
            !b.ConstructionComponent.GetUnderConstruction(),
            out path)) {
            int index = path.Count - 1;
            if (index >= 0)
                building = path[index];
        }

        if (!building && PathFinder.TryFindBuildingPath(null,
            b => b.BuildingData.IsRaidable &&
            !b.ConstructionComponent.GetUnderConstruction(),
            out path)) {
            int index = path.Count - 1;
            if (index >= 0)
                building = path[index];
        }

        return building;
    }

    public float CalculateRandomCooldown()
    {
        float cooldown = UnityEngine.Random.Range(minRaidCooldown, maxRaidCooldown);
        return cooldown;
    }

    private void CreateRaid()
    {
        int raidersAmount = GetRandomRaidersCount();

        var dir = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));
        dir.Normalize();

        for (int i = 0; i < raidersAmount; i++) {
            float angle = UnityEngine.Random.Range(minSpawnAngleOffset, maxSpawnAngleOffset);
            dir = Quaternion.Euler(0f, angle, 0f) * dir;

            var position = dir * spawnDistance;
            var rotation = Quaternion.LookRotation(-position.normalized);

            var boat = CreateBoat(position, rotation);
            if (!boat) {
                Debug.LogError("boat is not valid");
                continue;
            }

            var raider = CreateRaider(position, rotation.eulerAngles, boat.InstanceId.GetGuid());
            if (!raider) {
                Debug.LogError("raiedr is not valid");
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
        IsUnderRaid = false;
        IsRaidExist = false;

        RaidEndedResult result = new RaidEndedResult()
        {
            IsRepeled = isRepeled
        };

        OnRaidEnded?.Invoke(result);
    }

    private void DestroyEmptyBoats()
    {
        for (int i = boatsManager.RaiderBoats.Count - 1; i >= 0; i--) {
            try {
                var boat = boatsManager.RaiderBoats[i];
                if (boat.CurrentRider) continue;

                Destroy(boat.gameObject);
                boatsManager.UnregisterRaiderBoat(boat);
            }
            catch (Exception e) {
                Debug.LogError(e);
            }
        }
    }

    private void RemoveCityLoot()
    {
        for (int i = 0; i < inventory.Items.Count; i++) {
            ItemInstance item = inventory.TryGetItemByIndex(i);

            int id = item.Definition.ItemId;
            int amount = item.Amount;

            CityStorage.Instance.Inventory.RemoveItem(id, amount);
        }
    }

    private void ClearLosses()
    {
        for (int i = 0; i < inventory.Items.Count; i++) {
            ItemInstance item = inventory.TryGetItemByIndex(i);

            int id = item.Definition.ItemId;
            int amount = item.Amount;

            inventory.RemoveItem(id, amount);
        }
    }

    private void ApplyRandomCooldown()
    {
        CurrentRaidCooldown = CalculateRandomCooldown();
    }

    private void ResetCurrentRaidTime()
    {
        CurrentRaidCooldownTime = 0;
    }

    private void OnEnteredBoat(Human human)
    {
        if (!ShouldEndRaid()) return;

        EndRaid(false);
    }

    private void OnExitedBoat(Human human)
    {
        if (!TryStartRaid()) return;

        ClearLosses();
    }

    private void OnHumanDied(Human human)
    {
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

        foreach (var raider in creaturesManager.Raiders) {
            if (raider.IsRaidFinished) return false;
        }

        return true;
    }

    private bool ShouldEndRaid()
    {
        if (!IsUnderRaid) return false;

        foreach (var raider in creaturesManager.Raiders) {
            if (!raider.IsRaidFinished && !raider.BoatRider.RidingBoat) return false;
            if (raider.HealthComponent.IsAlive && !raider.BoatRider.RidingBoat) return false;
        }

        return true;
    }

    private Human CreateRaider(Vector3 position, Vector3 rotation, Guid boatInstanceId)
    {
        var prefab = raiderPrefabs[UnityEngine.Random.Range(0, raiderPrefabs.Length)] as Human;

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
                EquipmentId = GetRandomWeaponDefinition()?.ItemId
            },

            Skills = SkillsFactory.CreateRandomSkillsData(SkillsFactory.GetLevelsCount()),
            SpawnPosition = new Vector3Data(position)
        };

        var human = CreatureFactory.CreateHuman(prefab, position, Quaternion.Euler(rotation), data);
        return human;
    }

    private Boat CreateBoat(Vector3 position, Quaternion rotation)
    {
        var dockPoint = BoatDockUtils.GetNearestFreeDockPoint(boatDocksManager.RaiderDockPoints, position);
        if (!dockPoint) {
            Debug.LogError("dockPoint is not valid using BoatDockUtils.GetNearestFreeDockPoint");
            return null;
        }

        var data = new BoatData()
        {
            Id = boatPrefab.Definition.BoatId,
            Position = new Vector3Data(position),
            Rotation = new Vector3Data(rotation.eulerAngles),
            DockInstanceId = dockPoint.InstanceId.GetGuid(),
            Status = HumanStatusEnum.Raider
        };

        var boat = BoatFactory.CreateBoat(boatPrefab, position, rotation, data);
        if (!boat) {
            Debug.LogError("boat is not valid using BoatFactory.CreateBoat");
            return null;
        }

        return boat;
    }

    private WeaponDefinition GetRandomWeaponDefinition()
    {
        var gameStage = GameStageSystem.CalculateGameStagePercent();
        var minDamage = EquipmentUtils.GetMinDamage(weapons);
        var maxDamage = EquipmentUtils.GetMaxDamage(weapons);

        float targetPower = Mathf.Lerp(minDamage, maxDamage, gameStage);

        var suitableWeapons = weapons.Where(w =>
            w.Power >= Mathf.Lerp(minDamage, maxDamage, gameStage - weaponDamageThreshold) &&
            w.Power <= Mathf.Lerp(minDamage, maxDamage, gameStage + weaponDamageThreshold)
        ).ToList();

        if (suitableWeapons.Count > 0) {
            return suitableWeapons[UnityEngine.Random.Range(0, suitableWeapons.Count)];
        }

        return weapons.OrderBy(w => Mathf.Abs(w.Power - targetPower)).FirstOrDefault();
    }

    private int GetRandomRaidersCount()
    {
        int floorsCount = BuildingsManager.Instance.BuiltFloors.Count;
        int raidersCount = UnityEngine.Random.Range((int)(floorsCount * minRaiderCountMultiplier), (int)(floorsCount * maxRaiderCountMultiplier));
        return raidersCount;
    }
}