using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WorkersPanel : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private FitSizeToChildren fitSizeToChildren;

    [SerializeField] private GridLayoutGroup layoutGroup;
    public GridLayoutGroup LayoutGroup => layoutGroup;

    [SerializeField] private GameObject haveNoCitizensText;

    private List<CitizenWidget> spawnedWidgets = new();

    private void Start()
    {
        UpdateMenu();
    }

    public void UpdateMenu()
    {
        if (!gameObject.activeSelf) return;
        if (!gameObject.activeInHierarchy) return;

        //if (layoutGroup.transform.childCount == 0) {
        //    rectTransform.sizeDelta = startSize.Value;
        //}
        //else {
        //    fitSizeToChildren.UpdateSize();
        //}

        if (haveNoCitizensText) {
            haveNoCitizensText.SetActive(layoutGroup.transform.childCount == 0);
        }
    }

    public void SortWidgets(Building building)
    {
        if (building == null) return;

        if (!gameObject.activeSelf) return;
        if (!gameObject.activeInHierarchy) return;

        var sortedWidgets = spawnedWidgets.Where(w => w && w.Citizen).OrderByDescending(w => w.Citizen.SkillsComponent?.GetSkill(building.SkillId)?.CurrentLevel ?? 0).ToList();
        for (int i = 0; i < sortedWidgets.Count; i++) {
            sortedWidgets[i].transform.SetSiblingIndex(i);
        }
    }

    public void UpdateSize()
    {
        fitSizeToChildren.UpdateSize();
    }

    public void AddWidget(CitizenWidget widget)
    {
        if (widget == null) return;
        if (spawnedWidgets.Contains(widget)) return;

        spawnedWidgets.Add(widget);
        fitSizeToChildren.AddIncludedTransform(widget.gameObject);
        fitSizeToChildren.UpdateSizeDelay();
    }

    public void RemoveWidget(CitizenWidget widget)
    {
        spawnedWidgets.Remove(widget);
        fitSizeToChildren.RemoveIncludedTransform(widget.gameObject);
        fitSizeToChildren.UpdateSizeDelay();
    }
}