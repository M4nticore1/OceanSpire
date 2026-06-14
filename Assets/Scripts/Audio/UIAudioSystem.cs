using UnityEngine;
using UnityEngine.Audio;

public class UIAudioSystem : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup mixerGoup;
    [SerializeField] private AudioClip[] releaseClips;

    private void OnEnable()
    {
        CustomButton.onButtonReleased.AddListener(OnButtonReleased);
    }

    private void OnDisable()
    {
        CustomButton.onButtonReleased.RemoveListener(OnButtonReleased);
    }

    private void OnButtonReleased(CustomButton button)
    {
        AudioUtils.PlaySFX(releaseClips, mixerGoup);
    }
}