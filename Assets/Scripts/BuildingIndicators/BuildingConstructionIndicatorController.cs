using UnityEngine;

public class BuildingConstructionIndicatorController : BuildingIndicatorController
{
    private CompleteConstructionMenu completeConstructionMenu => CompleteConstructionMenu.Instance;

    protected override void Start()
    {
        base.Start();

        if (completeConstructionMenu == null) {
            Debug.LogError($"[{nameof(BuildingConstructionIndicatorController)}] Complete Construction Menu is not valid!");
        }
    }

    protected override void HandleClick()
    {
        if (completeConstructionMenu == null) return;

        completeConstructionMenu.Show(building);
    }

    protected override bool ShouldShow()
    {
        if (constructionComponent == null) return false;

        return constructionComponent.IsUnderConstruction;
    }
}