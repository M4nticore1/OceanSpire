using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreaturesLoader : WorldLoader
{
    [SerializeField] private CreaturesList creaturesList;
    [SerializeField] private HumanNamesList humanNamesList;
    [SerializeField] private CreatureIdEnum[] citizenIds;

    [SerializeField] private int startResidentsCount = 2;
    [SerializeField] private SpawnArea spawnArea;

    protected override void Load(WorldData data)
    {
        if (data != null && data.Citizens != null) {
            LoadHumans(data.Citizens.Cast<HumanData>().ToList());
        }
        else {
            InitCitizens();
        }

        if (data != null && data.Wanderers != null) {
            LoadHumans(data.Wanderers.Cast<HumanData>().ToList());
        }

        if (data != null && data.Raiders != null) {
            LoadHumans(data.Raiders.Cast<HumanData>().ToList());
        }
    }

    private void LoadHumans(List<HumanData> humansData)
    {
        foreach (var data in humansData) {
            if (data == null) {
                Debug.LogError($"[{nameof(CreaturesLoader)}] Save human data is null");
                continue;
            }

            var prefab = creaturesList.GetCreature(data.Id);
            CreatureFactory.CreateHuman(prefab, data);
        }
    }

    private void InitCitizens()
    {

        for (int i = 0; i < startResidentsCount; i++) {
            var position = spawnArea.GetRandomSpawnPosition();
            var rotation = spawnArea.transform.rotation.eulerAngles;

            var citizenId = citizenIds[Random.Range(0, citizenIds.Length)];

            var prefab = creaturesList.GetCreature(citizenId) as Citizen;
            if (!prefab) {
                Debug.LogError($"[{nameof(CreaturesLoader)}] Citizen prefab is not valid");
                continue;
            }

            var levelsCount = Mathf.Max(1, SkillsData.GetLevelsCountByGameStage());

            var citizenData = new CitizenData()
            {
                Id = citizenId,
                Position = new Vector3Data(position),
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
                Skills = SkillsData.CreateByLevelsCount(levelsCount),
            };

            var citizen = CreatureFactory.CreateHuman(prefab, citizenData);
        }
    }
}