using UnityEngine;

public class EvictContextElement : ContextMenuElement
{
    protected override void OnShowed()
    {

    }

    protected override void OnButtonClicked()
    {

    }

    protected override bool ShouldShow()
    {
        Human human = SelectManager.Instance.GetSelectedHuman();
        if (!human) return false;

        if (human.currentStatusEnum != HumanStatusEnum.Citizen) return false;

        return true;
    }
}
