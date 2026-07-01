using UnityEngine;
using UnityEngine.Audio;

public class SelectAudioSystem : AudioSystem
{
    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private AudioClip[] selectAudioClips;

    protected override void Subscribe()
    {
        Building.OnBuildingSelected += OnBuildingSelected;
        Boat.OnBoatSelected += OnBoatSelected;
        Human.OnHumanSelected += OnHumanSelected;
    }

    protected override void Unsubscribe()
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