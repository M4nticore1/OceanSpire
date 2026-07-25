using UnityEngine;

public class CraftingModuleHighlightController : MonoBehaviour
{
    [SerializeField] private CraftingModule craftingModule;

    private void OnEnable()
    {
        craftingModule.OnInited += OnInited;

        craftingModule.OnWorkingStarted += OnWorkingStarted;
        craftingModule.OnWorkingStarted += OnWorkingStopped;

        craftingModule.OnItemCraftEnded += OnItemCraftEnded;
        craftingModule.OnClicked += OnClicked;
    }

    private void OnDisable()
    {
        craftingModule.OnInited -= OnInited;

        craftingModule.OnWorkingStarted -= OnWorkingStarted;
        craftingModule.OnWorkingStarted -= OnWorkingStopped;

        craftingModule.OnItemCraftEnded -= OnItemCraftEnded;
        craftingModule.OnClicked -= OnClicked;
    }

    private void Start()
    {
        UpdateHighlight();
    }

    private void UpdateHighlight()
    {
        var craft = craftingModule.SelectedCraftItem;
        var power = craft != null && craft.IsCraftingFinished() ? 1f : 0f;

        craftingModule.OwnedBuilding.SpawnedConstruction.SetFlickingPower(power);
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

    private void OnItemCraftEnded(CraftItemInstance craftItem)
    {
        UpdateHighlight();
    }

    private void OnClicked()
    {
        UpdateHighlight();
    }
}