using UnityEngine;

public class BoatingSkillAdapter : SkillAdapter
{
    protected override bool TrySubscribe()
    {
        if (!base.TrySubscribe()) return false;

        BoatRider.OnRiderEnteredBoat += HandleRiderBoatAdded;

        return true;
    }

    protected override bool TryUnsubscribe()
    {
        if (!base.TryUnsubscribe()) return false;

        BoatRider.OnRiderEnteredBoat -= HandleRiderBoatAdded;

        return true;
    }

    protected override ILevelBonusable GetBonusTarget(SkillsComponent skillsComponent)
    {
        if (skillsComponent == null) return null;

        var boatRider = skillsComponent.GetComponent<BoatRider>();
        if (boatRider == null) return null;

        return boatRider.RidingBoat;
    }

    private void SetBonus(Boat boat, float bonus)
    {
        if (boat == null) return;

        boat.SetLevelBonus(bonus);
    }

    private void HandleRiderBoatAdded(BoatRider rider, Boat boat)
    {
        if (rider == null) return;

        var citizen = rider.GetComponent<Citizen>();
        if (citizen == null) return;

        var skillsComponent = citizen.SkillsComponent;
        if (skillsComponent == null) return;

        AddSkillsComponent(skillsComponent);
        SetBonus(boat, GetBonus(skillsComponent));
    }
}