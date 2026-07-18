using UnityEngine;

public class EvictContextElement : ContextElement
{
    [Header("Evict")]
    [SerializeField] private EvictMenu evictMenu;

    protected override void OnButtonClicked()
    {
        evictMenu.Show();
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        var citizen = target.GetComponent<Citizen>();
        if (!citizen) return false;

        return true;
    }
}