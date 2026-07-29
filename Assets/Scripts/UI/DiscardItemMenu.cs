using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DiscardItemMenu : MonoBehaviour, IOpenable
{
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private TextLocalizer itemNameText;
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_InputField amountInputField;
    [SerializeField] private Slider amountSlider;
    [SerializeField] private CustomButton discardButton;
    [SerializeField] private CustomButton closeButton;
    [SerializeField] private float startDiscardPercent = 0.1f;

    private ItemInstance item;
    private int amountToRemove = 0;

    public bool IsShowed { get; private set; }

    public event Action OnShowed;
    public event Action OnHidden;

    private void OnEnable()
    {
        slidePanel.OnHidden += OnHide;
        amountInputField.onValueChanged.AddListener(OnDiscardInputFieldValueChanged);
        amountSlider.onValueChanged.AddListener(OnDiscardSliderValueChanged);
        discardButton.OnReleased.AddListener(OnDiscardButtonClicked);
        closeButton.OnReleased.AddListener(Hide);
    }

    private void OnDisable()
    {
        slidePanel.OnHidden -= OnHide;
        amountInputField.onValueChanged.RemoveListener(OnDiscardInputFieldValueChanged);
        amountSlider.onValueChanged.RemoveListener(OnDiscardSliderValueChanged);
        discardButton.OnReleased.RemoveListener(OnDiscardButtonClicked);
        closeButton.OnReleased.RemoveListener(Hide);
    }

    public void Show(ItemInstance itemInstance)
    {
        if (itemInstance == null) {
            Debug.LogError($"[{nameof(DiscardItemMenu)}] Item Instance is not valid!");
            return;
        }

        item = itemInstance;
        Show();
    }

    public void Show()
    {
        if (IsShowed) return;

        IsShowed = true;
        slidePanel.Show();

        SetAmountToRemove(GetStartAmount());
        UpdateItemName();
        UpdateItemImage();
        UpdateInputFieldValue();
        UpdateSliderValue();
        UpdateDiscardButtonEnabled();

        OnShowed?.Invoke();
    }

    public void Hide()
    {
        slidePanel.Hide();
    }

    private void OnHide()
    {
        if (!IsShowed) return;

        IsShowed = false;
        OnHidden?.Invoke();
    }

    private void DiscardItem()
    {
        if (item == null) {
            Debug.LogError($"[{nameof(DiscardItemMenu)}] Item is not valid!");
            return;
        }

        item.RemoveAmount(amountToRemove);
    }

    private void SetAmountToRemove(int value)
    {
        if (item == null || item.Amount <= 0) {
            amountToRemove = 0;
            return;
        }

        amountToRemove = Mathf.Clamp(value, 0, item.Amount);
    }

    private void UpdateItemName()
    {
        if (item == null) return;

        itemNameText.SetPlaceHolderLocalization(item);
    }

    private void UpdateItemImage()
    {
        if (item == null) return;

        itemImage.sprite = item.Definition.ItemIcon;
    }

    private void UpdateDiscardButtonEnabled()
    {
        discardButton.SetState(amountToRemove > 0 ? CustomButtonState.Idle : CustomButtonState.Disabled);
    }

    private void UpdateInputFieldValue()
    {
        if (amountInputField.isFocused) return;

        amountInputField.SetTextWithoutNotify(amountToRemove.ToString());
    }

    private void UpdateSliderValue()
    {
        if (EventSystem.current && EventSystem.current.currentSelectedGameObject == amountSlider.gameObject) return;

        if (item == null || item.Amount <= 0) {
            amountSlider.value = 0;
            return;
        }

        amountSlider.value = (float)amountToRemove / item.Amount;
    }

    private void OnDiscardInputFieldValueChanged(string value)
    {
        if (item == null || !amountInputField.isFocused) return;

        if (string.IsNullOrEmpty(value)) {
            SetAmountToRemove(0);
        }
        else if (int.TryParse(value, out var parsedValue)) {
            SetAmountToRemove(parsedValue);

            if (parsedValue != amountToRemove) {
                amountInputField.SetTextWithoutNotify(amountToRemove.ToString());
            }
        }
        else {
            SetAmountToRemove(0);
            amountInputField.SetTextWithoutNotify("0");
        }

        UpdateSliderValue();
        UpdateDiscardButtonEnabled();
    }

    private void OnDiscardSliderValueChanged(float value)
    {
        if (item == null) return;
        if (EventSystem.current && EventSystem.current.currentSelectedGameObject != amountSlider.gameObject) return;

        SetAmountToRemove(Mathf.RoundToInt(Mathf.Lerp(0, item.Amount, value)));
        UpdateInputFieldValue();
        UpdateDiscardButtonEnabled();
    }

    private void OnDiscardButtonClicked()
    {
        DiscardItem();
        Hide();
    }

    private int GetStartAmount()
    {
        if (item == null) return 0;

        return Mathf.RoundToInt(item.Amount * startDiscardPercent);
    }
}