using System;
using UnityEngine;

public class WandererAdmissionManager : MonoBehaviour
{
    [SerializeField] private CreaturesManager creaturesManager;
    [SerializeField] private CitizensManager citizensManager;
    [SerializeField] private CityStorage cityStorage;

    public event Action<Human> OnWandererAccepted;
    public event Action<Human> OnWandererRejected;

    public void AcceptWanderer(Wanderer wanderer)
    {
        if (!wanderer) {
            Debug.LogError($"[{nameof(WandererAdmissionManager)}] Wanderer not fount to access");
            return;
        }

        var boatRider = wanderer.BoatRider;
        if (!boatRider) {
            Debug.LogError($"[{nameof(WandererAdmissionManager)}] Boat Rider not fount at {wanderer}");
            return;
        }

        var ridingBoat = boatRider.RidingBoat;
        if (!ridingBoat) {
            Debug.LogError($"[{nameof(WandererAdmissionManager)}] Riding Boat not fount at {boatRider}");
            return;
        }

        var dockPoint = ridingBoat.DockPoint;
        if (!dockPoint) {
            Debug.LogError($"[{nameof(WandererAdmissionManager)}] Dock Point not fount at {ridingBoat}");
            return;
        }

        var entranceTransform = dockPoint.EntraceTransform;
        if (!entranceTransform) {
            Debug.LogError($"[{nameof(WandererAdmissionManager)}] Entrance Transform not fount at {dockPoint}");
            return;
        }

        if (cityStorage.Inventory.GetItem(ItemID.Population).Amount <= creaturesManager.Citizens.Count) {
            Debug.LogError($"[{nameof(WandererAdmissionManager)}] The number of citizens exceeds the maximum number of the population");
            return;
        }

        var creautereId = wanderer.GenderComponent.IsMale ? CreatureIdEnum.HumanCitizenMale : CreatureIdEnum.HumanCitizenFemale;
        var citizenPrefab = CreaturesList.Instance.GetCreature(creautereId);

        if (!citizenPrefab) {
            Debug.LogError($"[{nameof(WandererAdmissionManager)}] Wanderer Prefab not fount at {CreaturesList.Instance}");
            return;
        }

        wanderer.Accept();

        var position = entranceTransform.position;
        var rotaton = entranceTransform.rotation;

        var data = new CitizenData()
        {
            Id = creautereId,
            Health = HealthData.Create(wanderer.HealthComponent),
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

    public void RejectWanderer(Wanderer wanderer)
    {
        if (!wanderer) {
            Debug.LogError($"[{nameof(WandererAdmissionManager)}] Wanderer is not valid to reject");
            return;
        }

        wanderer.Reject();
        OnWandererRejected?.Invoke(wanderer);
    }

    public bool CanAcceptWanderer(Wanderer wanderer)
    {
        if (!wanderer) return false;

        var boatRider = wanderer.BoatRider;
        if (!boatRider) return false;

        var ridingBoat = boatRider.RidingBoat;
        if (!ridingBoat) return false;

        var dockPoint = ridingBoat.DockPoint;
        if (!dockPoint) return false;

        var entranceTransform = dockPoint.EntraceTransform;
        if (!entranceTransform) return false;

        if (citizensManager.GetAvaliableCitizensCount() >= cityStorage.Inventory.GetItem(ItemID.Population).Stack.Amount) return false;

        return true;
    }
}