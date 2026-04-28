using UnityEngine;

public class UIAudioSystem : MonoBehaviour
{
    [SerializeField] private AudioClip[] releaseClips;

    private void OnEnable()
    {
        CustomButton.onButtonReleased += OnButtonReleased;
    }

    private void OnDisable()
    {
        CustomButton.onButtonReleased -= OnButtonReleased;
    }

    private void OnButtonReleased(CustomButton button)
    {
        AudioUtils.PlaySFX(releaseClips);
    }
}