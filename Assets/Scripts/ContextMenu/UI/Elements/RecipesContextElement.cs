using UnityEngine;

public class RecipesContextElement : ContextMenuElement
{
    [SerializeField] private ProductionControlMenu recipesMenu;

    protected override void OnShowed()
    {

    }

    protected override void OnButtonClicked()
    {
        recipesMenu.Open();
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        Building building = target.GetComponent<Building>();
        if (!building) return false;

        if (!building.GetComponent<ProductionModule>()) return false;
        if (building.ConstructionComponent.IsUnderConstruction) return false;

        return true;
    }
}