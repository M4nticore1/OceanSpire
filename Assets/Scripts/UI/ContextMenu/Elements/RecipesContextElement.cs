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

    protected override bool ShouldShow()
    {
        Building building = SelectManager.Instance.GetSelectedBuilding();
        if (!building) return false;

        if (!building.GetComponent<ProductionModule>()) return false;
        if (building.ConstructionComponent.IsUnderConstruction) return false;

        return true;
    }
}