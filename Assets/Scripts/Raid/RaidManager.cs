using System.Collections.Generic;
using UnityEngine;

public class RaidManager : MonoBehaviour
{
    public static RaidManager instance;

    [SerializeField] private Inventory inventory;
    public Inventory Inventory => inventory;

    [Header("Prefabs")]
    [SerializeField] private Human attackerPrefab;
    [SerializeField] private Boat boatPrefab;

    [Header("Cooldown")]
    [SerializeField] private float minRaidCooldown = 10f;
    [SerializeField] private float maxRaidCooldown = 20f;
    private float currentRaidCooldown = 0f;
    private float currentRaidTime = 0f;

    [Header("Spawn")]
    [SerializeField] private float minSpawnAngleOffset = 5f;
    [SerializeField] private float maxSpawnAngleOffset = 10f;
    [SerializeField] private float spawnDistance = 145f;

    private int aliveRaidersCount = 0;

    [Header("Positions")]
    [SerializeField] private BoatDockPoint[] dockPoints;
    private Dictionary<Boat, Vector3> spawnPositions;

    private bool isUnderRaid = false;
    private int landedRaidersCount = 0;

    public static event System.Action onRaidStarted;
    public static event System.Action onRaidEnded;

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
        if (isUnderRaid) return;

        currentRaidTime += Time.deltaTime;
        if (currentRaidTime < currentRaidCooldown) return;

        CreateRaid();
        ResetCurrentRaidTime();
        ApplyRandomCooldown();
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

    private void CreateRaid()
    {
        int raidersAmount = GetRandomRaidersAmount();
        aliveRaidersCount = raidersAmount;

        Vector3 dir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        dir.Normalize();

        spawnPositions = new Dictionary<Boat, Vector3>();

        for (int i = 0; i < raidersAmount; i++) {
            float angle = Random.Range(minSpawnAngleOffset, maxSpawnAngleOffset);
            dir = Quaternion.Euler(0f, angle, 0f) * dir;

            Vector3 position = dir * spawnDistance;
            Quaternion rotation = Quaternion.LookRotation(-position.normalized);

            Boat boat = CreateBoat(position, rotation);

            Human raider = CreateRaider(position, rotation, boat.InstanceId.id, true);
            raider.BoatRider.EnterBoat();
            raider.SetInteractBuilding(GetRandomRaidBuilding());

            spawnPositions.Add(boat, position);
        }

        isUnderRaid = true;
    }

    private void StartRaid()
    {
        ClearLosses();
        onRaidStarted?.Invoke();
    }

    private void StopRaid()
    {
        RemoveCityLoot();
        isUnderRaid = false;
        onRaidEnded?.Invoke();
    }

    private void RemoveCityLoot()
    {
        for (int i = 0; i < inventory.items.Count; i++) {
            ItemInstance item = inventory.items[i].item;

            int id = item.ItemData.ItemId;
            int amount = item.Amount;

            CityStorage.instance.Inventory.RemoveItemAmount(id, amount);
        }
    }

    private void ClearLosses()
    {
        for (int i = 0; i < inventory.items.Count; i++) {
            ItemInstance item = inventory.items[i].item;

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
        currentRaidTime = 0;
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
            StopRaid();
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
        Debug.Log("AliveRaiders: " + aliveRaidersCount);

        if (aliveRaidersCount <= 0) {
            StopRaid();
        }
    }

    private Human CreateRaider(Vector3 position, Quaternion rotation, int boatInstanceId, bool isRidingOnBoat)
    {
        int id = (int)CreatureIdEnum.Human;
        float health = CreaturesList.Instance.Creatures[id].GetComponent<Health>().MaxHealth;

        HumanEntry data = new HumanEntry(id, HumanStatusEnum.Raider, position, rotation.eulerAngles, health, boatInstanceId, isRidingOnBoat);
        Human human = CreatureFactory.CreateHuman(data);

        return human;
    }

    private Boat CreateBoat(Vector3 position, Quaternion rotation)
    {
        int id = boatPrefab.BoatData.BoatId;
        float health = boatPrefab.Health.MaxHealth;
        int dockInstanceId = GetNearestDockPoint(position).InstanceId.id;

        BoatEntry data = new BoatEntry(id, BoatStateEnum.MovingToDock, position, rotation.eulerAngles, health, dockInstanceId);
        Boat boat = BoatFactory.CreateBoat(data);

        return boat;
    }

    private Building GetRandomRaidBuilding()
    {
        Building building = null;
        int floorIndex = 0;
        int placeIndex = 0;

        while (!building || !building.BuildingData.IsRaidable) {
            floorIndex = Random.Range(0, BuildingsManager.instance.BuiltFloors.Count);
            placeIndex = Random.Range(0, BuildingsManager.RoomsCountPerFloor);
            building = BuildingsManager.instance.BuiltFloors[floorIndex].RoomBuildingPlaces[placeIndex].PlacedBuilding;
        }

        return building;
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