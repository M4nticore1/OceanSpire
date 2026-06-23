using UnityEngine;
using UnityEngine.Audio;

public class LootContainerAudioSystem : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup mixerGroup;

    [SerializeField] private AudioClip[] containerTakenClips;
    [SerializeField] private AudioClip[] containerFallStartClips;
    [SerializeField] private AudioClip[] containerLandedClips;

    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;

    private void OnEnable()
    {
        SwimmingDriftingLoot.OnContainerTaken += OnContainerTaked;
        FlyingDriftingLoot.OnFlyingLootStartedFalling += OnContainerStartedFalling;
        FlyingDriftingLoot.onContainerLanded += OnContainerFalled;
    }

    private void OnDisable()
    {
        SwimmingDriftingLoot.OnContainerTaken -= OnContainerTaked;
        FlyingDriftingLoot.OnFlyingLootStartedFalling -= OnContainerStartedFalling;
        FlyingDriftingLoot.onContainerLanded -= OnContainerFalled;
    }

    private void OnContainerTaked(DriftingLoot container)
    {
        AudioUtils.PlaySFXAtPosition(containerTakenClips, container.transform.position, minDistance, maxDistance, mixerGroup);
    }

    private void OnContainerStartedFalling(DriftingLoot container)
    {
        AudioUtils.PlaySFXAtPosition(containerFallStartClips, container.transform.position, minDistance, maxDistance, mixerGroup);
    }

    private void OnContainerFalled(DriftingLoot container)
    {
        AudioUtils.PlaySFXAtPosition(containerLandedClips, container.transform.position, minDistance, maxDistance, mixerGroup);
    }
}