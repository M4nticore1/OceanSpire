using UnityEngine;
using UnityEngine.UI;

public class HarvestEffectWidget : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private RectTransform rectTransform;
    public RectTransform RectTransform => rectTransform;

    [SerializeField] private Image resourceImage;

    [Header("Animation")]
    [SerializeField] private float animationSpeed = 1f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public ItemInstance Item { get; private set; }

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private float moveProgress = 0f;

    public void Init(ItemInstance item, Vector3 startWorldPos, Vector3 targetWorldPos)
    {
        Item = item;
        startPosition = startWorldPos;
        targetPosition = targetWorldPos;

        rectTransform.position = startWorldPos;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.zero;

        moveProgress = 0f;

        UpdateResourceImage();
    }

    public void Tick()
    {
        float deltaTime = Time.deltaTime;
        moveProgress += deltaTime * animationSpeed;

        if (!TryDestroy()) {
            ProcessMove();
            ProcessScale();
        }
    }

    private void ProcessMove()
    {
        float evaluatedMove = moveCurve.Evaluate(moveProgress);
        rectTransform.position = Vector3.Lerp(startPosition, targetPosition, evaluatedMove);
    }

    private void ProcessScale()
    {
        float evaluatedScale = scaleCurve.Evaluate(moveProgress);
        rectTransform.localScale = Vector3.one * evaluatedScale;
    }

    private void UpdateResourceImage()
    {
        if (Item == null) return;

        resourceImage.sprite = Item.Definition.ItemIcon;
    }

    private bool TryDestroy()
    {
        if (!ShouldDestroy()) return false;
        Destroy(gameObject);
        return true;
    }

    private bool ShouldDestroy()
    {
        return moveProgress >= 1f;
    }
}