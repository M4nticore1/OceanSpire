using UnityEngine;
using UnityEngine.Audio;

public class SelectAudioSystem : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private AudioClip[] selectAudioClips;

    private void OnEnable()
    {
        Building.OnBuildingSelected += OnBuildingSelected;
        Boat.OnBoatSelected += OnBoatSelected;
        Human.OnHumanSelected += OnHumanSelected;
    }

    private void OnDisable()
    {
        Building.OnBuildingSelected -= OnBuildingSelected;
        Boat.OnBoatSelected -= OnBoatSelected;
        Human.OnHumanSelected -= OnHumanSelected;
    }

    private void OnBuildingSelected(Building building)
    {
        AudioUtils.PlaySFX(selectAudioClips, mixerGroup);
    }

    private void OnBoatSelected(Boat boat)
    {
        AudioUtils.PlaySFX(selectAudioClips, mixerGroup);
    }

    private void OnHumanSelected(Human human)
    {
        AudioUtils.PlaySFX(selectAudioClips, mixerGroup);
    }
}