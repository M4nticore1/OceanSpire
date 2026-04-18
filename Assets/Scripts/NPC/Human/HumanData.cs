using System;
using UnityEngine;

[Serializable]
public class HumanDataV1 : CreatureDataV1
{
    public HumanStatusEnum status { get; private set; } = HumanStatusEnum.Citizen;
    public bool isMale { get; private set; } = true;
    public float health { get; private set; } = 0f;
    public int interactBuildingInstanceId { get; private set; } = 0;
    public NameData name { get; private set; }
    public BoatRiderData boatRider { get; private set; }
    public WeaponHandlerData weapon { get; private set; }
    public SkillsData skills { get; private set; }

    public HumanDataV1(int id,
        int instanceId,
        Vector3 position,
        Vector3 rotation,
        float health,
        HumanStatusEnum status,
        int interactBuildingInstanceId,
        NameData name,
        BoatRiderData boatRider,
        WeaponHandlerData weapon,
        SkillsData skills) :
        base(id, instanceId, position, rotation)
    {
        this.health = health;
        this.status = status;
        this.interactBuildingInstanceId = interactBuildingInstanceId;
        this.name = name;
        this.boatRider = boatRider;
        this.weapon = weapon;
        this.skills = skills;
    }
}
