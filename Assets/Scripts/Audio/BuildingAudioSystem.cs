using UnityEngine;

public class BuildingAudioSystem : MonoBehaviour
{
    [SerializeField] private AudioClip[] buildingStartedClips;
    [SerializeField] private AudioClip[] buildingFinishedClips;
    [SerializeField] private AudioClip[] buildingDemolishedClips;

    private void OnEnable()
    {
        Building.onBuildingConstructionStarted += OnBuildingConstructionStarted;
        Building.onBuildingConstructionFinished += OnBuildingConstructionFinished;
        Building.onBuildingDemolished += OnBuildingDemolished;
    }

    private void OnDisable()
    {
        Building.onBuildingConstructionStarted -= OnBuildingConstructionStarted;
        Building.onBuildingConstructionFinished -= OnBuildingConstructionFinished;
        Building.onBuildingDemolished -= OnBuildingDemolished;
    }

    private void OnBuildingConstructionStarted(Building building)
    {
        if (!ShouldPlay()) return;

        AudioUtils.PlaySFX(buildingStartedClips);
    }

    private void OnBuildingConstructionFinished(Building building)
    {
        if (!ShouldPlay()) return;

        AudioUtils.PlaySFX(buildingFinishedClips);
    }

    private void OnBuildingDemolished(Building building)
    {
        if (!ShouldPlay()) return;

        AudioUtils.PlaySFX(buildingDemolishedClips);
    }

    private bool ShouldPlay()
    {
        return BuildingsLoader.Instance.isLoaded;
    }
}