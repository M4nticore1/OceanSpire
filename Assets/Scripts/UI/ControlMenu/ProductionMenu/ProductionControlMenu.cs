using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ProductionControlMenu : ControlMenu
{
    [SerializeField] private ProducedResourcePanel producedResourcePanelPrefab;
    private ProducedResourcePanel[] spawnedProducedResourcePanels;

    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private ScrollRect scrollRect;
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
        ProductionModule module = SelectManager.Instance.GetSelectedBuilding().GetComponent<ProductionModule>();
        CraftItem[] crafts = module.ProductionLevelData.craftItems;
        int length = crafts.Length;
        spawnedProducedResourcePanels = new ProducedResourcePanel[length];

        for (int i = 0; i < length; i++) {
            CraftItem craft = crafts[i];
            ProducedResourcePanel spawned = Instantiate(producedResourcePanelPrefab, layoutGroup.transform);
            spawned.Init(module, craft, i);
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
        fitSizeToChildren.FitToChildren();
    }
}