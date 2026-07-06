using UnityEngine;

public class CreaturesLoader : WorldLoader
{
    [SerializeField] private CreaturesList creaturesList;
    [SerializeField] private HumanNamesList humanNamesList;
    [SerializeField] private CreatureIdEnum[] citizenIds;

    [SerializeField] private int startResidentsCount = 2;
    [SerializeField] private Transform entitySpawnPosition;
    [SerializeField] public float maxSpawnRange = 5f;

    protected override void Load(WorldData data)
    {
        if (data != null && data.Citizens != null) {
            LoadHumans(data.Citizens);
        }
        else {
            InitCitizens();
        }

        if (data != null && data.Wanderers != null) {
            LoadHumans(data.Wanderers);
        }

        if (data != null && data.Raiders != null) {
            LoadHumans(data.Raiders);
        }
    }

    private void LoadHumans(HumanData[] humansData)
    {
        foreach (var data in humansData) {
            if (data == null) {
                Debug.Log("Save human data is null");
                return;
            }

            var prefab = creaturesList.GetCreature(data.Id);
            var citizen = CreatureFactory.CreateHuman(prefab, data.Position.Vector3(), Quaternion.Euler(data.Rotation.Vector3()), data);
        }
    }

    private void InitCitizens()
    {
        var position = entitySpawnPosition.position;
        var rotation = entitySpawnPosition.rotation.eulerAngles;

        for (int i = 0; i < startResidentsCount; i++) {
            float x = Random.Range(position.x - maxSpawnRange, position.x + maxSpawnRange);
            float y = position.y;
            float z = Random.Range(position.z - maxSpawnRange, position.z + maxSpawnRange);

            var finalPosition = new Vector3(x, y, z);

            int citizenId = (int)citizenIds[Random.Range(0, citizenIds.Length)];
            var prefab = creaturesList.GetCreature(citizenId) as Citizen;

            var skillsCount = Mathf.Max(1, SkillsFactory.GetLevelsCount());

            var citizenData = new CitizenData()
            {
                Id = citizenId,
                Position = new Vector3Data(finalPosition),
                Rotation = new Vector3Data(rotation),

                Health = new HealthData()
                {
                    CurrentHealth = prefab.HealthComponent.MaxHealth
                },

                Name = new NameData()
                {
                    FirstNameId = prefab.GenderComponent.IsMale ? humanNamesList.GetRandomMaleFirstNameId() : humanNamesList.GetRandomFemaleFirstNameId(),
                    LastNameId = prefab.GenderComponent.IsMale ? humanNamesList.GetRandomMaleLastNameId() : humanNamesList.GetRandomFemaleLastNameId(),
                },

                BoatRider = BoatRiderData.Default(),
                Weapon = WeaponsDataFactory.CreateRandomData(WeaponsDataFactory.GetMinWeaponDamageId(), WeaponsDataFactory.GetMinWeaponDamageId()),
                Skills = SkillsFactory.CreateRandomSkillsData(skillsCount),
            };

            var citizen = CreatureFactory.CreateHuman(prefab, finalPosition, Quaternion.Euler(rotation), citizenData);
        }
    }
}