using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProducedResourcePanel : MonoBehaviour
{
    [SerializeField] private ResourceWidget resourceWidgetPrefab;
    [SerializeField] private CustomButton button;
    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private Image progressBar;
    [SerializeField] private Transform producedResourceSlot;
    [SerializeField] private LayoutGroup consumedResourcesSlot;
    private FlickingImage flickingProgressBar;
    private ProductionModule productionModule;

    private CraftItem currentCraftItem;
    private int index = 0;
    private bool isSelected = false;

    private void Awake()
    {
        flickingProgressBar = progressBar.GetComponent<FlickingImage>();
    }

    private void OnEnable()
    {
        button.onReleased += OnClicked;
        button.onDeselected += OnDeselected;
    }

    private void OnDisable()
    {
        button.onReleased -= OnClicked;
        button.onDeselected -= OnDeselected;
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

    public void Init(ProductionModule productionModule, CraftItem craftItem, int index)
    {
        this.productionModule = productionModule;
        this.currentCraftItem = craftItem;
        this.index = index;

        CreateProducedResource();
        CreateConsumedResources();
    }

    public void Select()
    {
        isSelected = true;

        button.SetState(CustomButtonState.Selected);
        button.FinishTransitionAnimation();
        UpdateTimer();
        UpdateProgressBar();
    }

    private void Deselect()
    {
        isSelected = false;
        UpdateTimer();
        UpdateProgressBar();
    }

    private void CreateProducedResource()
    {
        ResourceWidget widget = Instantiate(resourceWidgetPrefab, producedResourceSlot.transform);
        widget.SetAmountItem(currentCraftItem.ProduceItem);
    }

    private void CreateConsumedResources()
    {
        foreach (var resource in currentCraftItem.ConsumeResources) {
            ResourceWidget widget = Instantiate(resourceWidgetPrefab, consumedResourcesSlot.transform);
            widget.SetAmountItem(resource);
        }
    }

    private void UpdateTimer()
    {
        string text;

        if (isSelected && (productionModule.IsWorking || productionModule.IsReadyToCollect)) {
            int currentTime = (int)productionModule.CurrentProductionTime;
            int targetTime = productionModule.ProductionLevelData.craftItems[index].ProduceTime;
            text = TimeFormatter.SecondToTimer(currentTime, targetTime);
        }
        else {
            int targetTime = productionModule.ProductionLevelData.craftItems[index].ProduceTime;
            text = TimeFormatter.SecondsToMinuteTime(targetTime);
        }

        timer.SetText(text);
    }

    private void UpdateProgressBar()
    {
        float currentTime = productionModule.CurrentProductionTime;
        int targetTime = productionModule.ProductionLevelData.craftItems[index].ProduceTime;
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
        productionModule.SetProducedItemIndex(index);
        Select();
    }

    private void OnDeselected()
    {
        Deselect();
    }
}