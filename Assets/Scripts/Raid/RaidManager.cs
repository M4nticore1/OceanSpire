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
    [SerializeField] private CreaturesList creaturesList;
    [SerializeField] private BoatsList boatsList;
    [SerializeField] private HumanNamesList humanNamesList;
    [SerializeField] private CityStorage cityStorage;

    [SerializeField] private Inventory inventory;
    public Inventory Inventory => inventory;

    [Header("Prefabs")]
    [SerializeField] private Creature[] raiderPrefabs;
    [SerializeField] private Boat boatPrefab;

    [Header("Cooldown")]
    [SerializeField] private float minRaidCooldown = 10f;
    [SerializeField] private float maxRaidCooldown = 20f;
    public float CurrentRaidCooldown { get; private set; } = 0f;
    public float CurrentRaidCooldownTime { get; private set; } = 0f;

    [Header("Spawn")]
    [SerializeField] private float minSpawnAngleOffset = 5f;
    [SerializeField] private float maxSpawnAngleOffset = 10f;
    [SerializeField] private float spawnDistance = 145f;

    private int aliveRaidersCount = 0;

    [Header("Positions")]
    [SerializeField] private BoatDockPoint[] dockPoints;

    public bool IsRaidExist { get; private set; } = false;
    public bool IsUnderRaid { get; private set; } = false;
    public int landedRaidersCount = 0;

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

    public void Init(RaidData raidData)
    {
        if (raidData.UnderRaid) {
            StartRaid();
        }

        IsRaidExist = raidData.RaidExist;
        CurrentRaidCooldown = raidData.RaidCooldown;
        CurrentRaidCooldownTime = raidData.TimeSinceLastRaid;
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
        List<Building> path = new();

        if (PathFinder.TryGetPathToBuilding(null, b => b.BuildingData.IsRaidable && b.RaidComponent.Raiders.Count < b.LevelData.MaxHumansCount, ref path)) {
            building = path[path.Count - 1];
        }

        if (!building && PathFinder.TryGetPathToBuilding(null, b => b.BuildingData.IsRaidable, ref path)) {
            building = path[path.Count - 1];
        }

        return building;
    }

    public float CalculateRandomCooldown()
    {
        float cooldown = Random.Range(minRaidCooldown, maxRaidCooldown);
        return cooldown;
    }

    private void CreateRaid()
    {
        int raidersAmount = GetRandomRaidersAmount();
        aliveRaidersCount = raidersAmount;

        Vector3 dir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        dir.Normalize();

        for (int i = 0; i < raidersAmount; i++) {
            float angle = Random.Range(minSpawnAngleOffset, maxSpawnAngleOffset);
            dir = Quaternion.Euler(0f, angle, 0f) * dir;

            Vector3 position = dir * spawnDistance;
            Quaternion rotation = Quaternion.LookRotation(-position.normalized);

            var boat = CreateBoat(position, rotation);
            var raider = CreateRaider(position, rotation.eulerAngles, boat.InstanceId.GetInstanceId());
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
        RemoveCityLoot();
        IsUnderRaid = false;
        IsRaidExist = false;

        RaidEndedResult result = new RaidEndedResult()
        {
            IsRepeled = isRepeled
        };

        OnRaidEnded?.Invoke(result);
    }

    private void RemoveCityLoot()
    {
        for (int i = 0; i < inventory.Items.Count; i++) {
            ItemInstance item = inventory.GetItemByIndex(i);

            int id = item.Definition.ItemId;
            int amount = item.Amount;

            CityStorage.Instance.Inventory.RemoveItem(id, amount);
        }
    }

    private void ClearLosses()
    {
        for (int i = 0; i < inventory.Items.Count; i++) {
            ItemInstance item = inventory.GetItemByIndex(i);

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
        Raider raider = human as Raider;
        if (raider == null) return;

        if (!raider.IsRaidFinished) return;

        landedRaidersCount--;

        if (landedRaidersCount == 0) {
            EndRaid(false);
        }
    }

    private void OnExitedBoat(Human human)
    {
        var raider = human.GetComponent<Raider>();
        if (!raider) return;

        landedRaidersCount++;

        if (landedRaidersCount == 1) {
            ClearLosses();
            StartRaid();
        }
    }

    private void OnHumanDied(Human human)
    {
        var raider = human.GetComponent<Raider>();
        if (!raider) return;

        aliveRaidersCount--;

        if (aliveRaidersCount <= 0) {
            EndRaid(true);
        }
    }

    private Human CreateRaider(Vector3 position, Vector3 rotation, int boatInstanceId)
    {
        var prefab = raiderPrefabs[UnityEngine.Random.Range(0, raiderPrefabs.Length)] as Human;

        var data = new RaiderData()
        {
            Id = prefab.Definition.CreatureId,
            InstanceId = InstancesManager.Instance.GetNextInstanceId(),
            Position = new Vector3Data(position),
            Rotation = new Vector3Data(rotation),
            Health = prefab.HealthComponent.MaxHealth,

            Name = new NameData()
            {
                FirstNameId = prefab.GenderComponent.IsMale ? humanNamesList.GetRandomMaleFirstNameId() : humanNamesList.GetRandomFemaleFirstNameId(),
                LastNameId = prefab.GenderComponent.IsMale ? humanNamesList.GetRandomMaleLastNameId() : humanNamesList.GetRandomFemaleLastNameId(),
            },

            BoatRider = new BoatRiderData()
            {
                BoatInstanceId = boatInstanceId,
                Riding = true,
            },

            Weapon = WeaponsDataFactory.CreateRandomData(WeaponsDataFactory.GetMinWeaponDamageId() + 1, WeaponsDataFactory.GetMaxWeaponDamage()),
            Skills = SkillsFactory.CreateRandomSkillsData(SkillsFactory.GetLevelsCount()),
            SpawnPosition = new Vector3Data(position)
        };

        var human = CreatureFactory.CreateHuman(prefab, position, Quaternion.Euler(rotation), data);
        return human;
    }

    private Boat CreateBoat(Vector3 position, Quaternion rotation)
    {
        var data = new BoatData()
        {
            Id = boatPrefab.Definition.BoatId,
            InstanceId = InstancesManager.Instance.GetNextInstanceId(),
            Position = new Vector3Data(position),
            Rotation = new Vector3Data(rotation.eulerAngles),
            DockInstanceId = GetNearestDockPoint(position).InstanceId.GetInstanceId(),
            Status = HumanStatusEnum.Raider
        };

        var boat = BoatFactory.CreateBoat(boatPrefab, position, rotation, data);

        return boat;
    }

    private BoatDockPoint GetNearestDockPoint(Vector3 position)
    {
        BoatDockPoint bestDockPoint = null;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < dockPoints.Length; i++) {
            var dockPoint = dockPoints[i];

            if (dockPoint.Boat != null)
                continue;

            float sqr = (position - dockPoint.transform.position).sqrMagnitude;

            if (sqr < bestSqr) {
                bestDockPoint = dockPoint;
                bestSqr = sqr;
            }
        }

        return bestDockPoint;
    }

    private int GetRandomRaidersAmount()
    {
        int floorAmount = BuildingsManager.Instance.BuiltFloors.Count;
        int raidersAmount = Random.Range(floorAmount / 2, floorAmount + 1);
        return raidersAmount;
    }
}