using UnityEngine;

public class EvictContextElement : ContextElement
{
    [SerializeField] private EvictMenu evictMenu;

    protected override void OnShowed()
    {

    }

    protected override void OnButtonClicked()
    {
        evictMenu.Open();
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        var citizen = target.GetComponent<Citizen>();
        if (!citizen) return false;

        return true;
    }
}