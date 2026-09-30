using UnityEngine;

public class BuildingCollectIndicatorController : BuildingIndicatorController
{
    private CraftingModule craftingModule => building?.GetModule<CraftingModule>() as CraftingModule;

    protected override void Subscribe()
    {
        base.Subscribe();

        if (craftingModule != null) {
            craftingModule.OnItemCraftStarted += HandleItemCraftStarted;
            craftingModule.OnItemCraftFinished += HandleItemCraftFinished;
            craftingModule.OnItemCollected += HandleItemCraftCollected;
        }
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        if (craftingModule != null) {
            craftingModule.OnItemCraftStarted -= HandleItemCraftStarted;
            craftingModule.OnItemCraftFinished -= HandleItemCraftFinished;
            craftingModule.OnItemCollected -= HandleItemCraftCollected;
        }
    }

    protected override void HandleClick()
    {
        craftingModule.TryCollectItem();
    }

    protected override bool ShouldShow()
    {
        var craftItem = GetCraftItem();
        if (craftItem == null) return false;

        return craftItem.IsCraftingFinished();
    }

    protected override Texture GetIndicatorTexture()
    {
        var baseTexture = base.GetIndicatorTexture();

        var craftItem = GetCraftItem();
        if (craftItem == null) return baseTexture;

        var definition = craftItem.Definition;
        if (definition == null) return baseTexture;

        var produceItem = definition.ProduceItem;
        if (produceItem == null) return baseTexture;

        var icon = produceItem.GetInformationIcon();
        if (icon == null) return baseTexture;

        return icon.texture;
    }

    private void HandleItemCraftStarted(CraftItemInstance craftItem)
    {
        RunUpdateShownEndOfFrame();
    }

    private void HandleItemCraftFinished(CraftItemInstance craftItem)
    {
        RunUpdateShownEndOfFrame();
    }

    private void HandleItemCraftCollected(CraftItemInstance craftItem)
    {
        RunUpdateShownEndOfFrame();
    }

    private CraftItemInstance GetCraftItem()
    {
        if (craftingModule == null) return null;

        return craftingModule.SelectedCraftItem;
    }
}