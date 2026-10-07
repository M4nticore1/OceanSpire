using UnityEngine;

public static class DailyTaskWidgetFactory
{
    public static DailyTaskWidget CreateWidget(DailyTaskWidget prefab, Transform transform, DailyTaskInstance task)
    {
        if (prefab == null) {
            Debug.LogError("Prefab is not valid!");
            return null;
        }
        if (transform == null) {
            Debug.LogError("Transform is not valid!");
            return null;
        }
        if (task == null) {
            Debug.LogError("Task is not valid!");
            return null;
        }

        DailyTaskWidget widget = GameObject.Instantiate(prefab, transform);
        widget.Init(task);

        return widget;
    }
}
