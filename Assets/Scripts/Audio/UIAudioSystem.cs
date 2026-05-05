using UnityEngine;

public class UIAudioSystem : MonoBehaviour
{
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
        AudioUtils.PlaySFX(releaseClips);
    }
}