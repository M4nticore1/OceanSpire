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
        Human human = target.GetComponent<Human>();
        if (!human) return false;

        if (human.CurrentStatusEnum != HumanStatusEnum.Citizen) return false;

        return true;
    }
}