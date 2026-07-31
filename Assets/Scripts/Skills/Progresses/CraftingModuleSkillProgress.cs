using UnityEngine;

public class CraftingModuleSkillProgress : SkillProgress
{
    [SerializeField] private float gainXpFrequency = 10f;

    private float currentAddXpTime;

    private void Update()
    {
        currentAddXpTime += Time.deltaTime;
        if (currentAddXpTime < gainXpFrequency) return;

        foreach (var component in SkillAdapter.SkillComponents) {
            if (!ShouldAddXp(component)) continue;

            float xp = XpGain * gainXpFrequency;
            AddXp(xp);
        }

        currentAddXpTime = 0f;
    }

    protected override bool TrySubscribe()
    {
        if (!base.TrySubscribe()) return false;

        CraftingModule.OnModuleItemCraftEnded += OnItemCrafted;
        return true;
    }

    protected override bool TryUnsubscribe()
    {
        if (!base.TryUnsubscribe()) return false;

        CraftingModule.OnModuleItemCraftEnded -= OnItemCrafted;
        return true;
    }

    private void OnItemCrafted(CraftingModule module, CraftItemInstance craftItem)
    {
        if (module.OwnedBuilding.SkillId != SkillAdapter.SkillId) return;

        AddXp(XpGain);
    }

    private bool ShouldAddXp(SkillsComponent skillsComponent)
    {
        var interactComponent = skillsComponent.GetComponent<CreatureInteractComponent>();
        if (!interactComponent) return false;

        var interactBuilding = interactComponent.InteractBuilding;
        if (!interactBuilding) return false;

        var craftBuilding = interactBuilding.GetComponent<CraftingModule>();
        if (!craftBuilding) return false;

        if (!craftBuilding.IsWorking) return false;
        if (craftBuilding.OwnedBuilding.SkillId != SkillAdapter.SkillId) return false;

        return true;
    }
}
// maksimka huesos ebani i on ebet sobak vsegda gotov trahnut srazu neskolkih kotov kakaska