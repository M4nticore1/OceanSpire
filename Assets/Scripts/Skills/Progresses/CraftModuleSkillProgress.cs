using UnityEngine;

public class CraftModuleSkillProgress : SkillProgress
{
    protected override bool TrySubscribe()
    {
        if (!base.TrySubscribe()) return false;

        CraftingModule.OnModuleItemCrafted += OnItemCrafted;
        return true;
    }

    protected override bool TryUnsubscribe()
    {
        if (!base.TryUnsubscribe()) return false;

        CraftingModule.OnModuleItemCrafted -= OnItemCrafted;
        return true;
    }

    private void OnItemCrafted(BuildingModule module, CraftItemInstance craftItem)
    {
        AddXp();
    }
}
// maksimka huesos ebani i on ebet sobak vsegda gotov trahnut srazu neskolkih kotov kakaska