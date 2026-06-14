using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CraftingControlMenu : ControlMenu
{
    [SerializeField] private CraftItemPanel producedResourcePanelPrefab;
    private CraftItemPanel[] spawnedProducedResourcePanels;

    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private SelectGroup selectGroup;
    [SerializeField] private FitSizeToChildren fitSizeToChildren;

    protected override void OnOpen()
    {

    }

    protected override void OnClose()
    {

    }

    protected override void UpdateMenu()
    {
        ClearPanels();
        CreatePanels();
        FitLayoutGroupSize();
    }

    private void CreatePanels()
    {
        var module = SelectManager.Instance.GetSelectedBuilding().GetComponent<CraftingModule>();
        var crafts = module.ProductionLevelData.CraftItems;
        int length = crafts.Length;
        spawnedProducedResourcePanels = new CraftItemPanel[length];

        for (int i = 0; i < length; i++) {
            var craft = crafts[i];

            var spawned = Instantiate(producedResourcePanelPrefab, layoutGroup.transform);
            spawned.Init(module, craft, i, selectGroup);

            spawnedProducedResourcePanels[i] = spawned;

            if (i != module.CurrentProductingItemIndex) continue;

            spawned.Select();
        }
    }

    private void ClearPanels()
    {
        if (spawnedProducedResourcePanels == null) return;

        foreach (var panel in spawnedProducedResourcePanels) {
            Destroy(panel.gameObject);
        }

        spawnedProducedResourcePanels = null;
    }

    private void FitLayoutGroupSize()
    {
        StartCoroutine(FitLayoutGroupSizeCoroutine());
    }

    private IEnumerator FitLayoutGroupSizeCoroutine()
    {
        yield return new WaitForEndOfFrame();

        scrollRect.verticalNormalizedPosition = 1f;
        fitSizeToChildren.UpdateSize();
    }
}