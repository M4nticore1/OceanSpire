using UnityEngine;

public class CitizenMenuContextElement : ContextElement
{
    [SerializeField] private CitizenMenu citizenMenu;

    protected override void OnShowed()
    {
        
    }

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