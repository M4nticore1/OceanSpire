using UnityEngine;

public class CraftingModuleHighlightController : MonoBehaviour
{
    [SerializeField] private CraftingModule craftingModule;

    private void OnEnable()
    {
        craftingModule.OnItemCraftEnded += OnItemCraftEnded;
        craftingModule.OnItemCollected += OnItemCollected;
    }

    private void OnDisable()
    {
        craftingModule.OnItemCraftEnded -= OnItemCraftEnded;
        craftingModule.OnItemCollected -= OnItemCollected;
    }

    private void OnItemCraftEnded(CraftItemInstance craftItem)
    {
        craftingModule.OwnedBuilding.SpawnedConstruction.SetFlickingPower(1f);
    }

    private void OnItemCollected(CraftItemInstance craftItem)
    {
        craftingModule.OwnedBuilding.SpawnedConstruction.SetFlickingPower(0f);
    }
}