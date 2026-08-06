using UnityEngine;

public class CraftingModuleVFXController : VFXController
{
    [SerializeField] private ParticleSystem collectedItemVFX;
    [SerializeField] private Vector3 spawnVfxPositionOffset = new Vector3(0f, 2.5f, 0f);

    protected override void Subscribe()
    {
        base.Subscribe();

        CraftingModule.OnModuleItemCollected += HandleCraftItemCollected;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        CraftingModule.OnModuleItemCollected -= HandleCraftItemCollected;
    }

    private void HandleCraftItemCollected(CraftingModule module, CraftItemInstance craftItem)
    {
        Instantiate(collectedItemVFX, module.transform.position + spawnVfxPositionOffset, Quaternion.identity);
    }
}