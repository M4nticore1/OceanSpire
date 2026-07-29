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
    private int amountToDiscard = 0;

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

    private void OnDestroy()
    {
        UnsubscribeItem(item);
    }

    public void Show(ItemInstance itemInstance)
    {
        if (itemInstance == null) {
            Debug.LogError($"[{nameof(DiscardItemMenu)}] Item Instance is not valid!");
            return;
        }

        UnsubscribeItem(item);
        item = itemInstance;
        SubscribeItem(item);

        Show();
    }

    public void Show()
    {
        if (IsShowed) return;
        if (item == null) return;

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

    private void SubscribeItem(ItemInstance item)
    {
        if (item == null) return;

        item.OnItemAmountChanged += HandleItemAmountChanged;
    }

    private void UnsubscribeItem(ItemInstance item)
    {
        if (item == null) return;

        item.OnItemAmountChanged -= HandleItemAmountChanged;
    }

    private void DiscardItem()
    {
        if (item == null) {
            Debug.LogError($"[{nameof(DiscardItemMenu)}] Item is not valid!");
            return;
        }

        item.RemoveAmount(amountToDiscard);
    }

    private void SetAmountToRemove(int value)
    {
        if (item == null || item.Amount <= 0) {
            amountToDiscard = 0;
            return;
        }

        amountToDiscard = Mathf.Clamp(value, 0, item.Amount);
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
        discardButton.SetState(amountToDiscard > 0 ? CustomButtonState.Idle : CustomButtonState.Disabled);
    }

    private void UpdateInputFieldValue(bool force = false)
    {
        if (!force && amountInputField.isFocused) return;

        amountInputField.SetTextWithoutNotify(amountToDiscard.ToString());
    }

    private void UpdateSliderValue(bool force = false)
    {
        if (!force && EventSystem.current && EventSystem.current.currentSelectedGameObject == amountSlider.gameObject) return;

        if (item == null || item.Amount <= 0) {
            amountSlider.value = 0;
            return;
        }

        amountSlider.value = (float)amountToDiscard / item.Amount;
    }

    private void OnDiscardInputFieldValueChanged(string value)
    {
        if (item == null || !amountInputField.isFocused) return;

        if (string.IsNullOrEmpty(value)) {
            SetAmountToRemove(0);
        }
        else if (int.TryParse(value, out var parsedValue)) {
            SetAmountToRemove(parsedValue);

            if (parsedValue != amountToDiscard) {
                amountInputField.SetTextWithoutNotify(amountToDiscard.ToString());
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

        SetAmountToRemove((int)(Mathf.Lerp(0, item.Amount, value)));
        UpdateInputFieldValue();
        UpdateDiscardButtonEnabled();
    }

    private void OnDiscardButtonClicked()
    {
        DiscardItem();
        Hide();
    }

    private void HandleItemAmountChanged(ItemInstance item)
    {
        amountToDiscard = Mathf.Clamp(amountToDiscard, 0, item.Amount);
        SetAmountToRemove(amountToDiscard);

        UpdateItemName();
        UpdateInputFieldValue(true);
        UpdateSliderValue(true);
        UpdateDiscardButtonEnabled();
    }

    private int GetStartAmount()
    {
        if (item == null) return 0;
        if (item.Amount <= 0) return 0;

        var amount = (int)(item.Amount * startDiscardPercent);
        return Mathf.Clamp(amount, 1, item.Amount);
    }
}