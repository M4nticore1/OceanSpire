using UnityEngine;

public class SelectAudioSystem : MonoBehaviour
{
    [SerializeField] private AudioClip[] selectAudioClips;

    private void OnEnable()
    {
        Building.onBuildingSelected += OnBuildingSelected;
        Boat.onBoatSelected += OnBoatSelected;
        Human.onHumanSelected += OnHumanSelected;
    }

    private void OnDisable()
    {
        Building.onBuildingSelected -= OnBuildingSelected;
        Boat.onBoatSelected -= OnBoatSelected;
        Human.onHumanSelected -= OnHumanSelected;
    }

    private void OnBuildingSelected(Building building)
    {
        AudioUtils.PlaySFX(AudioUtils.GetRandomAudioClip(selectAudioClips));
    }

    private void OnBoatSelected(Boat boat)
    {
        AudioUtils.PlaySFX(AudioUtils.GetRandomAudioClip(selectAudioClips));
    }

    private void OnHumanSelected(Human human)
    {
        AudioUtils.PlaySFX(AudioUtils.GetRandomAudioClip(selectAudioClips));
    }
}