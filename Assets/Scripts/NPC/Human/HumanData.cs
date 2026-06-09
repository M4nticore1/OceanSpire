using System;
using UnityEngine;

[Serializable]
public class HumanData : CreatureData
{
    public float Health = 0f;
    public int? EnteredBuildingInstanceId = null;
    public int? InteractBuildingInstanceId = null;
    public int MovementStateId = 0;
    public NameData Name;
    public BoatRiderData BoatRider;
    public EquipmentData Weapon;
    public SkillsData Skills;

    public static HumanData Create(Human human)
    {
        var humanData = new HumanData();
        humanData.FillHumanData(human);

        return humanData;
    }

    protected void FillHumanData(Human human)
    {
        Id = human.Definition.CreatureId;
        InstanceId = human.InstanceId.GetInstanceId();
        Position = new Vector3Data(human.transform.position);
        Rotation = new Vector3Data(human.transform.rotation.eulerAngles);
        Health = human.HealthComponent.CurrentHealth;
        EnteredBuildingInstanceId = human.CityNavigator.CurrentBuilding?.InstanceId.GetInstanceId();
        InteractBuildingInstanceId = human.InteractComponent.InteractBuilding?.InstanceId.GetInstanceId();
        MovementStateId = (int)human.CityNavigator.CurrentState;
        Name = NameData.Create(human.NameComponent);
        BoatRider = BoatRiderData.Create(human.BoatRider);
        Weapon = EquipmentData.Create(human.WeaponComponent);
        Skills = SkillsData.Create(human.SkillsComponent);
    }
}
