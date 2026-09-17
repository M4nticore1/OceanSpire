using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class AmountableWidget : MonoBehaviour
{
    public List<IAmountable> Amounts { get; private set; } = new();
    public IAmountable Limit { get; private set; }

    [Header("UI")]
    [SerializeField] private CustomButton infoButton;
    [SerializeField] private TextLocalizer itemNameText;
    [SerializeField] private TextMeshProUGUI resourceAmountText;
    [SerializeField] private Image resourceImage;
    [SerializeField] private Image resourceAmountBar;

    [Header("Parameters")]
    [SerializeField] private bool useLimit = false;
    [SerializeField] private bool zeroAmountDestroy = false;

    [Header("Color")]
    [SerializeField] private ItemWidgetColorType changeColorType = ItemWidgetColorType.Greater;

    [SerializeField] private bool useAmountColors = false;
    public bool UseAmountColors {
        get {
            return useAmountColors;
        }
        set {
            useAmountColors = value;
        }
    }

    [SerializeField] private Color enoughAmountColor = Color.green;
    [SerializeField] private Color notEnoughAmountColor = Color.red;

    private Coroutine UpdateDisplayCoroutine;

    protected virtual void OnEnable()
    {
        Subscribe();

        if (infoButton != null)
            infoButton.OnReleased.AddListener(HandleInfoButtonClicked);

        RunUpdateDisplayEndOfFrame();
    }

    protected virtual void OnDisable()
    {
        Unsubscribe();

        if (infoButton != null)
            infoButton.OnReleased.RemoveListener(HandleInfoButtonClicked);
    }

    protected virtual void Start()
    {
        RunUpdateDisplayEndOfFrame();
    }

    protected abstract void HandleInfoButtonClicked();

    protected abstract LocalizationItem GetName();

    protected abstract Sprite GetIcon();

    protected virtual bool ShouldDestroy()
    {
        if (!zeroAmountDestroy) return false;

        return true;
    }

    protected virtual int CalculateAmountsSum()
    {
        int sum = 0;

        foreach (var amountable in Amounts) {
            if (amountable == null)
                continue;

            sum += amountable.Amount;
        }

        return sum;
    }

    // Subscribe
    private void Subscribe()
    {
        foreach (var amount in Amounts) {
            if (amount == null)
                continue;

            amount.OnAmountChanged -= OnAmountChanged;
            amount.OnAmountChanged += OnAmountChanged;
        }

        if (Limit != null) {
            Limit.OnAmountChanged -= OnLimitChanged;
            Limit.OnAmountChanged += OnLimitChanged;
        }
    }

    private void Unsubscribe()
    {
        foreach (var amount in Amounts) {
            if (amount == null)
                continue;

            amount.OnAmountChanged -= OnAmountChanged;
        }

        if (Limit != null)
            Limit.OnAmountChanged -= OnLimitChanged;
    }

    // Amounts
    public void AddAmount(IAmountable amount)
    {
        if (amount == null)
            return;

        if (Amounts.Contains(amount))
            return;

        Amounts.Add(amount);

        if (isActiveAndEnabled) {
            amount.OnAmountChanged -= OnAmountChanged;
            amount.OnAmountChanged += OnAmountChanged;
        }

        UpdateDisplay();
    }

    public void RemoveAmount(IAmountable amount)
    {
        if (amount == null)
            return;

        if (!Amounts.Remove(amount))
            return;

        amount.OnAmountChanged -= OnAmountChanged;

        UpdateDisplay();
    }

    public void SetLimit(IAmountable amount)
    {
        if (Limit == amount)
            return;

        if (Limit != null)
            Limit.OnAmountChanged -= OnLimitChanged;

        Limit = amount;

        if (isActiveAndEnabled && Limit != null) {
            Limit.OnAmountChanged -= OnLimitChanged;
            Limit.OnAmountChanged += OnLimitChanged;
        }

        UpdateDisplay();
    }

    // UI
    public void SetColor(Color color)
    {
        if (resourceAmountText == null)
            return;

        resourceAmountText.color = color;
    }

    public void SetAmountText(int amount)
    {
        if (resourceAmountText == null)
            return;

        resourceAmountText.SetText(amount.ToString());
    }

    public void SetAmountText(int amount, int limit)
    {
        if (resourceAmountText == null)
            return;

        resourceAmountText.SetText($"{amount}/{limit}");
    }

    // Display
    private void UpdateDisplay()
    {
        UpdateAmountAndLimitText();
        TryUpdateResourceBar();
        TryUpdateAmountColor();
        UpdateName();
        UpdateIcon();
    }

    protected void UpdateAmountAndLimitText()
    {
        int amount = CalculateAmountsSum();

        if (useLimit && Limit != null)
            SetAmountText(amount, Limit.Amount);
        else
            SetAmountText(amount);
    }

    protected void UpdateName()
    {
        if (itemNameText == null)
            return;

        itemNameText.SetLocalizationItem(GetName());
    }

    protected void UpdateIcon()
    {
        if (resourceImage == null)
            return;

        resourceImage.sprite = GetIcon();
    }

    private void TryUpdateResourceBar()
    {
        if (resourceAmountBar == null)
            return;

        if (Limit == null || Limit.Amount <= 0) {
            resourceAmountBar.fillAmount = 0f;
            return;
        }

        var fillAmount = (float)CalculateAmountsSum() / Limit.Amount;

        resourceAmountBar.fillAmount = Mathf.Clamp01(fillAmount);
    }

    private void TryUpdateAmountColor()
    {
        if (!useAmountColors)
            return;

        SetColor(IsEnough() ? enoughAmountColor : notEnoughAmountColor);
    }

    private bool IsEnough()
    {
        int amount = CalculateAmountsSum();

        if (Amounts.Count <= 0 || Limit == null)
            return true;

        switch (changeColorType) {
            case ItemWidgetColorType.GreaterOrEqual:
                return amount >= Limit.Amount;

            case ItemWidgetColorType.Greater:
                return amount > Limit.Amount;

            case ItemWidgetColorType.Zero:
                return amount > 0;

            default:
                return false;
        }
    }

    // Coroutines
    private void RunUpdateDisplayEndOfFrame()
    {
        if (UpdateDisplayCoroutine == null) {
            UpdateDisplayCoroutine = StartCoroutine(UpdateDisplayEndOfFrame());
        }
    }

    private IEnumerator UpdateDisplayEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        UpdateDisplayCoroutine = null;
        UpdateDisplay();
    }

    private void OnAmountChanged(int amount)
    {
        if (ShouldDestroy())
            return;

        UpdateAmountAndLimitText();
        TryUpdateResourceBar();
        TryUpdateAmountColor();
    }

    private void OnLimitChanged(int amount)
    {
        UpdateAmountAndLimitText();
        TryUpdateResourceBar();
        TryUpdateAmountColor();
    }
}