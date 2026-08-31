using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class CustomDropdown : CustomUI
{
    [SerializeField] private CustomButton button;
    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private float transitionSpeed = 1f;

    public bool IsListeningClick = true;

    private float transitionAlpha = 0;
    private Vector3 targetScale;

    private bool isOpened = false;
    private bool isAnimating = false;

    public event Action OnOpened;
    public event Action OnClosed;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (customUIManager != null) {
            customUIManager.RegisterCustomDropdown(this);
        }
        else {
            Debug.Log($"[{nameof(CustomDropdown)}] Custom UI Manager is not valid!");
        }

        button.OnReleased.AddListener(OnButtonClicked);
        InputListener.Instance.OnReleased += OnPointerReleased;

        SetTransitionAlpha(1f);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (customUIManager != null) {
            customUIManager.UnregisterCustomDropdown(this);
        }

        button.OnReleased.RemoveListener(OnButtonClicked);
        InputListener.Instance.OnReleased -= OnPointerReleased;
    }

    public override void Tick()
    {
        base.Tick();

        if (!enabled) return;
        if (!gameObject.activeSelf) return;
        if (!gameObject.activeInHierarchy) return;

        if (isAnimating) {
            MoveDropdown();
        }
    }

    public void SetListeningClick(bool value)
    {
        IsListeningClick = value;
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
        isOpened = true;
        SetTransitionAlpha(0f);
        targetScale = Vector3.one;
        OnStateShanged();
        OnOpened?.Invoke();
    }

    private void Close()
    {
        isOpened = false;
        SetTransitionAlpha(0f);
        targetScale = Vector3.zero;
        OnStateShanged();
        OnClosed?.Invoke();
    }

    private void OnStateShanged()
    {
        isAnimating = true;
    }

    private void MoveDropdown()
    {
        transitionAlpha = math.lerp(transitionAlpha, 1f, transitionSpeed * Time.deltaTime);
        transitionAlpha = Mathf.Clamp01(transitionAlpha);
        SetTransitionAlpha(transitionAlpha);
    }

    private void SetTransitionAlpha(float value)
    {
        transitionAlpha = value;

        Vector3 scale = viewport.localScale;
        scale.y = math.lerp(scale.y, targetScale.y, value);
        viewport.localScale = scale;

        if (transitionAlpha >= 1f) {
            isAnimating = false;
        }
    }

    private void OnPointerReleased()
    {
        if (!isOpened) return;
        if (!IsListeningClick) return;
        if (PointerUtils.GetRaycastUIResult().gameObject == button.gameObject) return;

        Close();
    }
}