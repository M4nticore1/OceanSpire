using UnityEngine;

public class CitizenMenuContextElement : ContextElement
{
    [Header("Citizen")]
    [SerializeField] private CitizenMenu citizenMenu;

    protected override void OnButtonClicked()
    {
        citizenMenu.Open();
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        var citizen = target.GetComponent<Citizen>();
        if (!citizen) return false;

        return true;
    }
}