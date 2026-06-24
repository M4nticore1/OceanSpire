using UnityEngine;

public class CombatSkillAdapter : SkillAdapter
{
    protected override bool TrySubscribe()
    {
        if (!base.TrySubscribe()) return false;



        return true;
    }

    protected override bool TryUnsubscribe()
    {
        if (!base.TryUnsubscribe()) return false;



        return true;
    }

    protected override void OnSkillLevelChanged(SkillsComponent skillsComponent)
    {

    }

    private void AddBonus(EquipmentComponent equipmentComponent, float bonus)
    {
        //weaponEquipmentComponent.AddPowerBonus(bonus);
    }

    private void RemoveBonus(EquipmentComponent equipmentComponent, float bonus)
    {
        //weaponEquipmentComponent.RemovePowerBonus(bonus);
    }
}