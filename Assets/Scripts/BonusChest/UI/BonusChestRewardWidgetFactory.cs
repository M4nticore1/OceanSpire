using UnityEngine;

public static class BonusChestRewardWidgetFactory
{
    public static BonusChestRewardWidget CreateWidget(BonusChestRewardWidget prefab, Transform transform, ItemAdRewardInstance reward)
    {
        var widget = GameObject.Instantiate(prefab, transform);
        widget.Init(reward);

        return widget;
    }
}
