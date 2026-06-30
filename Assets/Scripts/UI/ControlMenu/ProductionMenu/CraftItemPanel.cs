using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftItemPanel : MonoBehaviour
{
    [SerializeField] private ResourceWidget consumeResourceWidgetPrefab;
    [SerializeField] private ResourceWidget craftResourceWidgetPrefab;
    [SerializeField] private CustomButton button;
    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private Image progressBar;
    [SerializeField] private Transform producedResourceSlot;
    [SerializeField] private LayoutGroup consumedResourcesSlot;
    [SerializeField] private Color bonusColor = Color.HSVToRGB(120, 60, 100);
    private FlickingImage flickingProgressBar;

    private CraftingModule craftingModule;
    private CraftItemInstance craftItem;

    private bool isSelected = false;

    private void Awake()
    {
        flickingProgressBar = progressBar.GetComponent<FlickingImage>();
    }

    private void OnEnable()
    {
        button.OnSelected.AddListener(OnSelected);
        button.OnDeselected.AddListener(OnDeselected);
    }

    private void OnDisable()
    {
        button.OnSelected.RemoveListener(OnSelected);
        button.OnDeselected.RemoveListener(OnDeselected);
    }

    private void OnDestroy()
    {
        if (craftItem == null) return;

        craftItem.OnCraftingSpeedBonusChanged -= OnCraftingSpeedBonusChanged;
    }

    private void Update()
    {
        if (!isSelected) return;
        if (!craftingModule.IsWorking) return;

        UpdateTimer();
        UpdateProgressBar();
    }

    public void Init(CraftingModule craftingModule, CraftItemInstance craftItem, SelectGroup selectGroup)
    {
        if (!craftingModule) {
            Debug.LogError("craftingModule is not valid");
            return;
        }

        if (craftItem == null) {
            Debug.LogError("craftItem is not valid");
            return;
        }

        this.craftingModule = craftingModule;
        this.craftItem = craftItem;
        button.SetSelectGroup(selectGroup);

        UpdateSelected();

        CreateProducedResourceWidget();
        CreateConsumedResourcesWidget();

        UpdateTimer();
        UpdateProgressBar();

        craftItem.OnCraftingSpeedBonusChanged += OnCraftingSpeedBonusChanged;
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
        if (craftItem == null) {
            Debug.LogError("CraftItem is not valid");
            return;
        }

        if (!craftingModule) {
            Debug.LogError("CraftingModule is not valid");
            return;
        }
        
        if (craftItem == craftingModule.SelectedCraftItem) {
            Select();
        }
        else {
            Deselect();
        }
    }

    private void CreateProducedResourceWidget()
    {
        var widget = Instantiate(craftResourceWidgetPrefab, producedResourceSlot.transform);
        widget.SetItem(craftItem.Definition.ProduceItem.Definition);
        widget.AddAmount(craftItem.Definition.ProduceItem);
    }

    private void CreateConsumedResourcesWidget()
    {
        foreach (var resource in craftItem.Definition.ConsumeResources) {
            var widget = Instantiate(consumeResourceWidgetPrefab, consumedResourcesSlot.transform);
            var definition = resource.Definition;

            widget.SetItem(definition);
            widget.AddAmount(resource);
            widget.SetLimit(CityStorage.Instance.Inventory.GetItemById(definition.ItemId));
        }
    }

    private void UpdateTimer()
    {
        var text = "";

        if (isSelected) {
            var craftTime = craftItem.GetCraftTime();
            var currentCraftingTime = craftItem.GetCurrentCraftingTime();

            if (currentCraftingTime == null)
                currentCraftingTime = 0;

            currentCraftingTime = Mathf.Clamp(currentCraftingTime.Value, 0, craftTime);

            text = TimeFormatter.SecondToTimer(currentCraftingTime.Value, craftTime);
        }
        else {
            var targetTime = craftItem.GetCraftTime();
            text = TimeFormatter.SecondsToMinuteTime(targetTime);
        }

        var bonusColorHex = ColorUtility.ToHtmlStringRGB(bonusColor);
        var bonus = craftItem.CraftingSpeedBonus * 100;
        var bonusText = bonus > 0f ? $" <color=#{bonusColorHex}>(-{bonus}%)</color>" : "";
        text += bonusText;

        timer.SetText(text);
    }

    private void UpdateProgressBar()
    {
        var craftTime = craftItem.GetCraftTime();
        var currentCraftingTime = craftItem.GetCurrentCraftingTime();

        var amount = 0f;

        if (isSelected && currentCraftingTime != null && craftTime > 0) {
            amount = currentCraftingTime.Value / craftTime;

            if (currentCraftingTime >= craftTime)
                flickingProgressBar.SetFlickingEnabled(true);
            else
                flickingProgressBar.SetFlickingEnabled(false);
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
            craftingModule.TryCollectItem();
            craftingModule.TryRefundResources();

            if (craftingModule.IsWorking) {
                craftingModule.ResetCraftingFinishTime();
            }
            else {
                craftingModule.RemoveCraftingFinishTime();
            }

            craftingModule.SetCraftingItem(craftItem);
        }

        UpdateTimer();
        UpdateProgressBar();
    }

    private void OnDeselected()
    {
        isSelected = false;

        if (craftingModule.SelectedCraftItem == craftItem) {
            craftingModule.RemoveCraftignItem();
        }

        UpdateTimer();
        UpdateProgressBar();
    }
}