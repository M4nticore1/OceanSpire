using System;
using UnityEngine;

[Serializable]
public class HumanData : CreatureData
{
    public float Health = 0f;
    public int? EnteredBuildingInstanceId = null;
    public int? InteractBuildingInstanceId = null;
    public bool RidingOnElevator = false;
    public NameData Name;
    public BoatRiderData BoatRider;
    public EquipmentData Weapon;
    public SkillsData Skills;

    public static HumanData Create(Human human)
    {
        return new HumanData()
        {
            Id = human.Definition.CreatureId,
            InstanceId = human.InstanceId.Id,
            Position = new Vector3Data(human.transform.position),
            Rotation = new Vector3Data(human.transform.rotation.eulerAngles),
            Health = human.HealthComponent.CurrentHealth,
            EnteredBuildingInstanceId = human.CityNavigator.CurrentBuilding?.InstanceId.Id,
            InteractBuildingInstanceId = human.InteractComponent.InteractBuilding?.InstanceId.Id,
            RidingOnElevator = human.CityNavigator.IsRidingOnElevator,
            Name = NameData.Create(human.NameComponent),
            BoatRider = BoatRiderData.Create(human.BoatRider),
            Weapon = EquipmentData.Create(human.WeaponComponent),
            Skills = SkillsData.Create(human.SkillsComponent),
        };
    }
}
