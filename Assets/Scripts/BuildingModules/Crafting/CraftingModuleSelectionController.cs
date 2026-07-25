using UnityEngine;

public class CraftingModuleSelectionController : MonoBehaviour
{
    [SerializeField] private CraftingModule craftingModule;

    private void OnEnable()
    {
        craftingModule.OnItemCollected += OnItemCollected;
    }

    private void OnDisable()
    {
        craftingModule.OnItemCollected -= OnItemCollected;
    }

    private void OnItemCollected(CraftItemInstance craftItem)
    {
        Debug.Log("Collected");
        craftingModule.OwnedBuilding.SelectComponent.Deselect();
    }
}