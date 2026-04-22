using UnityEngine;

public static class HumanDataFactory
{
    public static HumanDataV1 CreateRandomCitizenData()
    {
        int id = (int)CreatureIdEnum.Human;
        int instanceId = InstancesManager.instance.GetNextInstanceId();
        float health = CreaturesList.Instance.Creatures[id].GetComponent<HealthComponent>().MaxHealth;
        HumanStatusEnum status = HumanStatusEnum.Citizen;

        int firstNameId = HumanNamesList.Instance.GetRandomMaleFirstNameId();
        int lastNameId = HumanNamesList.Instance.GetRandomMaleLastNameId();
        NameData nameData = new NameData(firstNameId, lastNameId);

        BoatRiderData boatRiderData = new BoatRiderData(-1, false);

        int damage = WeaponsDataFactory.GetMinWeaponDamageId();
        WeaponHandlerData weaponsData = WeaponsDataFactory.CreateRandomDataGenerator(damage, damage);

        SkillsData skillsData = SkillsFactory.CreateRandomSkillsData(SkillsFactory.GetLevelsCount());

        HumanDataV1 humanData = new HumanDataV1(id, instanceId, Vector3.zero, Vector3.zero, health, status, -1, nameData, boatRiderData, weaponsData, skillsData);
        
        return humanData;
    }

    public static HumanDataV1 CreateRandomWandererData()
    {
        int id = (int)CreatureIdEnum.Human;
        int instanceId = InstancesManager.instance.GetNextInstanceId();
        float health = CreaturesList.Instance.Creatures[id].GetComponent<HealthComponent>().MaxHealth;
        HumanStatusEnum status = HumanStatusEnum.Wanderer;

        int firstNameId = HumanNamesList.Instance.GetRandomMaleFirstNameId();
        int lastNameId = HumanNamesList.Instance.GetRandomMaleLastNameId();
        NameData nameData = new NameData(firstNameId, lastNameId);

        BoatRiderData boatRiderData = new BoatRiderData(-1, false);

        int damage = WeaponsDataFactory.GetMinWeaponDamageId();
        WeaponHandlerData weaponsData = WeaponsDataFactory.CreateRandomDataGenerator(damage, damage);

        SkillsData skillsData = SkillsFactory.CreateRandomSkillsData(SkillsFactory.GetLevelsCount());

        HumanDataV1 humanData = new HumanDataV1(id, instanceId, Vector3.zero, Vector3.zero, health, status, -1, nameData, boatRiderData, weaponsData, skillsData);

        return humanData;
    }

    public static HumanDataV1 CreateRandomRaiderData()
    {
        int id = (int)CreatureIdEnum.Human;
        int instanceId = InstancesManager.instance.GetNextInstanceId();
        float health = CreaturesList.Instance.Creatures[id].GetComponent<HealthComponent>().MaxHealth;
        HumanStatusEnum status = HumanStatusEnum.Raider;

        int firstNameId = HumanNamesList.Instance.GetRandomMaleFirstNameId();
        int lastNameId = HumanNamesList.Instance.GetRandomMaleLastNameId();
        NameData nameData = new NameData(firstNameId, lastNameId);

        BoatRiderData boatRiderData = new BoatRiderData(-1, false);
        WeaponHandlerData weaponsData = WeaponsDataFactory.CreateRandomDataGenerator(WeaponsDataFactory.GetMinWeaponDamageId() + 1, WeaponsDataFactory.GetMaxWeaponDamage());
        SkillsData skillsData = SkillsFactory.CreateRandomSkillsData(SkillsFactory.GetLevelsCount());

        HumanDataV1 data = new HumanDataV1(id, instanceId, Vector3.zero, Vector3.zero, health, status, -1, nameData, boatRiderData, weaponsData, skillsData);

        return data;
    }
}
