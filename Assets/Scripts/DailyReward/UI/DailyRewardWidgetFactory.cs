using UnityEngine;

public static class DailyRewardWidgetFactory
{
    public static DailyRewardWidget CreateWidget(DailyRewardWidget prefab, Transform transform, ItemAdRewardInstance reward)
    {
        var widget = GameObject.Instantiate(prefab, transform);
        widget.Init(reward);

        return widget;
    }
}
