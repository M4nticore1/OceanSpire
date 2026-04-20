using UnityEngine;
using UnityEngine.UI;

public static class LayoutGroupUtils
{
    public static int GetColumnsCount(GridLayoutGroup layoutGroup)
    {
        var rect = layoutGroup.GetComponent<RectTransform>();

        float width = rect.rect.width;

        float avaliableWidth = width - layoutGroup.padding.left - layoutGroup.padding.right;
        float cellFullWidth = layoutGroup.cellSize.x + layoutGroup.spacing.x;

        int columns = Mathf.FloorToInt((avaliableWidth + layoutGroup.spacing.x) / cellFullWidth);
        columns = Mathf.Max(1, columns);

        return columns;
    }

    public static int GetRowsCount(GridLayoutGroup layoutGroup)
    {
        int childCount = layoutGroup.transform.childCount;
        int rows = Mathf.CeilToInt((float)childCount / GetColumnsCount(layoutGroup));

        return rows;
    }
}