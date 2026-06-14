using System;
using UnityEngine;

public enum ToggleTransition
{
    SpriteSwap,
    Animation
}

public class CustomToggle : MonoBehaviour
{
    [SerializeField] private ToggleTransition transition;
    [SerializeField] private CustomButton button;
    [SerializeField] private RectTransform checkmark;
    [SerializeField] private RectTransform toggleOnPosition;
    [SerializeField] private RectTransform toggleOffPosition;
    [SerializeField] private float toggleSwapSpeed = 10f;

    [SerializeField] private bool isOn = false;
    public bool IsOn => isOn;

    private Vector3 targetPosition = Vector3.zero;
    private float animationAlpha = 1f;

    public event Action<bool> OnValueChanged;

    private void OnEnable()
    {
        button.OnReleased.AddListener(OnButtonClicked);

        SetOn(IsOn);
        SetAnimationAlpha(1f);
    }

    private void OnDisable()
    {
        button.OnReleased.RemoveListener(OnButtonClicked);
    }

    private void Start()
    {
        UpdateTargetPosition(IsOn);
    }

    private void Update()
    {
        if (transition == ToggleTransition.Animation) {
            animationAlpha = Mathf.Lerp(animationAlpha, 1f, toggleSwapSpeed * Time.deltaTime);
            checkmark.position = Vector3.Lerp(checkmark.position, targetPosition, animationAlpha);
        }
    }

    public void SwapOn()
    {
        SetOn(!isOn);
    }

    public void SetOn(bool value)
    {
        if (value == isOn) return;

        isOn = value;
        UpdateTargetPosition(value);
        SetAnimationAlpha(0f);
        UpdateCheckmarkEnabled(value);

        OnValueChanged?.Invoke(value);
    }

    private void UpdateTargetPosition(bool isOn)
    {
        if (transition != ToggleTransition.Animation) return;

        targetPosition = isOn ? toggleOnPosition.position : toggleOffPosition.position;
    }

    private void SetAnimationAlpha(float value)
    {
        if (transition != ToggleTransition.Animation) return;

        animationAlpha = value;
    }

    private void UpdateCheckmarkEnabled(bool isOn)
    {
        if (transition != ToggleTransition.SpriteSwap) return;

        checkmark.gameObject.SetActive(isOn);
    }

    private void OnButtonClicked()
    {
        SwapOn();
    }
}