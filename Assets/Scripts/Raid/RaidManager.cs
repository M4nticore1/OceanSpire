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
    private float currentRaidCooldown = 0f;
    private float currentRaidCooldownTime = 0f;

    [Header("Spawn")]
    [SerializeField] private float minSpawnAngleOffset = 5f;
    [SerializeField] private float maxSpawnAngleOffset = 10f;
    [SerializeField] private float spawnDistance = 145f;

    private int aliveRaidersCount = 0;

    [Header("Positions")]
    [SerializeField] private BoatDockPoint[] dockPoints;
    private Dictionary<Boat, Vector3> spawnPositions = new();

    public bool IsRaidCreated { get; private set; } = false;
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
        EventBus.onRaiderDied += OnRaiderDied;
        Human.onEnteredBoat += OnEnteredBoat;
        Human.onExitedBoat += OnExitedBoat;
    }

    private void OnDisable()
    {
        EventBus.onRaiderDied -= OnRaiderDied;
        Human.onEnteredBoat -= OnEnteredBoat;
        Human.onExitedBoat -= OnExitedBoat;
    }

    private void Start()
    {
        ApplyRandomCooldown();
    }

    private void Update()
    {
        if (IsRaidCreated) return;

        if (currentRaidCooldownTime < currentRaidCooldown)
            currentRaidCooldownTime += Time.deltaTime;

        if (currentRaidCooldownTime < currentRaidCooldown) return;

        ResetCurrentRaidTime();
        ApplyRandomCooldown();

        if (!CalculateNextRaidBuilding()) return;

        CreateRaid();
    }

    public void AddLose(ItemInstance lose)
    {
        int id = lose.ItemData.ItemId;
        int amount = lose.Amount;
        inventory.AddItemAmount(id, amount);
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

            Human raider = CreateRaider(position, rotation.eulerAngles, boat.InstanceId.id);

            spawnPositions.Add(boat, position);
        }

        IsRaidCreated = true;
    }

    private void StartRaid()
    {
        ClearLosses();
        IsUnderRaid = true;
        onRaidStarted?.Invoke();
    }

    private void StopRaid(bool isRepeled)
    {
        RemoveCityLoot();
        IsUnderRaid = false;
        IsRaidCreated = false;

        RaidEndedResult result = new RaidEndedResult()
        {
            isRepeled = isRepeled
        };

        onRaidEnded?.Invoke(result);
    }

    private void RemoveCityLoot()
    {
        for (int i = 0; i < inventory.Items.Count; i++) {
            ItemInstance item = inventory.Items[i].item;

            int id = item.ItemData.ItemId;
            int amount = item.Amount;

            CityStorage.Instance.Inventory.RemoveItemAmount(id, amount);
        }
    }

    private void ClearLosses()
    {
        for (int i = 0; i < inventory.Items.Count; i++) {
            ItemInstance item = inventory.Items[i].item;

            int id = item.ItemData.ItemId;
            int amount = item.Amount;

            inventory.RemoveItemAmount(id, amount);
        }
    }

    private void ApplyRandomCooldown()
    {
        currentRaidCooldown = GetRandomCooldown();
    }

    private void ResetCurrentRaidTime()
    {
        currentRaidCooldownTime = 0;
    }

    private void DestroyBoats()
    {
        //foreach (var boat in spawnedBoats) {
        //    Destroy(boat.gameObject);
        //}
    }

    private void OnEnteredBoat(Human human)
    {
        RaiderState raiderState = human.currentStatus as RaiderState;
        if (raiderState == null) return;

        if (!raiderState.isFinishedRaiding) return;

        landedRaidersCount--;

        if (landedRaidersCount == 0) {
            StopRaid(false);
        }
    }

    private void OnExitedBoat(Human human)
    {
        if (human.currentStatusEnum != HumanStatusEnum.Raider) return;

        landedRaidersCount++;

        if (landedRaidersCount == 1) {
            StartRaid();
        }
    }

    private void OnRaiderDied(Human human)
    {
        aliveRaidersCount--;

        if (aliveRaidersCount <= 0) {
            StopRaid(true);
        }
    }

    private Human CreateRaider(Vector3 position, Vector3 rotation, int boatInstanceId)
    {
        HumanDataV1 data = HumanDataFactory.CreateRandomRaiderData();
        data.SetPosition(position);
        data.SetRotation(rotation);
        data.boatRider.SetBoatInstanceId(boatInstanceId);
        data.boatRider.SetRiding(true);

        Human human = CreatureFactory.CreateHuman(data);

        return human;
    }

    private Boat CreateBoat(Vector3 position, Quaternion rotation)
    {
        int id = boatPrefab.BoatData.BoatId;
        int instanceId = InstancesManager.instance.GetNextInstanceId();
        float health = boatPrefab.Health.MaxHealth;
        int dockInstanceId = GetNearestDockPoint(position).InstanceId.id;

        BoatData data = new BoatData(id, instanceId, BoatStateEnum.MovingToDock, position, rotation.eulerAngles, health, dockInstanceId);
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
        int floorAmount = BuildingsManager.instance.BuiltFloors.Count;
        int raidersAmount = Random.Range(floorAmount / 2, floorAmount + 1);
        return raidersAmount;
    }

    private float GetRandomCooldown()
    {
        float cooldown = Random.Range(minRaidCooldown, maxRaidCooldown);
        return cooldown;
    }
}