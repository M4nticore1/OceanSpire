using UnityEngine;

public class RecipesContextElement : ContextElement
{
    [Header("Crafting")]
    [SerializeField] private CraftingControlMenu recipesMenu;

    private Building building;

    protected override void OnButtonClicked()
    {
        recipesMenu.Show(building);
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        building = target.GetComponent<Building>();
        if (building == null) return false;

        if (building.GetComponent<CraftingModule>() == null) return false;
        if (building.ConstructionComponent.GetUnderConstruction()) return false;

        return true;
    }
}