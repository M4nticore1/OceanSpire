using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomDropdown : UIBehaviour
{
    private RectTransform rectTransform = null;
    [SerializeField] private CustomButton button = null;
    [SerializeField] private LayoutGroup dropdownLayoutGroup = null;
    [SerializeField] private float transitionSpeed = 10f;
    private float transitionAlpha = 0;
    private Vector3 targetScale;
    private bool isOpened = false;
    private bool isAnimating = false;

    protected override void Awake()
    {
        base.Awake();

        rectTransform = GetComponent<RectTransform>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        button.onReleased += OnButtonClicked;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        button.onReleased -= OnButtonClicked;
    }

    private void Update()
    {
        if (isAnimating) {
            MoveDropdown();
        }
    }

    private void OnButtonClicked()
    {
        if (isOpened)
            CloseDropdown();
        else
            OpenDropdown();
    }

    private void OpenDropdown()
    {
        targetScale = Vector3.one;
        isOpened = true;
        OnStateShanged();
    }

    private void CloseDropdown()
    {
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
        transitionAlpha += Time.deltaTime;
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, transitionAlpha);
        if (transitionAlpha >= 1f) {
            isAnimating = false;
            transitionAlpha = 0f;
        }
    }
}
