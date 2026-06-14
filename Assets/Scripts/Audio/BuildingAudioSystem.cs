using UnityEngine;
using UnityEngine.Audio;

public class BuildingAudioSystem : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup mixerGoup;
    [SerializeField] private AudioClip[] buildingStartedClips;
    [SerializeField] private AudioClip[] buildingFinishedClips;
    [SerializeField] private AudioClip[] buildingDemolishedClips;

    private void OnEnable()
    {
        Building.OnBuildingUpgradeStarted += OnBuildingConstructionStarted;
        Building.OnBuildingUpgradeCompleted += OnBuildingUpgradeCompleted;
        Building.OnBuildingDemolished += OnBuildingDemolished;
    }

    private void OnDisable()
    {
        Building.OnBuildingUpgradeStarted -= OnBuildingConstructionStarted;
        Building.OnBuildingUpgradeCompleted -= OnBuildingUpgradeCompleted;
        Building.OnBuildingDemolished -= OnBuildingDemolished;
    }

    private void OnBuildingConstructionStarted(Building building)
    {
        if (!ShouldPlay()) return;

        AudioUtils.PlaySFX(buildingStartedClips, mixerGoup);
    }

    private void OnBuildingUpgradeCompleted(Building building)
    {
        if (!ShouldPlay()) return;

        AudioUtils.PlaySFX(buildingFinishedClips, mixerGoup);
    }

    private void OnBuildingDemolished(Building building)
    {
        if (!ShouldPlay()) return;

        AudioUtils.PlaySFX(buildingDemolishedClips, mixerGoup);
    }

    private bool ShouldPlay()
    {
        return BuildingsLoader.Instance.IsLoaded;
    }
}