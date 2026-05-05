using UnityEngine;

public class EvictContextElement : ContextMenuElement
{
    protected override void OnShowed()
    {

    }

    protected override void OnButtonClicked()
    {

    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        Human human = target.GetComponent<Human>();
        if (!human) return false;

        if (human.CurrentStatusEnum != HumanStatusEnum.Citizen) return false;

        return true;
    }
}
