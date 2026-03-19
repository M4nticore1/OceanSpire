using UnityEngine;
<<<<<<< Updated upstream
=======
using UnityEngine.UI;
>>>>>>> Stashed changes

public class ProductionResourcePanel : MonoBehaviour
{
    [SerializeField] private ResourceWidget resourceWidgetPrefab;
    [SerializeField] private Transform producedResourceSlot;
<<<<<<< Updated upstream
    [SerializeField] private GridLayout consumedResourcesSlot;
=======
    [SerializeField] private LayoutGroup consumedResourcesSlot;
>>>>>>> Stashed changes

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
