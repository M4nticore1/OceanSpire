using UnityEngine;

public class CraftingModuleHighlightController : MonoBehaviour
{
    [SerializeField] private CraftingModule craftingModule;

    private void OnEnable()
    {
        craftingModule.OnInited += OnInited;

        craftingModule.OnWorkingStarted += OnWorkingStarted;
        craftingModule.OnWorkingStarted += OnWorkingStopped;

        craftingModule.OnItemCraftFinished += OnItemCraftFinished;
        craftingModule.OnClicked += OnClicked;
    }

    private void OnDisable()
    {
        craftingModule.OnInited -= OnInited;

        craftingModule.OnWorkingStarted -= OnWorkingStarted;
        craftingModule.OnWorkingStarted -= OnWorkingStopped;

        craftingModule.OnItemCraftFinished -= OnItemCraftFinished;
        craftingModule.OnClicked -= OnClicked;
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

    private void OnInited()
    {

    }

    private void OnWorkingStarted()
    {
        UpdateHighlight();
    }

    private void OnWorkingStopped()
    {
        UpdateHighlight();
    }

    private void OnItemCraftFinished(CraftItemInstance craftItem)
    {
        UpdateHighlight();
    }

    private void OnClicked()
    {
        UpdateHighlight();
    }
}