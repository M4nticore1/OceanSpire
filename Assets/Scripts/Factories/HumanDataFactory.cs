//using UnityEngine;

//public static class HumanDataFactory
//{
//    public static HumanData CreateRandomCitizenData()
//    {
//        Creature prefab = CreaturesList.Instance.GetRandomCitizen();
//        int id = prefab.Definition.CreatureId;

//        int instanceId = InstancesManager.Instance.GetNextInstanceId();
//        float health = prefab.GetComponent<HealthComponent>().MaxHealth;

//        int firstNameId = HumanNamesList.Instance.GetRandomMaleFirstNameId();
//        int lastNameId = HumanNamesList.Instance.GetRandomMaleLastNameId();
//        NameData nameData = new NameData()
//        {
//            FirstNameId = firstNameId,
//            LastNameId = lastNameId,
//        };

//        BoatRiderData boatRiderData = new BoatRiderData();

//        int damage = WeaponsDataFactory.GetMinWeaponDamageId();
//        EquipmentData weaponData = WeaponsDataFactory.CreateRandomData(damage, damage);

//        SkillsData skillsData = SkillsFactory.CreateRandomSkillsData(SkillsFactory.GetLevelsCount());

//        HumanData humanData = new HumanData()
//        {
//            Id = id,
//            InstanceId = instanceId,
//            Health = health,
//            Name = nameData,
//            BoatRider = boatRiderData,
//            Weapon = weaponData,
//            Skills = skillsData,
//        };

//        return humanData;
//    }

//    public static HumanData CreateRandomWandererData()
//    {
//        Creature prefab = CreaturesList.Instance.GetRandomWanderer();
//        int id = prefab.Definition.CreatureId;

//        int instanceId = InstancesManager.Instance.GetNextInstanceId();
//        float health = prefab.GetComponent<HealthComponent>().MaxHealth;

//        int firstNameId = HumanNamesList.Instance.GetRandomMaleFirstNameId();
//        int lastNameId = HumanNamesList.Instance.GetRandomMaleLastNameId();
//        NameData nameData = new NameData()
//        {
//            FirstNameId = firstNameId,
//            LastNameId = lastNameId
//        };

//        BoatRiderData boatRiderData = new BoatRiderData();

//        int damage = WeaponsDataFactory.GetMinWeaponDamageId();
//        EquipmentData weaponData = WeaponsDataFactory.CreateRandomData(damage, damage);

//        SkillsData skillsData = SkillsFactory.CreateRandomSkillsData(SkillsFactory.GetLevelsCount());

//        HumanData humanData = new HumanData()
//        {
//            Id = id,
//            InstanceId = instanceId,
//            Health = health,
//            Name = nameData,
//            BoatRider = boatRiderData,
//            Weapon = weaponData,
//            Skills = skillsData,
//        };

//        return humanData;
//    }

//    public static HumanData CreateRandomRaiderData()
//    {
//        Creature prefab = CreaturesList.Instance.GetRandomRaider();
//        int id = prefab.Definition.CreatureId;

//        int instanceId = InstancesManager.Instance.GetNextInstanceId();
//        float health = prefab.GetComponent<HealthComponent>().MaxHealth;

//        int firstNameId = HumanNamesList.Instance.GetRandomMaleFirstNameId();
//        int lastNameId = HumanNamesList.Instance.GetRandomMaleLastNameId();
//        NameData nameData = new NameData()
//        {
//            FirstNameId = firstNameId,
//            LastNameId = lastNameId
//        };

//        BoatRiderData boatRiderData = new BoatRiderData();

//        EquipmentData weaponData = WeaponsDataFactory.CreateRandomData(WeaponsDataFactory.GetMinWeaponDamageId() + 1, WeaponsDataFactory.GetMaxWeaponDamage());
//        SkillsData skillsData = SkillsFactory.CreateRandomSkillsData(SkillsFactory.GetLevelsCount());

//        HumanData humanData = new HumanData()
//        {
//            Id = id,
//            InstanceId = instanceId,
//            Health = health,
//            Name = nameData,
//            BoatRider = boatRiderData,
//            Weapon = weaponData,
//            Skills = skillsData,
//        };

//        return humanData;
//    }
//}