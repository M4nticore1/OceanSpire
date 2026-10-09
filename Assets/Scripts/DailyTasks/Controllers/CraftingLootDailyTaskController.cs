using UnityEngine;

public class CraftingLootDailyTaskController : DailyTaskController
{
    protected override bool Subscribe()
    {
        if (!base.Subscribe()) return false;

        CraftingModule.OnModuleItemCollected += HandleModuleItemCollected;

        return true;
    }

    protected override bool Unsubscribe()
    {
        if (!base.Unsubscribe()) return false;

        CraftingModule.OnModuleItemCollected -= HandleModuleItemCollected;

        return true;
    }

    private void HandleModuleItemCollected(CraftingModule craftingModule, CraftItemInstance craftItemInstance)
    {
        if (craftItemInstance == null) return;

        var definition = craftItemInstance.Definition;
        if (definition == null) return;

        var productionItem = definition.ProduceItem;
        if (productionItem == null) return;

        AddTaskProgress(productionItem.Amount);
    }
}