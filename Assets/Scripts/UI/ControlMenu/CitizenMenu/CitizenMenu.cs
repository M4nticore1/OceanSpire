using UnityEngine;

public class CitizenMenu : ControlMenu
{
    [SerializeField] private SkillsPanel skillsPanel;
    [SerializeField] private EquipmentPanel equipmentPanel;

    protected override void OnOpen()
    {
        var selectedCitizen = SelectManager.Instance.GetSelectedHuman();
        if (!selectedCitizen) return;

        skillsPanel.SetSkills(selectedCitizen.SkillsComponent);
    }

    protected override void OnClose()
    {

    }

    protected override void UpdateMenu()
    {

    }
}