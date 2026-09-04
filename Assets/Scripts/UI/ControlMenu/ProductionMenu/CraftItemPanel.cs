using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftItemPanel : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private ResourceWidget consumeResourceWidgetPrefab;
    [SerializeField] private ResourceWidget craftResourceWidgetPrefab;

    [Header("UI")]
    [SerializeField] private CustomButton button;
    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private Image progressBar;
    [SerializeField] private Transform producedResourceSlot;
    [SerializeField] private LayoutGroup consumedResourcesSlot;

    [Header("Color")]
    [SerializeField] private Color positiveBonusColor = Color.HSVToRGB(120, 75, 100);
    [SerializeField] private Color negativeBonusColor = Color.HSVToRGB(0, 75, 100);

    private FlickingImage flickingProgressBar;
    private CraftingModule craftingModule;
    private CraftItemInstance craftItem;

    private bool isSelected = false;

    private void Awake()
    {
        if (progressBar)
            flickingProgressBar = progressBar.GetComponent<FlickingImage>();
    }

    private void OnEnable()
    {
        if (button) {
            button.OnSelected.AddListener(OnSelected);
            button.OnDeselected.AddListener(OnDeselected);
        }
    }

    private void OnDisable()
    {
        if (button) {
            button.OnSelected.RemoveListener(OnSelected);
            button.OnDeselected.RemoveListener(OnDeselected);
        }
    }

    private void OnDestroy()
    {
        if (craftItem != null) {
            craftItem.OnSpeedBonusChanged -= OnCraftingSpeedBonusChanged;
        }
    }

    private void Update()
    {
        if (!isSelected) return;
        if (!craftingModule) return;
        if (!craftingModule.IsWorking) return;

        craftItem.UpdateCraftingTimeByFinishTime();

        UpdateTimer();
        UpdateProgressBar();
    }

    public void Init(CraftingModule craftingModule, CraftItemInstance craftItem, SelectGroup selectGroup)
    {
        if (!craftingModule || craftItem == null) {
            Debug.LogError($"[{nameof(CraftItemPanel)}] Invalid Init parameters");
            return;
        }

        this.craftingModule = craftingModule;
        this.craftItem = craftItem;

        if (button) button.SetSelectGroup(selectGroup);

        UpdateSelected();
        CreateProducedResourceWidget();
        CreateConsumedResourcesWidget();

        craftItem.UpdateCraftingTimeByFinishTime();
        UpdateTimer();
        UpdateProgressBar();

        craftItem.OnSpeedBonusChanged += OnCraftingSpeedBonusChanged;
    }

    public void Select()
    {
        if (button.State == CustomButtonState.Selected) return;

        button.SetState(CustomButtonState.Selected);
        button.EndTransitionAnimation();
    }

    private void Deselect()
    {
        if (button.State == CustomButtonState.Idle) return;

        button.SetState(CustomButtonState.Idle);
        button.EndTransitionAnimation();
    }

    private void UpdateSelected()
    {
        if (craftItem == null || !craftingModule) return;

        if (craftItem == craftingModule.SelectedCraftItem) {
            Select();
        }
        else {
            Deselect();
        }
    }

    private void CreateProducedResourceWidget()
    {
        if (!craftResourceWidgetPrefab || !producedResourceSlot) return;

        var widget = Instantiate(craftResourceWidgetPrefab, producedResourceSlot.transform);
        widget.SetItemDefinition(craftItem.Definition.ProduceItem.Definition);
        widget.AddAmount(craftItem.Definition.ProduceItem);
    }

    private void CreateConsumedResourcesWidget()
    {
        if (!consumeResourceWidgetPrefab || !consumedResourcesSlot) return;

        foreach (var resource in craftItem.Definition.ConsumeResources) {
            var widget = Instantiate(consumeResourceWidgetPrefab, consumedResourcesSlot.transform);
            var definition = resource.Definition;

            widget.SetItemDefinition(definition);
            widget.AddAmount(CityStorage.Instance.Inventory.GetInventoryItem(definition.ItemId));
            widget.SetLimit(resource);
        }
    }

    private void UpdateTimer()
    {
        if (!timer) return;

        string text = "";

        if (isSelected) {
            int craftTime = craftItem.GetCraftTimeWithBonus();
            int currentCraftingTime = craftItem.CurrentCraftingTime;

            if (craftItem.IsCraftingFinished()) {
                currentCraftingTime = craftTime;
            }

            currentCraftingTime = Mathf.Clamp(currentCraftingTime, 0, craftTime);
            text = TimeFormatter.SecondToFractionalTimer(currentCraftingTime, craftTime);
        }
        else {
            int targetTime = craftItem.GetCraftTimeWithBonus();
            text = TimeFormatter.SecondsToMinuteTimer(targetTime);
        }

        float bonusPercent = (craftItem.CraftingSpeedMultiplier - 1) * 100f;
        string bonusColorHex = ColorUtility.ToHtmlStringRGB(bonusPercent > 0 ? positiveBonusColor : negativeBonusColor);
        string bonusText = bonusPercent > 0f ? $" <color=#{bonusColorHex}>(-{bonusPercent:F0}%)</color>" : bonusPercent < 0f ? $" <color=#{bonusColorHex}>(+{bonusPercent:F0}%)</color>" : "";

        text += bonusText;
        timer.SetText(text);
    }

    private void UpdateProgressBar()
    {
        if (!progressBar) return;

        int craftTime = craftItem.GetCraftTimeWithBonus();
        int currentCraftingTime = craftItem.CurrentCraftingTime;

        float amount = 0f;

        if (isSelected && craftTime > 0) {
            amount = Mathf.Clamp01((float)currentCraftingTime / craftTime);

            if (flickingProgressBar != null) {
                bool isFinished = currentCraftingTime >= craftTime;
                flickingProgressBar.SetFlickingEnabled(isFinished);
            }
        }

        progressBar.fillAmount = amount;
    }

    private void OnCraftingSpeedBonusChanged(float bonus)
    {
        UpdateTimer();
        UpdateProgressBar();
    }

    private void OnSelected()
    {
        isSelected = true;

        if (craftingModule.SelectedCraftItem != craftItem) {
            craftingModule.SetCraftingItemAndApply(craftItem);
        }

        UpdateTimer();
        UpdateProgressBar();
    }

    private void OnDeselected()
    {
        isSelected = false;

        UpdateTimer();
        UpdateProgressBar();
    }
}