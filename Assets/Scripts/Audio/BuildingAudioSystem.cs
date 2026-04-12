using UnityEngine;

public class BuildingAudioSystem : MonoBehaviour
{
    [SerializeField] private AudioClip buildingFinishedClip;

    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;

    private void OnEnable()
    {
        BuildingPlace.onClicked += OnBuildingPlaceClicked;
    }

    private void OnDisable()
    {
        BuildingPlace.onClicked -= OnBuildingPlaceClicked;
    }

    private void OnBuildingPlaceClicked(Building building)
    {
        AudioUtils.PlaySFXAtPosition(buildingFinishedClip, building.transform.position, minDistance, maxDistance);
    }
}