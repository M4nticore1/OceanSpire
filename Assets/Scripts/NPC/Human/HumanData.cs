using System;
using UnityEngine;

[Serializable]
public class HumanData : CreatureData
{
    public NameData Name = NameData.Default();
    public HealthData Health = HealthData.Default();
    public InteractionComponentData Interaction = InteractionComponentData.Default();
    public CityNavigatorData CityNavigator = CityNavigatorData.Default();
    public BoatRiderData BoatRider = BoatRiderData.Default();
    public EquipmentData Weapon = EquipmentData.Default();
    public SkillsData Skills = SkillsData.Default();

    public static HumanData Create(Human human)
    {
        var humanData = new HumanData();
        humanData.FillHumanData(human);

        return humanData;
    }

    protected void FillHumanData(Human human)
    {
        Id = human.Definition.CreatureId;
        InstanceId = human.InstanceId.GetId();
        Position = new Vector3Data(human.transform.position);
        Rotation = new Vector3Data(human.transform.rotation.eulerAngles);
        Health = HealthData.Create(human.HealthComponent);
        CityNavigator = CityNavigatorData.Create(human.CityNavigator);
        Interaction = InteractionComponentData.Create(human.InteractComponent);
        Name = NameData.Create(human.NameComponent);
        BoatRider = BoatRiderData.Create(human.BoatRider);
        Weapon = EquipmentData.Create(human.WeaponComponent);
        Skills = SkillsData.Create(human.SkillsComponent);
    }
}
