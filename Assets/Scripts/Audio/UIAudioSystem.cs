using UnityEngine;
using UnityEngine.Audio;

public class UIAudioSystem : AudioSystem
{
    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private AudioClip[] releaseClips;

    protected override void Subscribe()
    {
        CustomButton.onButtonReleased.AddListener(OnButtonReleased);
    }

    protected override void Unsubscribe()
    {
        CustomButton.onButtonReleased.RemoveListener(OnButtonReleased);
    }

    private void OnButtonReleased(CustomButton button)
    {
        AudioUtils.PlaySFX(releaseClips, mixerGroup);
    }
}