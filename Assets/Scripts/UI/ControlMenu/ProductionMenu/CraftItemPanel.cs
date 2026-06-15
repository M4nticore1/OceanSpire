using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftItemPanel : MonoBehaviour
{
    [SerializeField] private ResourceWidget resourceWidgetPrefab;
    [SerializeField] private CustomButton button;
    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private Image progressBar;
    [SerializeField] private Transform producedResourceSlot;
    [SerializeField] private LayoutGroup consumedResourcesSlot;
    private FlickingImage flickingProgressBar;
    private CraftingModule craftingModule;

    private CraftItemDefinition currentCraftItem;
    private int index = 0;
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

    private void Start()
    {
        UpdateTimer();
        UpdateProgressBar();
    }

    private void Update()
    {
        if (!isSelected) return; 

        UpdateTimer();
        UpdateProgressBar();
    }

    public void Init(CraftingModule craftingModule, CraftItemDefinition craftItem, int index, SelectGroup selectGroup)
    {
        this.craftingModule = craftingModule;
        currentCraftItem = craftItem;
        this.index = index;
        button.SetSelectGroup(selectGroup);

        CreateProducedResourceWidget();
        CreateConsumedResourcesWidget();
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
        var widget = Instantiate(resourceWidgetPrefab, producedResourceSlot.transform);
        widget.SetItem(currentCraftItem.ProduceItem.Definition);
        widget.AddAmount(currentCraftItem.ProduceItem);
    }

    private void CreateConsumedResourcesWidget()
    {
        foreach (var resource in currentCraftItem.ConsumeResources) {
            var widget = Instantiate(resourceWidgetPrefab, consumedResourcesSlot.transform);
            widget.SetItem(resource.Definition);
            widget.AddAmount(resource);
        }
    }

    private void UpdateTimer()
    {
        if (!craftingModule) {
            Debug.Log($"CraftingModule not found at {name}");
            return;
        }

        string text;

        if (isSelected && (craftingModule.IsWorking || craftingModule.IsReadyToCollect)) {
            int currentTime = (int)craftingModule.CurrentCraftItem.CurrentCraftingTime;
            int targetTime = craftingModule.ProductionLevelData.TryGetCraftItem(index).ProduceTime;
            text = TimeFormatter.SecondToTimer(currentTime, targetTime);
        }
        else {
            int targetTime = craftingModule.ProductionLevelData.TryGetCraftItem(index).ProduceTime;
            text = TimeFormatter.SecondsToMinuteTime(targetTime);
        }

        timer.SetText(text);
    }

    private void UpdateProgressBar()
    {
        if (!craftingModule) {
            Debug.Log($"CraftingModule is not valid at {name}");
            return;
        }

        var craftItem = craftingModule.ProductionLevelData.TryGetCraftItem(index);
        if (!craftItem) {
            Debug.Log($"CraftItem is not valid in {craftingModule.ProductionLevelData} by index {index}");
            return;
        }

        float currentTime = craftingModule.CurrentCraftItem.CurrentCraftingTime;
        int targetTime = craftItem.ProduceTime;
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

    private void OnClicked()
    {
        craftingModule.SetCraftingItemByIndex(index);
        Select();
    }

    private void OnDeselected()
    {
        Deselect();
    }
}