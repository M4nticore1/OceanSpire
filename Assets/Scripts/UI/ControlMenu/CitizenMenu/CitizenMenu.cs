using UnityEngine;

public class CitizenMenu : ControlMenu
{
    [SerializeField] private SkillsPanel skillsPanel;
    [SerializeField] private EquipmentPanel equipmentPanel;

    protected override void OnOpen()
    {
        Human selectedCitizen = SelectManager.Instance.GetSelectedHuman();
        if (!selectedCitizen) return;

        skillsPanel.SetSkills(selectedCitizen.SkillsComponent);
        equipmentPanel.SetWeapon(selectedCitizen.WeaponComponent);
    }

    protected override void OnClose()
    {

    }

    protected override void UpdateMenu()
    {

    }
}