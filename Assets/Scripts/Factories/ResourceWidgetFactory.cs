using UnityEngine;

public static class ResourceWidgetFactory
{
    public static ResourceWidget CreateResourceWidget(ResourceWidget prefab, Transform transform, ItemInstance amountItem, ItemInstance maxAmountItem)
    {
        var widget = GameObject.Instantiate(prefab, transform);
        widget.Init(amountItem, maxAmountItem);

        return widget;
    }

    public static ResourceWidget CreateResourceWidget(ResourceWidget prefab, Transform transform, ItemInstance amountItem)
    {
        var widget = GameObject.Instantiate(prefab, transform);
        widget.Init(amountItem);

        return widget;
    }

    public static ResourceWidget CreateResourceWidget(ResourceWidget prefab, Transform transform)
    {
        var widget = GameObject.Instantiate(prefab, transform);

        return widget;
    }
}
