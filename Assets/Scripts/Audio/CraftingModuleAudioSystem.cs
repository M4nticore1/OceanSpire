using UnityEngine;
using UnityEngine.Audio;

public class CraftingModuleAudioSystem : AudioSystem
{
    [SerializeField] private BuildingsLoader buildingsLoader;

    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private AudioClip craftingEndedSFX;
    [SerializeField] private AudioClip itemCollectedSFX;

    [SerializeField] private float minDistance = 50f;
    [SerializeField] private float maxDistance = 200f;

    protected override void Subscribe()
    {
        base.Subscribe();

        CraftingModule.OnModuleItemCraftEnded += OnCraftingEnded;
        CraftingModule.OnModuleItemCollected += OnItemCollected;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        CraftingModule.OnModuleItemCraftEnded -= OnCraftingEnded;
        CraftingModule.OnModuleItemCollected -= OnItemCollected;
    }

    private void OnCraftingEnded(CraftingModule craftingModule, CraftItemInstance craftItem)
    {
        if (!buildingsLoader.IsLoaded) return;

        AudioUtils.PlaySFXAtPosition(craftingEndedSFX, craftingModule.transform.position, minDistance, maxDistance, mixerGroup);
    }

    private void OnItemCollected(CraftingModule craftingModule, CraftItemInstance craftItem)
    {
        AudioUtils.PlaySFXAtPosition(itemCollectedSFX, craftingModule.transform.position, minDistance, maxDistance, mixerGroup);
    }
}