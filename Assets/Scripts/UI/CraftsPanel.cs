using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftsPanel : MonoBehaviour
{
    [SerializeField] private CraftItemPanel CraftItemWidget;
    [SerializeField] private LayoutGroup layoutGroup;

    private List<CraftItemPanel> spawnedWidgets = new();

    public void SetCraftsAndApply(List<CraftItemDefinition> crafts)
    {
        DestroyWidgets();

        if (crafts == null) {
            Debug.LogError($"[{nameof(CraftsPanel)}] Crafts is not valid!");
            return;
        }

        if (CraftItemWidget == null) {
            Debug.LogError($"[{nameof(CraftsPanel)}] Craft Item Widget is not valid!");
            return;
        }

        if (layoutGroup == null) {
            Debug.LogError($"[{nameof(CraftsPanel)}] Layout Group is not valid!");
            return;
        }

        CreateWidgets(crafts);
    }

    private void CreateWidgets(List<CraftItemDefinition> crafts)
    {

    }

    private void DestroyWidgets()
    {

    }
}