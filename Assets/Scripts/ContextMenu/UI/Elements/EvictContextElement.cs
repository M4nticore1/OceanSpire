using UnityEngine;

public class EvictContextElement : ContextElement
{
    protected override void OnShowed()
    {

    }

    protected override void OnButtonClicked()
    {

    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        var citizen = target.GetComponent<Citizen>();
        if (!citizen) return false;

        return true;
    }
}
