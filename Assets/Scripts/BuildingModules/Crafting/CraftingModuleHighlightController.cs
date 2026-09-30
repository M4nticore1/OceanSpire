using System.Collections;
using UnityEngine;

public class CraftingModuleHighlightController : MonoBehaviour
{
    [SerializeField] private CraftingModule craftingModule;

    private Coroutine updateHighlightEndOfFrameCoroutine;

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
        RunUpdateHighlightEndOfFrame();
    }

    private void RunUpdateHighlightEndOfFrame()
    {
        if (updateHighlightEndOfFrameCoroutine == null) {
            updateHighlightEndOfFrameCoroutine = StartCoroutine(UpdateHighlightEndOfFrame());
        }
    }

    private void UpdateHighlight()
    {
        if (craftingModule == null) {
            Debug.LogError($"[{nameof(CraftingModuleHighlightController)}] Crafting Module is not valid!");
            return;
        }

        var ownedBuilding = craftingModule.OwnedBuilding;
        if (ownedBuilding == null) {
            Debug.LogError($"[{nameof(CraftingModuleHighlightController)}] Owned is not valid at {craftingModule}!");
            return;
        }

        var spawnedConstruction = ownedBuilding.SpawnedConstruction;
        if (spawnedConstruction == null) {
            Debug.LogError($"[{nameof(CraftingModuleHighlightController)}] Construction is not valid at {ownedBuilding}!");
            return;
        }

        var craft = craftingModule.SelectedCraftItem;
        var power = craft != null && craft.IsCraftingFinished() ? 1f : 0f;

        spawnedConstruction.SetFlickingPower(power);
    }

    private void HandleInited()
    {
        RunUpdateHighlightEndOfFrame();
    }

    private void HandleWorkingStarted()
    {
        RunUpdateHighlightEndOfFrame();
    }

    private void HandleWorkingStopped()
    {
        RunUpdateHighlightEndOfFrame();
    }

    private void HandleItemCraftFinished(CraftItemInstance craftItem)
    {
        RunUpdateHighlightEndOfFrame();
    }

    private void HandleClicked()
    {
        RunUpdateHighlightEndOfFrame();
    }

    private void HandleConstructionChanged(BuildingConstruction buildingConstruction)
    {
        RunUpdateHighlightEndOfFrame();
    }

    private IEnumerator UpdateHighlightEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        updateHighlightEndOfFrameCoroutine = null;
        UpdateHighlight();
    }
}