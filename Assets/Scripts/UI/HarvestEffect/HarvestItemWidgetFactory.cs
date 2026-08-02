using UnityEngine;

public static class HarvestItemWidgetFactory
{
    public static HarvestEffectWidget CreateWidget(
        HarvestEffectWidget widgetPrefab,
        Transform parentCanvas,
        ItemInstance item,
        Vector3 startWorldPos,
        Vector3 targetWorldPos)
    {
        var widget = GameObject.Instantiate(widgetPrefab, parentCanvas);

        widget.Init(item, startWorldPos, targetWorldPos);

        return widget;
    }
}