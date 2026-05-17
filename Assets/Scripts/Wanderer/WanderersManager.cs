using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WanderersManager : MonoBehaviour
{
    public static WanderersManager instance;

    [SerializeField] private CreaturesList creaturesList;
    [SerializeField] private BoatsList boatsList;
    [SerializeField] private HumanNamesList humanNamesList;

    [SerializeField] private CreatureIdEnum[] wandererIds;
    [SerializeField] private BoatIdEnum wandererBoatId;

    [Header("Cooldown")]
    [SerializeField] private float minWandererSpawnCooldown = 10;
    [SerializeField] private float maxWandererSpawnCooldown = 10;
    public float CurrentWandererSpawnCooldown { get; private set; } = 0;
    public float CurrentWandererSpawnTime { get; private set; } = 0;

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

        Boat boat = CreateBoat(position, rotation);
        Human human = CreateWanderer(position, rotation, boat.InstanceId.Id);

        spawnPositions.Add(human, position);
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
        HumanDataFactory.CreateRandomWandererData();

        var id = (int)wandererIds[UnityEngine.Random.Range(0, wandererIds.Length)];
        var prefab = creaturesList.GetCreature(id) as Human;

        var data = new HumanData()
        {
            Id = id,
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
                BoatInstanceId = boatId,
                Riding = true
            },

            Weapon = WeaponsDataFactory.CreateRandomData(WeaponsDataFactory.GetMinWeaponDamageId(), WeaponsDataFactory.GetMaxWeaponDamage()),
            Skills = SkillsFactory.CreateRandomSkillsData(SkillsFactory.GetLevelsCount())
        };

        var human = CreatureFactory.CreateHuman(prefab, position, Quaternion.Euler(rotation), data);

        return human;
    }

    private Boat CreateBoat(Vector3 position, Vector3 rotation)
    {
        var prefab = boatsList.GetBoat((int)wandererBoatId);

        var boatData = new BoatData()
        {
            Id = prefab.Definition.BoatId,
            InstanceId = InstancesManager.Instance.GetNextInstanceId(),
            Position = new Vector3Data(position),
            Rotation = new Vector3Data(rotation),
            DockInstanceId = GetDockPoint().InstanceId.Id
        };

        var boat = BoatFactory.CreateBoat(prefab, position, Quaternion.Euler(rotation), boatData);

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