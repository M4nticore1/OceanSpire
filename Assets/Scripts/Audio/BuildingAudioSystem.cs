using UnityEngine;
using UnityEngine.Audio;

public class BuildingAudioSystem : AudioSystem
{
    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private AudioClip[] buildingStartedClips;
    [SerializeField] private AudioClip[] buildingFinishedClips;
    [SerializeField] private AudioClip[] buildingDemolishedClips;

    protected override void Subscribe()
    {
        Building.OnBuildingConstructionStarted += OnBuildingConstructionStarted;
        Building.OnBuildingConstructionFinished += OnBuildingUpgradeCompleted;
        Building.OnBuildingDemolished += OnBuildingDemolished;
    }

    protected override void Unsubscribe()
    {
        Building.OnBuildingConstructionStarted -= OnBuildingConstructionStarted;
        Building.OnBuildingConstructionFinished -= OnBuildingUpgradeCompleted;
        Building.OnBuildingDemolished -= OnBuildingDemolished;
    }

    private void OnBuildingConstructionStarted(Building building)
    {
        if (!ShouldPlay()) return;

        AudioUtils.PlaySFX(buildingStartedClips, mixerGroup);
    }

    private void OnBuildingUpgradeCompleted(Building building)
    {
        AudioUtils.PlaySFX(buildingFinishedClips, mixerGroup);
    }

    private void OnBuildingDemolished(Building building)
    {
        if (!ShouldPlay()) return;

        AudioUtils.PlaySFX(buildingDemolishedClips, mixerGroup);
    }

    private bool ShouldPlay()
    {
        //if (!buildingsLoader.IsLoaded) return false;

        return true;
    }
}