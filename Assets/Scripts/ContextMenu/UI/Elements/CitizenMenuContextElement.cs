using UnityEngine;

public class CitizenMenuContextElement : ContextElement
{
    [Header("Citizen")]
    [SerializeField] private CitizenMenu citizenMenu;

    private Citizen citizen;

    protected override void OnButtonClicked()
    {
        citizenMenu.Show(citizen);
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        citizen = target.GetComponent<Citizen>();
        if (citizen == null) return false;

        return true;
    }
}