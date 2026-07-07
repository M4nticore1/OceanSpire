using UnityEngine;
using UnityEngine.Audio;

public class UIAudioSystem : AudioSystem
{
    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private AudioClip[] releaseClips;

    protected override void Subscribe()
    {
        base.Subscribe();

        CustomButton.OnButtonReleased += OnButtonReleased;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        CustomButton.OnButtonReleased -= OnButtonReleased;
    }

    private void OnButtonReleased(CustomButton button)
    {
        Debug.Log($"Released {gameObject}");
        AudioUtils.PlaySFX(releaseClips, mixerGroup);
    }
}