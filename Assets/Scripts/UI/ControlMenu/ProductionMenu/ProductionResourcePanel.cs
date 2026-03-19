using UnityEngine;

public class ProductionResourcePanel : MonoBehaviour
{
    [SerializeField] private ResourceWidget resourceWidgetPrefab;
    [SerializeField] private Transform producedResourceSlot;

    private ItemInstance producedResource;
    private ItemInstance[] consumedResources;

    public void Init(ProduceResource produceResource)
    {
        producedResource = produceResource.produceItem;
        consumedResources = produceResource.consumeResources;

        CreateProducedResource();
        CreateConsumedResources();
    }

    private void CreateProducedResource()
    {
        ResourceWidget widget = Instantiate(resourceWidgetPrefab);
        widget.Init(producedResource);
    }

    private void CreateConsumedResources()
    {
        foreach (var resource in consumedResources) {
            ResourceWidget widget = Instantiate(resourceWidgetPrefab);
            widget.Init(resource);
        }
    }
}
