using UnityEngine;
using UnityEngine.Audio;

public class DriftingLootAudioSystem : AudioSystem
{
    [SerializeField] private AudioMixerGroup mixerGroup;

    [Header("Swimming")]
    [SerializeField] private AudioClip[] containerCollectedClips;
    [SerializeField] private AudioClip[] clickAudioClips;

    [SerializeField] private float swimmingMinDistance = 20;
    [SerializeField] private float swimmingMaxDistance = 100;

    [Header("Flying")]
    [SerializeField] private AudioClip[] containerFallStartClips;
    [SerializeField] private AudioClip[] containerLandedClips;

    [SerializeField] private float flyingMinDistance = 100;
    [SerializeField] private float flyingMaxDistance = 300;

    protected override void Subscribe()
    {
        SwimmingDriftingLoot.OnGlobalCollected += OnDriftingLootCollected;
        FlyingDriftingLoot.OnFlyingLootStartedFalling += OnContainerStartedFalling;
        FlyingDriftingLoot.onContainerLanded += OnContainerFalled;
        DriftingLoot.OnGlobalClicked += OnDriftingLootClicked;
    }

    protected override void Unsubscribe()
    {
        SwimmingDriftingLoot.OnGlobalCollected -= OnDriftingLootCollected;
        FlyingDriftingLoot.OnFlyingLootStartedFalling -= OnContainerStartedFalling;
        FlyingDriftingLoot.onContainerLanded -= OnContainerFalled;
        DriftingLoot.OnGlobalClicked -= OnDriftingLootClicked;
    }

    private void OnDriftingLootCollected(DriftingLoot container)
    {
        AudioUtils.PlaySFXAtPosition(containerCollectedClips, container.transform.position, swimmingMinDistance, swimmingMaxDistance, mixerGroup);
    }

    private void OnContainerStartedFalling(DriftingLoot container)
    {
        AudioUtils.PlaySFXAtPosition(containerFallStartClips, container.transform.position, flyingMinDistance, flyingMaxDistance, mixerGroup);
    }

    private void OnContainerFalled(DriftingLoot container)
    {
        AudioUtils.PlaySFXAtPosition(containerLandedClips, container.transform.position, flyingMinDistance, flyingMaxDistance, mixerGroup);
    }

    private void OnDriftingLootClicked(DriftingLoot driftingLoot)
    {
        if (driftingLoot as SwimmingDriftingLoot) {
            AudioUtils.PlaySFXAtPosition(clickAudioClips, driftingLoot.transform.position, swimmingMinDistance, swimmingMaxDistance, mixerGroup);
        }
    }
}