using System.Collections.Generic;
using UnityEngine;

public struct RaidEndedResult
{
    public bool isRepeled;
}

public class RaidManager : MonoBehaviour
{
    public static RaidManager Instance;

    [SerializeField] private Inventory inventory;
    public Inventory Inventory => inventory;

    [Header("Prefabs")]
    [SerializeField] private Human attackerPrefab;
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
    private Dictionary<Boat, Vector3> spawnPositions = new();

    public bool IsRaidExist { get; private set; } = false;
    public bool IsUnderRaid { get; private set; } = false;
    public int landedRaidersCount = 0;

    public event System.Action onRaidStarted;
    public event System.Action<RaidEndedResult> onRaidEnded;

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
        Human.onHumanDied += OnHumanDied;
        Human.onEnteredBoat += OnEnteredBoat;
        Human.onExitedBoat += OnExitedBoat;
    }

    private void OnDisable()
    {
        Human.onHumanDied -= OnHumanDied;
        Human.onEnteredBoat -= OnEnteredBoat;
        Human.onExitedBoat -= OnExitedBoat;
    }

    public void Init(RaidData raidData)
    {
        IsRaidExist = raidData.RaidExist;
        IsUnderRaid = raidData.UnderRaid;
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

    public Vector3 GetSpawnPosition(Boat boat)
    {
        return spawnPositions[boat];
    }

    public Building CalculateNextRaidBuilding()
    {
        Building building = null;
        List<Building> path = new();

        if (PathFinder.TryGetPathToBuilding(null, b => b.BuildingData.IsRaidable && b.RaidComponent.Raiders.Count < b.LevelData.maxResidentsCount, ref path)) {
            building = path[path.Count - 1];
        }

        if (!building && PathFinder.TryGetPathToBuilding(null, b => b.BuildingData.IsRaidable, ref path)) {
            building = path[path.Count - 1];
        }

        return building;
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

            Boat boat = CreateBoat(position, rotation);

            Human raider = CreateRaider(position, rotation.eulerAngles, boat.InstanceId.Id);

            spawnPositions.Add(boat, position);
        }

        IsRaidExist = true;
    }

    private void StartRaid()
    {
        ClearLosses();
        IsUnderRaid = true;
        onRaidStarted?.Invoke();
    }

    private void EndRaid(bool isRepeled)
    {
        RemoveCityLoot();
        IsUnderRaid = false;
        IsRaidExist = false;

        RaidEndedResult result = new RaidEndedResult()
        {
            isRepeled = isRepeled
        };

        onRaidEnded?.Invoke(result);
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
        CurrentRaidCooldown = GetRandomCooldown();
    }

    private void ResetCurrentRaidTime()
    {
        CurrentRaidCooldownTime = 0;
    }

    private void OnEnteredBoat(Human human)
    {
        RaiderState raiderState = human.currentStatus as RaiderState;
        if (raiderState == null) return;

        if (!raiderState.isFinishedRaiding) return;

        landedRaidersCount--;

        if (landedRaidersCount == 0) {
            EndRaid(false);
        }
    }

    private void OnExitedBoat(Human human)
    {
        if (human.CurrentStatusEnum != HumanStatusEnum.Raider) return;

        landedRaidersCount++;

        if (landedRaidersCount == 1) {
            StartRaid();
        }
    }

    private void OnHumanDied(Human human)
    {
        if (human.CurrentStatusEnum != HumanStatusEnum.Raider) return;

        aliveRaidersCount--;

        if (aliveRaidersCount <= 0) {
            EndRaid(true);
        }
    }

    private Human CreateRaider(Vector3 position, Vector3 rotation, int boatInstanceId)
    {
        HumanData data = HumanDataFactory.CreateRandomRaiderData();
        data.Position = new Vector3Data(position);
        data.Rotation = new Vector3Data(rotation);
        data.BoatRider.SetBoatInstanceId(boatInstanceId);
        data.BoatRider.SetRiding(true);

        Human human = CreatureFactory.CreateHuman(data);

        return human;
    }

    private Boat CreateBoat(Vector3 position, Quaternion rotation)
    {
        int id = boatPrefab.BoatData.BoatId;
        int instanceId = InstancesManager.Instance.GetNextInstanceId();
        float health = boatPrefab.Health.MaxHealth;
        int dockInstanceId = GetNearestDockPoint(position).InstanceId.Id;

        BoatData data = new BoatData()
        {
            Id = id,
            InstanceId = instanceId,
            Position = new Vector3Data(position),
            Rotation = new Vector3Data(rotation.eulerAngles),
            Health = health,
            DockInstanceId = dockInstanceId,
        };

        Boat boat = BoatFactory.CreateBoat(boatPrefab, data);

        return boat;
    }

    private BoatDockPoint GetNearestDockPoint(Vector3 position)
    {
        BoatDockPoint dockPoint = dockPoints[0];
        float distance = Vector3.Distance(position, dockPoint.transform.position);

        for (int i = 1; i < dockPoints.Length; i++) {
            if (dockPoints[i].boat != null) continue;

            float currentDistance = Vector3.Distance(position, dockPoints[i].transform.position);
            if (currentDistance >= distance) continue;

            dockPoint = dockPoints[i];
            distance = currentDistance;       
        }
        return dockPoint;
    }

    private int GetRandomRaidersAmount()
    {
        int floorAmount = BuildingsManager.Instance.BuiltFloors.Count;
        int raidersAmount = Random.Range(floorAmount / 2, floorAmount + 1);
        return raidersAmount;
    }

    private float GetRandomCooldown()
    {
        float cooldown = Random.Range(minRaidCooldown, maxRaidCooldown);
        return cooldown;
    }
}