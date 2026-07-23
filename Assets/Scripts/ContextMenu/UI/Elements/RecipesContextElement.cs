using UnityEngine;

public class RecipesContextElement : ContextElement
{
    [Header("Crafting")]
    [SerializeField] private CraftingControlMenu recipesMenu;

    protected override void OnButtonClicked()
    {
        Debug.Log("Clicked");
        recipesMenu.Show();
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        var building = target.GetComponent<Building>();
        if (!building) return false;

        if (!building.GetComponent<CraftingModule>()) return false;
        if (building.ConstructionComponent.GetUnderConstruction()) return false;

        return true;
    }
}