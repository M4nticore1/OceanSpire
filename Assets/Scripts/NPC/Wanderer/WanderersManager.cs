using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WanderersManager : MonoBehaviour
{
    public static WanderersManager Instance;

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
        var human = CreateWanderer(position, rotation, boat.InstanceId.Id);
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
        AssignDockPoints();
    }

    private void AssignDockPoints()
    {
        var wanderers = CreaturesManager.Instance.Wanderers;
        int dockIndex = 0;

        for (int i = 0; i < wanderers.Count; i++) {
            var wanderer = wanderers[i];
            var boat = wanderer.BoatRider.SelectedBoat;

            if (!boat.DockPoint) continue;

            boat.RemoveDockPoint();
            boat.SetDockPoint(DockPointsManager.Instance.WandererDockPoints[dockIndex]);
            boat.SetState(BoatStateEnum.MovingToDock);
            dockIndex++;
        }
    }

    private Human CreateWanderer(Vector3 position, Vector3 rotation, int boatId)
    {
        var id = (int)wandererIds[UnityEngine.Random.Range(0, wandererIds.Length)];
        var prefab = creaturesList.GetCreature(id) as Human;

        var data = new WandererData()
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
            Skills = SkillsFactory.CreateRandomSkillsData(SkillsFactory.GetLevelsCount()),
            SpawnPosition = new Vector3Data(position)
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