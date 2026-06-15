using System;
using UnityEngine;

public static class WandererAdmissionSystem
{
    public static event Action<Human> OnWandererAccepted;
    public static event Action<Human> OnWandererRejected;

    public static void AcceptWanderer(Wanderer wanderer)
    {
        if (!wanderer) {
            Debug.LogError("Wanderer not fount to access");
            return;
        }

        var boatRider = wanderer.BoatRider;
        if (!boatRider) {
            Debug.LogError($"Boat Rider not fount at {wanderer}");
            return;
        }

        var ridingBoat = boatRider.RidingBoat;
        if (!ridingBoat) {
            Debug.LogError($"Riding Boat not fount at {boatRider}");
            return;
        }

        var dockPoint = ridingBoat.DockPoint;
        if (!dockPoint) {
            Debug.LogError($"Dock Point not fount at {ridingBoat}");
            return;
        }

        var entranceTransform = dockPoint.EntraceTransform;
        if (!entranceTransform) {
            Debug.LogError($"Entrance Transform not fount at {dockPoint}");
            return;
        }

        int creautereId = wanderer.GenderComponent.IsMale ? (int)CreatureIdEnum.HumanCitizenMale : (int)CreatureIdEnum.HumanCitizenFemale;
        var citizenPrefab = CreaturesList.Instance.GetCreature(creautereId);

        if (!citizenPrefab) {
            Debug.LogError($"Wanderer Prefab not fount at {CreaturesList.Instance}");
            return;
        }

        wanderer.Accept();

        var position = entranceTransform.position;
        var rotaton = entranceTransform.rotation;

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

        GameObject.Destroy(ridingBoat.gameObject);
        var citizen = CreatureFactory.CreateHuman(citizenPrefab, position, rotaton, data);

        OnWandererAccepted?.Invoke(citizen);
    }

    public static void RejectWanderer(Wanderer wanderer)
    {
        if (!wanderer) {
            Debug.LogError("Wanderer is not valid to reject");
            return;
        }

        wanderer.Reject();
        OnWandererRejected?.Invoke(wanderer);
    }
}