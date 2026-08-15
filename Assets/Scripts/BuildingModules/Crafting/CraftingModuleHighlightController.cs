using UnityEngine;

public class CraftingModuleHighlightController : MonoBehaviour
{
    [SerializeField] private CraftingModule craftingModule;

    private void OnEnable()
    {
        craftingModule.OnInited += HandleInited;

        craftingModule.OnWorkingStarted += HandleWorkingStarted;
        craftingModule.OnWorkingStarted += HandleWorkingStopped;

        craftingModule.OnItemCraftFinished += HandleItemCraftFinished;
        craftingModule.OnClicked += HandleClicked;

        craftingModule.OwnedBuilding.OnConstructionChanged += HandleConstructionChanged;
    }

    private void OnDisable()
    {
        craftingModule.OnInited -= HandleInited;

        craftingModule.OnWorkingStarted -= HandleWorkingStarted;
        craftingModule.OnWorkingStarted -= HandleWorkingStopped;

        craftingModule.OnItemCraftFinished -= HandleItemCraftFinished;
        craftingModule.OnClicked -= HandleClicked;

        craftingModule.OwnedBuilding.OnConstructionChanged -= HandleConstructionChanged;
    }

    private void Start()
    {
        UpdateHighlight();
    }

    private void UpdateHighlight()
    {
        if (!craftingModule) return;

        var ownedBuilding = craftingModule.OwnedBuilding;
        if (!ownedBuilding) return;

        var spawnedConstruction = ownedBuilding.SpawnedConstruction;
        if (!spawnedConstruction) return;

        var craft = craftingModule.SelectedCraftItem;
        var power = craft != null && craft.IsCraftingFinished() ? 1f : 0f;

        spawnedConstruction.SetFlickingPower(power);
    }

    private void HandleInited()
    {

    }

    private void HandleWorkingStarted()
    {
        UpdateHighlight();
    }

    private void HandleWorkingStopped()
    {
        UpdateHighlight();
    }

    private void HandleItemCraftFinished(CraftItemInstance craftItem)
    {
        UpdateHighlight();
    }

    private void HandleClicked()
    {
        UpdateHighlight();
    }

    private void HandleConstructionChanged(BuildingConstruction buildingConstruction)
    {
        UpdateHighlight();
    }
}