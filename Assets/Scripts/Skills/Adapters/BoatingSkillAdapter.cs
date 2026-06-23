using UnityEngine;

public class BoatingSkillAdapter : SkillAdapter
{
    [SerializeField] private Boat boat;

    protected override bool TrySubscribe()
    {
        if (!base.TrySubscribe()) return false;

        boat.OnRiderAdded += OnBoatRiderAdded;
        boat.OnRiderRemoved += OnBoatRiderRemoved;

        return true;
    }

    protected override bool TryUnsubscribe()
    {
        if (!base.TryUnsubscribe()) return false;

        boat.OnRiderAdded -= OnBoatRiderAdded;
        boat.OnRiderRemoved -= OnBoatRiderRemoved;

        return true;
    }

    protected override void AddBonus(float bonus)
    {
        var boatSpeed = boat.Definition.BoatSpeed;
        var skillBonus = bonus;
        var bonusSpeed = boatSpeed * (1 + skillBonus);

        boat.Movement.NavAgent.speed = bonusSpeed;
    }

    protected override void RemoveBonus(float bonus)
    {
        var boatSpeed = boat.Definition.BoatSpeed;
        boat.Movement.NavAgent.speed = boatSpeed;
    }

    private void OnBoatRiderAdded(BoatRider boatRider)
    {
        var skillsComponent = boatRider.GetComponent<SkillsComponent>();
        AddSkillsComponent(skillsComponent);
        AddBonus(GetBonus(skillsComponent));
    }

    private void OnBoatRiderRemoved(BoatRider boatRider)
    {
        var skillsComponent = boatRider.GetComponent<SkillsComponent>();
        RemoveSkillsComponent(skillsComponent);
        RemoveBonus(GetBonus(skillsComponent));
    }
}