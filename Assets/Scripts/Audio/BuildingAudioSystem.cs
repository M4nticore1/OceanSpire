using UnityEngine;
using UnityEngine.Audio;

public class BuildingAudioSystem : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private AudioClip[] buildingStartedClips;
    [SerializeField] private AudioClip[] buildingFinishedClips;
    [SerializeField] private AudioClip[] buildingDemolishedClips;

    private void OnEnable()
    {
        Building.OnBuildingConstructionStarted += OnBuildingConstructionStarted;
        Building.OnBuildingConstructionCompleted += OnBuildingUpgradeCompleted;
        Building.OnBuildingDemolished += OnBuildingDemolished;
    }

    private void OnDisable()
    {
        Building.OnBuildingConstructionStarted -= OnBuildingConstructionStarted;
        Building.OnBuildingConstructionCompleted -= OnBuildingUpgradeCompleted;
        Building.OnBuildingDemolished -= OnBuildingDemolished;
    }

    private void OnBuildingConstructionStarted(Building building)
    {
        if (!ShouldPlay()) return;

        AudioUtils.PlaySFX(buildingStartedClips, mixerGroup);
    }

    private void OnBuildingUpgradeCompleted(Building building)
    {
        if (!ShouldPlay()) return;

        AudioUtils.PlaySFX(buildingFinishedClips, mixerGroup);
    }

    private void OnBuildingDemolished(Building building)
    {
        if (!ShouldPlay()) return;

        AudioUtils.PlaySFX(buildingDemolishedClips, mixerGroup);
    }

    private bool ShouldPlay()
    {
        return BuildingsLoader.Instance.IsLoaded;
    }
}