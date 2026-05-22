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

        var data = CitizenData.Create(wanderer);
        data.Id = creautereId;
        data.InstanceId = InstancesManager.Instance.GetNextInstanceId();
        data.BoatRider = new BoatRiderData();
        data.Position = new Vector3Data(position);
        data.Rotation = new Vector3Data(rotaton.eulerAngles);

        var citizen = CreatureFactory.CreateHuman(prefab, position, rotaton, data);

        GameObject.Destroy(wanderer.gameObject);
        GameObject.Destroy(wanderer.BoatRider.SelectedBoat.gameObject);

        OnWandererAccepted?.Invoke(citizen);
    }

    public static void RejectWanderer(Wanderer wanderer)
    {
        wanderer.Reject();
        OnWandererRejected?.Invoke(wanderer);
    }
}