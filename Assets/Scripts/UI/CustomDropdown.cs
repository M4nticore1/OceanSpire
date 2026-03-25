using Unity.Mathematics;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomDropdown : UIBehaviour
{
    [SerializeField] private CustomButton button;
    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private float transitionSpeed = 1f;
    private float transitionAlpha = 0;
    private Vector3 targetScale;
    private bool isOpened = false;
    private bool isAnimating = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        button.onReleased += OnButtonClicked;
        InputListener.Instance.onReleased += OnPointerReleased;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        button.onReleased -= OnButtonClicked;
        InputListener.Instance.onReleased -= OnPointerReleased;
    }

    protected override void Start()
    {
        base.Start();

        SetTransitionAlpha(1f);
    }

    private void Update()
    {
        if (!isAnimating) return;

        MoveDropdown();
    }

    private void OnButtonClicked()
    {
        if (isOpened)
            Close();
        else
            Open();
    }

    private void Open()
    {
        SetTransitionAlpha(0f);
        targetScale = Vector3.one;
        isOpened = true;
        OnStateShanged();
    }

    private void Close()
    {
        SetTransitionAlpha(0f);
        targetScale = Vector3.zero;
        isOpened = false;
        OnStateShanged();
    }

    private void OnStateShanged()
    {
        isAnimating = true;
    }

    private void MoveDropdown()
    {
        transitionAlpha = math.lerp(transitionAlpha, 1f, transitionSpeed * Time.deltaTime);
        SetTransitionAlpha(transitionAlpha);
    }

    private void SetTransitionAlpha(float value)
    {
        transitionAlpha = value;

        Vector3 scale = viewport.localScale;
        scale.y = math.lerp(scale.y, targetScale.y, transitionAlpha);
        viewport.localScale = scale;

        if (transitionAlpha >= 1f) {
            isAnimating = false;
        }
    }

    private void OnPointerReleased()
    {
        if (!isOpened) return;
        //if (InputListener.Instance.startPosition != InputListener.Instance.lastPosition) return;
        if (PointerUtils.GetRaycastUIResult().gameObject == button.gameObject) return;

        Close();
    }
}
