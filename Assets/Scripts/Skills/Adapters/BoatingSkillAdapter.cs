using UnityEngine;

public class BoatingSkillAdapter : SkillAdapter
{
    protected override bool TrySubscribe()
    {
        if (!base.TrySubscribe()) return false;

        BoatRider.OnRiderEnteredBoat += OnRiderBoatAdded;
        BoatRider.OnRiderExitedBoat += OnRiderBoatRemoved;

        return true;
    }

    protected override bool TryUnsubscribe()
    {
        if (!base.TryUnsubscribe()) return false;

        BoatRider.OnRiderEnteredBoat -= OnRiderBoatAdded;
        BoatRider.OnRiderExitedBoat -= OnRiderBoatRemoved;

        return true;
    }

    protected override void OnSkillLevelChanged(SkillsComponent skillsComponent)
    {
        
    }

    private void AddBonus(Boat boat, float bonus)
    {
        var boatSpeed = boat.Definition.BoatSpeed;
        var skillBonus = bonus;
        var bonusSpeed = boatSpeed * (1 + skillBonus);

        boat.Movement.NavAgent.speed = bonusSpeed;
    }

    private void RemoveBonus(Boat boat, float bonus)
    {
        var boatSpeed = boat.Definition.BoatSpeed;
        boat.Movement.NavAgent.speed = boatSpeed;
    }

    private void OnRiderBoatAdded(BoatRider rider, Boat boat)
    {
        var citizen = rider.GetComponent<Citizen>();
        if (!citizen) return;

        var skillsComponent = citizen.SkillsComponent;

        AddSkillsComponent(skillsComponent);
        AddBonus(boat, GetBonus(skillsComponent));
    }

    private void OnRiderBoatRemoved(BoatRider rider, Boat boat)
    {
        var citizen = rider.GetComponent<Citizen>();
        if (!citizen) return;

        var skillsComponent = citizen.SkillsComponent;

        RemoveSkillsComponent(skillsComponent);
        RemoveBonus(boat, GetBonus(skillsComponent));
    }
}