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

            AddXp(gainXpFrequency * XpGain);
        }

        currentAddXpTime = 0f;
    }

    private bool ShouldAddXp(SkillsComponent skillsComponent)
    {
        var interactComponent = skillsComponent.GetComponent<CreatureInteractComponent>();
        if (interactComponent == null) return false;

        var interactBuilding = interactComponent.InteractBuilding;
        if (interactBuilding == null) return false;

        var craftBuilding = interactBuilding.GetComponent<CraftingModule>();
        if (craftBuilding == null) return false;

        if (!craftBuilding.IsWorking) return false;
        if (craftBuilding.OwnedBuilding.SkillId != SkillAdapter.SkillId) return false;

        return true;
    }
}
// maksimka huesos ebani i on ebet sobak vsegda gotov trahnut srazu neskolkih kotov kakaska