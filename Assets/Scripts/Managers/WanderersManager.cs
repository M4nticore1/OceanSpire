using System.Collections.Generic;
using UnityEngine;

public class WanderersManager : MonoBehaviour
{
    public static WanderersManager instance;

    [Header("Prefabs")]
    [SerializeField] private Human humanPrefab;
    [SerializeField] private Boat boatPrefab;

    [Header("Cooldown")]
    [SerializeField] private float minWandererSpawnCooldown = 10;
    [SerializeField] private float maxWandererSpawnCooldown = 10;
    private float currentWandererSpawnCooldown = 0;
    private float currentWandererSpawnTime = 0;

    [Header("Positions")]
    private Dictionary<Human, Vector3> spawnPositions = new Dictionary<Human, Vector3>();

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
        Human.onWandererAccepted += OnWandererAccepted;
        Human.onWandererRejected += OnWandererRejected;
    }

    private void OnDisable()
    {
        Human.onWandererAccepted -= OnWandererAccepted;
        Human.onWandererRejected -= OnWandererRejected;
    }

    private void Start()
    {
        ApplyRandomCooldown();
    }

    private void Update()
    {
        if (!CanSpawn()) return;

        currentWandererSpawnTime += Time.deltaTime;
        if (currentWandererSpawnTime <= currentWandererSpawnCooldown)return;

        SpawnWanderer();
        ResetTimeToSpawn();
        ApplyRandomCooldown();
    }

    private void SpawnWanderer()
    {
        Vector3 position = WorldUtils.GetRandomBorderPosition();
        Vector3 rotation = Quaternion.LookRotation(-position.normalized).eulerAngles;

        Boat boat = CreateBoat(position, rotation);
        Human human = CreateWanderer(position, rotation, boat.instanceId, true);

        spawnPositions.Add(human, position);
    }

    private void ResetTimeToSpawn()
    {
        currentWandererSpawnTime = 0f;
    }

    private void ApplyRandomCooldown()
    {
        currentWandererSpawnCooldown = GetRandomCooldown();
    }

    private void OnWandererAccepted(Human human)
    {
        AssignDockPoints();
    }

    private void OnWandererRejected(Human human)
    {
        Vector3 position = spawnPositions[human];
        human.BoatRider.selectedBoat.SetState(BoatStateEnum.FloatingAway);
        human.BoatRider.selectedBoat.Movement.TryMoveTo(position);

        spawnPositions.Remove(human);

        AssignDockPoints();
    }

    private void AssignDockPoints()
    {
        List<Human> wanderers = CreaturesManager.instance.wanderers;

        for (int i = 0; i < wanderers.Count; i++) {
            Human wanderer = wanderers[i];
            Boat boat = wanderer.BoatRider.selectedBoat;

            boat.RemoveDockPoint();
            boat.SetDockPoint(DockPointsManager.instance.WandererDockPoints[i]);
            boat.SetState(BoatStateEnum.MovingToDock);
        }
    }

    private Human CreateWanderer(Vector3 position, Vector3 rotation, int boatId, bool isRidingOnBoat)
    {
        int creatureId = (int)CreatureIdEnum.Human;

        int instanceId = InstancesManager.instance.GetNextInstanceId();
        InstancesManager.instance.AddInstanceId(instanceId);

        HumanStateEnum status = HumanStateEnum.Wanderer;

        HumanEntry humanData = new HumanEntry(creatureId, instanceId, status, position, rotation, boatId, isRidingOnBoat);
        Human human = CreatureFactory.CreateHuman(humanData);

        return human;
    }

    private Boat CreateBoat(Vector3 position, Vector3 rotation)
    {
        int boatId = (int)BoatIdEnum.BasicBoat;

        int instanceId = InstancesManager.instance.GetNextInstanceId();
        InstancesManager.instance.AddInstanceId(instanceId);

        float boatHealth = BoatsList.Instance.boats[boatId].Health.MaxHealth;

        BoatEntry boatData = new BoatEntry(boatId, instanceId, BoatStateEnum.MovingToDock, position, rotation, boatHealth, GetDockPoint().InstanceId.id);
        Boat boat = BoatFactory.CreateBoat(boatData);

        return boat;
    }


    private BoatDockPoint GetDockPoint()
    {
        foreach (var dockPoint in DockPointsManager.instance.WandererDockPoints) {
            if (dockPoint.boat) continue;

            return dockPoint;
        }

        return null;
    }

    private bool CanSpawn()
    {
        if (CreaturesManager.instance.wanderers.Count >= DockPointsManager.instance.WandererDockPoints.Length) return false;

        return true;
    }

    private float GetRandomCooldown()
    {
        float cooldown = Random.Range(minWandererSpawnCooldown, maxWandererSpawnCooldown);
        return cooldown;
    }
}