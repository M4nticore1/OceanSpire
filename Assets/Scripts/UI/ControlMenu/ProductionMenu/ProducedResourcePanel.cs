using UnityEngine;
using UnityEngine.UI;

public class ProducedResourcePanel : MonoBehaviour
{
    [SerializeField] private ResourceWidget resourceWidgetPrefab;
    [SerializeField] private Transform producedResourceSlot;
    [SerializeField] private LayoutGroup consumedResourcesSlot;

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
}
