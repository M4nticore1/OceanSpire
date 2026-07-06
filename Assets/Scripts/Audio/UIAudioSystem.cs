using UnityEngine;
using UnityEngine.Audio;

public class UIAudioSystem : AudioSystem
{
    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private AudioClip[] releaseClips;

    protected override void Subscribe()
    {
        CustomButton.OnButtonReleased += OnButtonReleased;
    }

    protected override void Unsubscribe()
    {
        CustomButton.OnButtonReleased += OnButtonReleased;
    }

    private void OnButtonReleased(CustomButton button)
    {
        AudioUtils.PlaySFX(releaseClips, mixerGroup);
    }
}