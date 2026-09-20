using UnityEngine;

public class BuildingCraftItemsIndicatorController : BuildingIndicatorController
{
    private CityStorage cityStorage => CityStorage.Instance;
    private CraftingControlMenu craftsControlMenu => CraftingControlMenu.Instance;

    private CraftingModule craftingModule => building?.GetModule<CraftingModule>() as CraftingModule;

    protected override void Start()
    {
        base.Start();

        if (cityStorage == null) {
            Debug.LogError($"[{nameof(BuildingCraftItemsIndicatorController)}] City Storage is not valid!");
        }
        if (craftsControlMenu == null) {
            Debug.LogError($"[{nameof(BuildingCraftItemsIndicatorController)}] Crafts Control Menu is not valid!");
        }
    }

    protected override void Subscribe()
    {
        base.Subscribe();

        if (craftingModule != null) {
            craftingModule.OnItemCraftStarted += HandleItemCraftStarted;
            craftingModule.OnCraftItemChanged += HandleCraftItemChanged;
        }

        if (cityStorage != null && cityStorage.Inventory != null) {
            cityStorage.Inventory.OnItemAmountChanged += HandleItemAmountChanged;
        }
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        if (craftingModule != null) {
            craftingModule.OnItemCraftStarted -= HandleItemCraftStarted;
            craftingModule.OnCraftItemChanged -= HandleCraftItemChanged;
        }

        if (cityStorage != null && cityStorage.Inventory != null) {
            cityStorage.Inventory.OnItemAmountChanged -= HandleItemAmountChanged;
        }
    }

    protected override void HandleClick()
    {
        if (craftsControlMenu == null) return;

        craftsControlMenu.Show(building);
    }

    protected override bool ShouldShow()
    {
        if (constructionComponent.IsUnderConstruction) return false;
        if (craftingModule == null) return false;

        var craftItem = craftingModule.SelectedCraftItem;
        if (craftItem == null) return false;

        if (craftItem.IsCraftingFinished()) return false;

        return !EnoughItems();
    }

    protected override Texture GetIndicatorTexture()
    {
        var baseTexture = base.GetIndicatorTexture();

        var shortageConsumeItem = GetFirstShortageConsumeItem();
        if (shortageConsumeItem == null) return baseTexture;

        var definition = shortageConsumeItem.Definition;
        if (definition == null) return baseTexture;

        var texture = definition.ItemTexture;
        if (texture == null) return baseTexture;

        return texture;
    }

    private void HandleItemCraftStarted(CraftItemInstance craftItem)
    {
        RunUpdateShownEndOfFrame();
    }

    private void HandleCraftItemChanged(CraftItemInstance craftItem)
    {
        RunUpdateShownEndOfFrame();
    }

    private void HandleItemAmountChanged(ItemInstance item)
    {
        RunUpdateShownEndOfFrame();
    }

    private bool EnoughItems()
    {
        return GetFirstShortageConsumeItem() == null;
    }

    private ItemInstance GetFirstShortageConsumeItem()
    {
        if (cityStorage == null) return null;
        if (craftingModule == null) return null;

        var craftItem = craftingModule.SelectedCraftItem;
        if (craftItem == null) return null;

        var itemDefinition = craftItem.Definition;
        if (itemDefinition == null) return null;

        var consumeItems = itemDefinition.ConsumeResources;
        if (consumeItems == null) return null;

        foreach (var consumeItem in consumeItems) {
            if (consumeItem == null) continue;
            if (consumeItem.Definition == null) continue;

            var cityItem = cityStorage.Inventory.GetInventoryItem(consumeItem.Definition.ItemId);
            if (cityItem == null) return consumeItem;
            if (cityItem.Amount < consumeItem.Amount) return consumeItem;
        }

        return null;
    }
}