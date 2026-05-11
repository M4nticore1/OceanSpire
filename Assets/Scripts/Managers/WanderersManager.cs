using System.Collections.Generic;
using System.Linq;
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
        Human human = CreateWanderer(position, rotation, boat.InstanceId.Id);

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
        human.BoatRider.SelectedBoat.SetState(BoatStateEnum.FloatingAway);
        human.BoatRider.SelectedBoat.Movement.TryMoveTo(position);

        spawnPositions.Remove(human);

        AssignDockPoints();
    }

    private void AssignDockPoints()
    {
        List<Human> wanderers = CreaturesManager.Instance.Wanderers.ToList();

        for (int i = 0; i < wanderers.Count; i++) {
            Human wanderer = wanderers[i];
            Boat boat = wanderer.BoatRider.SelectedBoat;

            boat.RemoveDockPoint();
            boat.SetDockPoint(DockPointsManager.Instance.WandererDockPoints[i]);
            boat.SetState(BoatStateEnum.MovingToDock);
        }
    }

    private Human CreateWanderer(Vector3 position, Vector3 rotation, int boatId)
    {
        HumanData data = HumanDataFactory.CreateRandomWandererData();
        data.Position = new Vector3Data(position);
        data.Rotation = new Vector3Data(rotation);
        data.BoatRider.SetBoatInstanceId(boatId);
        data.BoatRider.SetRiding(true);

        Human human = CreatureFactory.CreateHuman(data);

        return human;
    }

    private Boat CreateBoat(Vector3 position, Vector3 rotation)
    {
        int id = boatPrefab.BoatData.BoatId;
        int instanceId = InstancesManager.Instance.GetNextInstanceId();
        float boatHealth = boatPrefab.Health.MaxHealth;

        BoatData boatData = new BoatData()
        {
            Id = id,
            InstanceId = instanceId,
            Position = new Vector3Data(position),
            Rotation = new Vector3Data(rotation),
            Health = boatHealth,
            DockInstanceId = GetDockPoint().InstanceId.Id
        };

        Boat boat = BoatFactory.CreateBoat(boatPrefab, boatData);

        return boat;
    }


    private BoatDockPoint GetDockPoint()
    {
        return DockPointsManager.Instance.WandererDockPoints[CreaturesManager.Instance.Wanderers.Count()];
    }

    private bool CanSpawn()
    {
        if (CreaturesManager.Instance.Wanderers.Count >= DockPointsManager.Instance.WandererDockPoints.Length) return false;

        return true;
    }

    private float GetRandomCooldown()
    {
        float cooldown = Random.Range(minWandererSpawnCooldown, maxWandererSpawnCooldown);
        return cooldown;
    }
}