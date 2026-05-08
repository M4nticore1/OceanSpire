using UnityEngine;

public class CitizenMenu : ControlMenu
{
    [SerializeField] private SkillsPanel skillsPanel;
    [SerializeField] private EquipmentPanel equipmentPanel;

    protected override void OnOpen()
    {
        Human citizen = SelectManager.Instance.GetSelectedHuman();
        if (!citizen) return;

        skillsPanel.SetSkills(citizen.SkillsComponent);
        equipmentPanel.SetWeapon(citizen.WeaponEquipment);
    }

    protected override void OnClose()
    {

    }

    protected override void UpdateMenu()
    {

    }
}