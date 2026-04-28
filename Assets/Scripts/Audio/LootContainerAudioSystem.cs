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
        LootContainer.OnContainerTaken += OnContainerTaked;
        LootContainer.OnContainerStartedFalling += OnContainerStartedFalling;
        LootContainer.onContainerLanded += OnContainerFalled;
    }

    private void OnDisable()
    {
        LootContainer.OnContainerTaken -= OnContainerTaked;
        LootContainer.OnContainerStartedFalling -= OnContainerStartedFalling;
        LootContainer.onContainerLanded -= OnContainerFalled;
    }

    private void OnContainerTaked(LootContainer container)
    {
        AudioUtils.PlaySFXAtPosition(containerTakenClips, container.transform.position, minDistance, maxDistance);
    }

    private void OnContainerStartedFalling(LootContainer container)
    {
        AudioUtils.PlaySFXAtPosition(containerFallStartClips, container.transform.position, minDistance, maxDistance);
    }

    private void OnContainerFalled(LootContainer container)
    {
        AudioUtils.PlaySFXAtPosition(containerLandedClips, container.transform.position, minDistance, maxDistance);
    }
}