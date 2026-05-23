using UnityEngine;

public class SelectAudioSystem : MonoBehaviour
{
    [SerializeField] private AudioClip[] selectAudioClips;

    private void OnEnable()
    {
        Building.OnBuildingSelected += OnBuildingSelected;
        Boat.onBoatSelected += OnBoatSelected;
        Human.OnHumanSelected += OnHumanSelected;
    }

    private void OnDisable()
    {
        Building.OnBuildingSelected -= OnBuildingSelected;
        Boat.onBoatSelected -= OnBoatSelected;
        Human.OnHumanSelected -= OnHumanSelected;
    }

    private void OnBuildingSelected(Building building)
    {
        AudioUtils.PlaySFX(selectAudioClips);
    }

    private void OnBoatSelected(Boat boat)
    {
        AudioUtils.PlaySFX(selectAudioClips);
    }

    private void OnHumanSelected(Human human)
    {
        AudioUtils.PlaySFX(selectAudioClips);
    }
}