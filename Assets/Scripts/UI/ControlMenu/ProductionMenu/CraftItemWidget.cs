using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftItemWidget : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private ItemWidget consumeResourceWidgetPrefab;
    [SerializeField] private ItemWidget craftResourceWidgetPrefab;

    [Header("UI")]
    [SerializeField] private TextLocalizer craftItemNameText;
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
    private CraftItemDefinition craftItemDefinition;

    private bool isSelected = false;

    private void Awake()
    {
        if (progressBar != null) {
            flickingProgressBar = progressBar.GetComponent<FlickingImage>();
        }
    }

    private void OnEnable()
    {
        if (button != null) {
            button.OnSelected.AddListener(OnSelected);
            button.OnDeselected.AddListener(OnDeselected);
        }
    }

    private void OnDisable()
    {
        if (button != null) {
            button.OnSelected.RemoveListener(OnSelected);
            button.OnDeselected.RemoveListener(OnDeselected);
        }
    }

    private void OnDestroy()
    {
        if (craftItem != null) {
            craftItem.OnCraftingSpeedTimeChanged -= OnCraftingSpeedBonusChanged;
        }
    }

    private void Update()
    {
        if (!isSelected) return;
        if (craftingModule == null) return;
        if (!craftingModule.IsWorking) return;

        craftItem.UpdateCraftingTimeByFinishTime();

        UpdateTimer();
        UpdateProgressBar();
    }

    public void Init(CraftItemDefinition craftItem)
    {
        if (craftItem == null) {
            Debug.LogError($"[{nameof(CraftItemWidget)}] Invalid Init parameters!");
            return;
        }

        craftItemDefinition = craftItem;

        UpdateCraftItemNameText();
        UpdateSelected();
        CreateProducedResourceWidget();
        CreateConsumedResourcesWidget();
        UpdateTimer();
        UpdateProgressBar();
    }

    public void Init(CraftItemInstance craftItem)
    {
        if (craftItem == null) {
            Debug.LogError($"[{nameof(CraftItemWidget)}] Invalid Init parameters!");
            return;
        }

        this.craftItem = craftItem;

        craftItem.UpdateCraftingTimeByFinishTime();
        craftItem.OnCraftingSpeedTimeChanged += OnCraftingSpeedBonusChanged;

        Init(craftItem.Definition);
    }

    public void Init(CraftItemInstance craftItem, CraftingModule craftingModule, SelectGroup selectGroup)
    {
        if (craftingModule == null || craftItem == null) {
            Debug.LogError($"[{nameof(CraftItemWidget)}] Invalid Init parameters!");
            return;
        }

        this.craftingModule = craftingModule;

        if (button != null) {
            button.SetSelectGroup(selectGroup);
        }

        Init(craftItem);
    }

    public void Select()
    {
        if (button == null) return;
        if (button.State == CustomButtonState.Selected) return;

        button.SetState(CustomButtonState.Selected);
        button.EndTransitionAnimation();
    }

    private void Deselect()
    {
        if (button == null) return;
        if (button.State == CustomButtonState.Idle) return;

        button.SetState(CustomButtonState.Idle);
        button.EndTransitionAnimation();
    }

    private void UpdateSelected()
    {
        if (button == null) return;
        if (craftItem == null) return;
        if (craftingModule == null) return;

        if (craftItem == craftingModule.SelectedCraftItem) {
            Select();
        }
        else {
            Deselect();
        }
    }

    private void CreateProducedResourceWidget()
    {
        if (craftResourceWidgetPrefab == null) return;
        if (producedResourceSlot == null) return;

        var widget = Instantiate(craftResourceWidgetPrefab, producedResourceSlot.transform);

        if (craftItem != null) {
            var definition = craftItem.Definition;
            if (definition == null) return;

            var produceItem = definition.ProduceItem;
            if (produceItem == null) return;

            widget.SetItemDefinition(produceItem.Definition);
            widget.AddAmount(produceItem);
        }
        else if (craftItemDefinition != null) {
            widget.SetItemDefinition(craftItemDefinition.ProduceItem.Definition);
            widget.AddAmount(craftItemDefinition.ProduceItem);
        }
    }

    private void CreateConsumedResourcesWidget()
    {
        if (consumeResourceWidgetPrefab == null) return;
        if (consumedResourcesSlot == null) return;

        CraftItemDefinition definition = null;
        if (craftItem != null) {
            definition = craftItem.Definition;
        }
        else if (craftItemDefinition != null) {
            definition = craftItemDefinition;
        }

        if (definition != null) {
            foreach (var resource in definition.ConsumeResources) {
                if (resource == null) continue;

                var widget = Instantiate(consumeResourceWidgetPrefab, consumedResourcesSlot.transform);
                var consumeDefinition = resource.Definition;

                widget.SetItemDefinition(consumeDefinition);
                widget.AddAmount(CityStorage.Instance.Inventory.GetInventoryItem(consumeDefinition.ItemId));
                widget.SetLimit(resource);
            }
        }
    }

    private void UpdateCraftItemNameText()
    {
        if (craftItemNameText == null) return;

        LocalizationItem localizationItem = null;

        if (craftItem != null) {
            localizationItem = craftItem.Definition.ProduceItem.Definition.NameLocalizationItem;
        }
        else if (craftItemDefinition != null) {
            localizationItem = craftItemDefinition.ProduceItem.Definition.NameLocalizationItem;
        }

        craftItemNameText.SetLocalizationItem(localizationItem);
    }

    private void UpdateTimer()
    {
        if (timer == null) return;

        var text = "";
        if (craftItem != null) {
            if (isSelected) {
                var craftTime = craftItem.GetCraftTimeWithBonus();
                var currentCraftingTime = craftItem.CurrentCraftingTime;

                if (craftItem.IsCraftingFinished()) {
                    currentCraftingTime = craftTime;
                }

                currentCraftingTime = Mathf.Clamp(currentCraftingTime, 0, craftTime);
                text = TimeFormatter.SecondToFractionalTimer(currentCraftingTime, craftTime);
            }
            else {
                var targetTime = craftItem.GetCraftTimeWithBonus();
                text = TimeFormatter.SecondsToMinuteTimer(targetTime);
            }

            var bonus = craftItem.GetCraftingTimeBonusPercent() * 100f;
            var absBonus = Mathf.Abs(bonus);
            var bonusColorHex = ColorUtility.ToHtmlStringRGB(bonus > 0 ? positiveBonusColor : negativeBonusColor);
            var bonusText = bonus > 0f ? $" <color=#{bonusColorHex}>(-{absBonus:F0}%)</color>" : bonus < 0f ? $" <color=#{bonusColorHex}>(+{absBonus:F0}%)</color>" : "";

            text += bonusText;
        }
        else if (craftItemDefinition != null) {
            text = TimeFormatter.SecondsToTimer(craftItemDefinition.ProduceTime);
        }

        timer.SetText(text);
    }

    private void UpdateProgressBar()
    {
        if (progressBar == null) return;
        if (craftItem == null) return;

        var craftTime = craftItem.GetCraftTimeWithBonus();
        var currentCraftingTime = craftItem.CurrentCraftingTime;

        var amount = 0f;
        if (isSelected && craftTime > 0) {
            amount = Mathf.Clamp01((float)currentCraftingTime / craftTime);

            if (flickingProgressBar != null) {
                var isFinished = currentCraftingTime >= craftTime;
                flickingProgressBar.SetFlickingEnabled(isFinished);
            }
        }

        progressBar.fillAmount = amount;
    }

    private void OnCraftingSpeedBonusChanged()
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