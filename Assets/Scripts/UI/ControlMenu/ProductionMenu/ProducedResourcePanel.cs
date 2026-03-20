using UnityEngine;
using UnityEngine.UI;

public class ProducedResourcePanel : MonoBehaviour
{
    [SerializeField] private ResourceWidget resourceWidgetPrefab;
    [SerializeField] private CustomButton button;
    [SerializeField] private Transform producedResourceSlot;
    [SerializeField] private LayoutGroup consumedResourcesSlot;
    private ProductionModule productionModule;

    private ItemInstance producedResource;
    private ItemInstance[] consumedResources;

    private void OnEnable()
    {
        button.onReleased += OnClicked;
    }

    private void OnDisable()
    {
        button.onReleased -= OnClicked;
    }

    public void Init(ProductionModule productionModule, ProduceResource produceResource)
    {
        this.productionModule = productionModule;
        producedResource = produceResource.produceItem;
        consumedResources = produceResource.consumeResources;

        CreateProducedResource();
        CreateConsumedResources();
    }

    private void CreateProducedResource()
    {
        ResourceWidget widget = Instantiate(resourceWidgetPrefab, producedResourceSlot.transform);
        widget.Init(producedResource);
    }

    private void CreateConsumedResources()
    {
        foreach (var resource in consumedResources) {
            ResourceWidget widget = Instantiate(resourceWidgetPrefab, consumedResourcesSlot.transform);
            widget.Init(resource);
        }
    }

    private void OnClicked()
    {
        //productionModule.set
    }
}
