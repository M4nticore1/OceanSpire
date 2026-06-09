using System.Linq;
using UnityEngine;

public class WanderersManager : MonoBehaviour
{
    public static WanderersManager Instance;

    [Header("Main")]
    [SerializeField] private CreaturesList creaturesList;
    [SerializeField] private BoatsList boatsList;
    [SerializeField] private HumanNamesList humanNamesList;

    [Header("Prefabs")]
    [SerializeField] private Creature[] wandererPrefabs;
    [SerializeField] private Boat boatPrefab;

    [Header("Cooldown")]
    [SerializeField] private float minWandererSpawnCooldown = 10;
    [SerializeField] private float maxWandererSpawnCooldown = 10;

    public float CurrentWandererSpawnCooldown { get; private set; } = 0;
    public float CurrentWandererSpawnTime { get; private set; } = 0;

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
        WandererAdmissionSystem.OnWandererAccepted += OnWandererAccepted;
        WandererAdmissionSystem.OnWandererRejected += OnWandererRejected;
    }

    private void OnDisable()
    {
        WandererAdmissionSystem.OnWandererAccepted -= OnWandererAccepted;
        WandererAdmissionSystem.OnWandererRejected -= OnWandererRejected;
    }

    private void Start()
    {
        ApplyRandomCooldown();
    }

    public void Init(WanderersData wanderersData)
    {
        CurrentWandererSpawnCooldown = wanderersData.Cooldown;
        CurrentWandererSpawnTime = wanderersData.TimeSinceLastSpawn;
    }

    private void Update()
    {
        if (!CanSpawn()) return;

        CurrentWandererSpawnTime += Time.deltaTime;
        if (CurrentWandererSpawnTime <= CurrentWandererSpawnCooldown)return;

        SpawnWanderer();
        ResetTimeToSpawn();
        ApplyRandomCooldown();
    }

    public float CalculateRandomCooldown()
    {
        float cooldown = Random.Range(minWandererSpawnCooldown, maxWandererSpawnCooldown);
        return cooldown;
    }

    private void SpawnWanderer()
    {
        Vector3 position = WorldUtils.GetRandomBorderPosition();
        Vector3 rotation = Quaternion.LookRotation(-position.normalized).eulerAngles;

        var boat = CreateBoat(position, rotation);
        var human = CreateWanderer(position, rotation, boat.InstanceId.GetInstanceId());
    }

    private void ResetTimeToSpawn()
    {
        CurrentWandererSpawnTime = 0f;
    }

    private void ApplyRandomCooldown()
    {
        CurrentWandererSpawnCooldown = CalculateRandomCooldown();
    }

    private void OnWandererAccepted(Human human)
    {
        UpdateDockPoints();
    }

    private void OnWandererRejected(Human human)
    {
        UpdateDockPoints();
    }

    private void UpdateDockPoints()
    {
        var wanderers = CreaturesManager.Instance.Wanderers;
        int dockIndex = 0;

        for (int i = 0; i < wanderers.Count; i++) {
            var wanderer = wanderers[i];
            if (wanderer.IsAccepted) continue;

            var boat = wanderer.BoatRider.RidingBoat;
            if (!boat.DockPoint) continue;

            boat.RemoveDockPoint();
            boat.SetDockPoint(DockPointsManager.Instance.WandererDockPoints[dockIndex]);
            boat.SetState(BoatStateEnum.MovingToDock);
            dockIndex++;
        }
    }

    private Human CreateWanderer(Vector3 position, Vector3 rotation, int boatInstanceId)
    {
        var prefab = wandererPrefabs[UnityEngine.Random.Range(0, wandererPrefabs.Length)] as Human;

        var data = new WandererData()
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
                RidingBoatInstanceId = boatInstanceId,
            },

            Weapon = WeaponsDataFactory.CreateRandomData(WeaponsDataFactory.GetMinWeaponDamageId(), WeaponsDataFactory.GetMaxWeaponDamage()),
            Skills = SkillsFactory.CreateRandomSkillsData(SkillsFactory.GetLevelsCount()),
            SpawnPosition = new Vector3Data(position)
        };

        var human = CreatureFactory.CreateHuman(prefab, position, Quaternion.Euler(rotation), data);

        return human;
    }

    private Boat CreateBoat(Vector3 position, Vector3 rotation)
    {
        var boatData = new BoatData()
        {
            Id = boatPrefab.Definition.BoatId,
            InstanceId = InstancesManager.Instance.GetNextInstanceId(),
            Position = new Vector3Data(position),
            Rotation = new Vector3Data(rotation),
            DockInstanceId = GetDockPoint().InstanceId.GetInstanceId(),
            Status = HumanStatusEnum.Wanderer
        };

        var boat = BoatFactory.CreateBoat(boatPrefab, position, Quaternion.Euler(rotation), boatData);

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
}