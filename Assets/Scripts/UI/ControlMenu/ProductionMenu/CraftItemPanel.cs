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
        button.OnReleased.AddListener(OnClicked);
        button.OnDeselected.AddListener(OnDeselected);
    }

    private void OnDisable()
    {
        button.OnReleased.RemoveListener(OnClicked);
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

        CreateProducedResourceWidget();
        CreateConsumedResourcesWidget();

        UpdateTimer();
        UpdateProgressBar();

        craftItem.OnCraftingSpeedBonusChanged += OnCraftingSpeedBonusChanged;
    }

    public void Select()
    {
        isSelected = true;

        button.SetState(CustomButtonState.Selected);
        button.EndTransitionAnimation();
        UpdateTimer();
        UpdateProgressBar();
    }

    private void Deselect()
    {
        isSelected = false;
        UpdateTimer();
        UpdateProgressBar();
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
            widget.SetItem(resource.Definition);
            widget.AddAmount(resource);
        }
    }

    private void UpdateTimer()
    {
        string text;

        if (craftItem.IsCrafting) {
            int currentTime = (int)craftItem.CurrentCraftingTime;
            int targetTime = (int)craftItem.GetProduceTime();
            text = TimeFormatter.SecondToTimer(currentTime, targetTime);
        }
        else {
            int targetTime = (int)craftItem.GetProduceTime();
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
        float currentTime = craftItem.CurrentCraftingTime;
        float targetTime = craftItem.GetProduceTime();
        float amount = 0f;

        if (targetTime > 0 && isSelected) {
            amount = currentTime / targetTime;

            if (currentTime >= targetTime)
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

    private void OnClicked()
    {
        craftingModule.TryCollectItem();
        craftingModule.TryRefundResources();
        craftingModule.ResetProducedTime();
        craftingModule.SetCraftingItem(craftItem);
        craftingModule.TryConsumeResources();

        Select();
    }

    private void OnDeselected()
    {
        Deselect();
    }
}