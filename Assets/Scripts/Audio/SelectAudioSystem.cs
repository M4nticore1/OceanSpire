using UnityEngine;
using UnityEngine.Audio;

public class SelectAudioSystem : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup mixerGoup;
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
        AudioUtils.PlaySFX(selectAudioClips, mixerGoup);
    }

    private void OnBoatSelected(Boat boat)
    {
        AudioUtils.PlaySFX(selectAudioClips, mixerGoup);
    }

    private void OnHumanSelected(Human human)
    {
        AudioUtils.PlaySFX(selectAudioClips, mixerGoup);
    }
}