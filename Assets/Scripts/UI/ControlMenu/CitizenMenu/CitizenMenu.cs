using UnityEngine;

public class CitizenMenu : ControlMenu
{
    [Header("Citizen Menu")]
    [SerializeField] private SkillsPanel skillsPanel;
    [SerializeField] private EquipmentPanel equipmentPanel;

    private Citizen citizen;

    protected override void OnShow()
    {
        base.OnShow();

        skillsPanel.SetSkills(citizen.SkillsComponent);
    }

    protected override void UpdateMenu()
    {

    }

    protected override ILocalizable GetTargetNameText()
    {
        return citizen;
    }

    protected override ILocalizable GetTargetDescriptionText()
    {
        return null;
    }

    public void Show(Citizen citizen)
    {
        if (citizen == null) {
            Debug.LogError($"[{nameof(CitizenMenu)}] Citizen is not valid!");
            return;
        }

        this.citizen = citizen;
        Show();
    }
}