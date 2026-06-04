using UnityEngine;

public class LootContainerAudioSystem : MonoBehaviour
{
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
        AudioUtils.PlaySFXAtPosition(containerTakenClips, container.transform.position, minDistance, maxDistance);
    }

    private void OnContainerStartedFalling(DriftingLoot container)
    {
        AudioUtils.PlaySFXAtPosition(containerFallStartClips, container.transform.position, minDistance, maxDistance);
    }

    private void OnContainerFalled(DriftingLoot container)
    {
        AudioUtils.PlaySFXAtPosition(containerLandedClips, container.transform.position, minDistance, maxDistance);
    }
}