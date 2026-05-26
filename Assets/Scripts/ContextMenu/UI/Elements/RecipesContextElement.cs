using UnityEngine;

public class RecipesContextElement : ContextElement
{
    [SerializeField] private CraftingControlMenu recipesMenu;

    protected override void OnShowed()
    {

    }

    protected override void OnButtonClicked()
    {
        recipesMenu.Open();
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        var building = target.GetComponent<Building>();
        if (!building) return false;

        if (!building.GetComponent<CraftingModule>()) return false;
        if (building.ConstructionComponent.IsUnderConstruction) return false;

        return true;
    }
}