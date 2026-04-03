using System.Collections.Generic;
using UnityEngine;

public class RaidManager : MonoBehaviour
{
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

    [Header("Positions")]
    [SerializeField] private BoatDockPoint[] dockPoints;
    private List<Human> spawnedRaiders = new List<Human>();
    private List<Boat> spawnedBoats = new List<Boat>();

    private bool isUnderRaid = false;

    public static event System.Action onRaidStarted;
    public static event System.Action onRaidFinished;

    private void OnEnable()
    {
        Creature.onCreatureDeath += OnCreatureDeath;
    }

    private void OnDisable()
    {
        Creature.onCreatureDeath -= OnCreatureDeath;
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

        StartRaid();
        ResetCurrentRaidTime();
        ApplyRandomCooldown();
    }

    private void StartRaid()
    {
        int raidersAmount = GetRandomRaidersAmount();
        Vector3 dir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        dir.Normalize();

        for (int i = 0; i < raidersAmount; i++) {
            float angle = Random.Range(minSpawnAngleOffset, maxSpawnAngleOffset);
            dir = Quaternion.Euler(0f, angle, 0f) * dir;

            Vector3 position = dir * spawnDistance;
            Quaternion rotation = Quaternion.LookRotation(-position.normalized);

            Boat boat = CreateBoat(position, rotation);
            boat.SetDockPoint(GetNearestDockPoint(boat));

            Human raider = CreateRaider(position, rotation);
            raider.BoatRider.EnterBoat(boat);
            raider.SetInteractBuilding(GetRandomRaidBuilding());
        }

        isUnderRaid = true;
        onRaidStarted?.Invoke();
    }

    private void FinishRaid()
    {
        DestroyBoats();

        isUnderRaid = false;
        onRaidFinished?.Invoke();
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
        foreach (var boat in spawnedBoats) {
            Destroy(boat.gameObject);
        }
    }

    private void OnCreatureDeath(Creature creature)
    {
        Human human = creature as Human;
        if (human) return;

        if (!spawnedRaiders.Contains(human)) return;

        spawnedRaiders.Remove(human);

        if (spawnedRaiders.Count == 0) {
            FinishRaid();
        }
    }

    private Human CreateRaider(Vector3 position, Quaternion rotation)
    {
        int id = (int)CreatureIdEnum.Human;
        HumanEntry data = new HumanEntry(id, HumanStateEnum.Raider, position, rotation.eulerAngles);
        Human human = CreatureFactory.CreateHuman(data);
        spawnedRaiders.Add(human);
        return human;
    }

    private Boat CreateBoat(Vector3 position, Quaternion rotation)
    {
        int id = boatPrefab.BoatData.BoatId;
        float health = boatPrefab.Health.MaxHealth;
        BoatEntry data = new BoatEntry(id, position, rotation.eulerAngles, health);
        Boat boat = BoatFactory.CreateBoat(data);
        spawnedBoats.Add(boat);
        return boat;
    }

    private Building GetRandomRaidBuilding()
    {
        Building building = null;
        int floorIndex = Random.Range(-1, BuildingsManager.instance.BuiltFloors.Count);

        if (floorIndex == -1) {
            building = BuildingsManager.instance.TowerGate;
        }
        else {
            int placeIndex = 0;

            while (!building || !building.BuildingData.IsRaidable) {
                floorIndex = Random.Range(0, BuildingsManager.instance.BuiltFloors.Count);
                placeIndex = Random.Range(0, BuildingsManager.RoomsCountPerFloor);
                building = BuildingsManager.instance.BuiltFloors[floorIndex].RoomBuildingPlaces[placeIndex].PlacedBuilding;
            }
        }

        return building;
    }

    private BoatDockPoint GetNearestDockPoint(Boat boat)
    {
        BoatDockPoint dockPoint = dockPoints[0];
        float distance = Vector3.Distance(boat.transform.position, dockPoint.transform.position);

        for (int i = 1; i < dockPoints.Length; i++) {
            if (dockPoints[i].boat != null) continue;

            float currentDistance = Vector3.Distance(boat.transform.position, dockPoints[i].transform.position);
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