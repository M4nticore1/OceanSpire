using System;
using UnityEngine;

public static class WandererAdmissionSystem
{
    public static event Action<Human> OnWandererAccepted;
    public static event Action<Human> OnWandererRejected;

    public static void AcceptWanderer(Wanderer wanderer)
    {
        int creautereId = wanderer.GenderComponent.IsMale ? (int)CreatureIdEnum.HumanCitizenMale : (int)CreatureIdEnum.HumanCitizenFemale;
        var prefab = CreaturesList.Instance.GetCreature(creautereId);
        var position = wanderer.BoatRider.SelectedBoat.DockPoint.EntraceTransform.position;
        var rotaton = wanderer.BoatRider.SelectedBoat.DockPoint.EntraceTransform.rotation;

        var data = new CitizenData()
        {
            Id = creautereId,
            InstanceId = InstancesManager.Instance.GetNextInstanceId(),
            Health = wanderer.HealthComponent.CurrentHealth,
            Position = new Vector3Data(position),
            Rotation = new Vector3Data(rotaton.eulerAngles),
            Name = NameData.Create(wanderer.NameComponent),
            BoatRider = new BoatRiderData(),
            Weapon = EquipmentData.Create(wanderer.WeaponComponent),
            Skills = SkillsData.Create(wanderer.SkillsComponent)
        };

        GameObject.Destroy(wanderer.BoatRider.SelectedBoat.gameObject);
        var citizen = CreatureFactory.CreateHuman(prefab, position, rotaton, data);

        OnWandererAccepted?.Invoke(citizen);
    }

    public static void RejectWanderer(Wanderer wanderer)
    {
        wanderer.Reject();
        OnWandererRejected?.Invoke(wanderer);
    }
}