using UnityEngine;

public class HarvestEffectPositionHandler : MonoBehaviour
{
    [SerializeField] private RectTransform startTransform;
    public RectTransform StartTransform => startTransform;

    [SerializeField] private RectTransform targetTransform;
    public RectTransform TargetTransform => targetTransform;
}