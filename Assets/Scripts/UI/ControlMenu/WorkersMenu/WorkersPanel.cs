using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WorkersPanel : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;

    [SerializeField] private GridLayoutGroup layoutGroup;
    public GridLayoutGroup LayoutGroup => layoutGroup;

    [SerializeField] private GameObject haveNoCitizensText;

    private Vector2? startSize = null;
    private List<CitizenWidget> spawnedWidgets = new();

    private void Awake()
    {
        startSize = rectTransform.sizeDelta;
    }

    private void Start()
    {
        UpdateMenu();
    }

    public void UpdateMenu()
    {
        if (startSize == null) return;

        if (layoutGroup.transform.childCount == 0) {
            rectTransform.sizeDelta = startSize.Value;
        }
        else {
            layoutGroup.GetComponent<RectTransform>().ForceUpdateRectTransforms();
            float ySize = startSize.Value.y + (LayoutGroupUtils.GetRowsCount(layoutGroup) * layoutGroup.cellSize.y);
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, ySize);
        }

        if (haveNoCitizensText) {
            haveNoCitizensText.SetActive(layoutGroup.transform.childCount == 0);
        }
    }

    public void SortWidgets()
    {
        var selectedBuilding = SelectManager.Instance.SelectedComponent?.GetComponent<Building>();
        if (!selectedBuilding) {
            Debug.LogError($"[{nameof(WorkersPanel)}] Selected Building is not valid!");
            return;
        }

        var sortedWidgets = spawnedWidgets.Where(w => w).OrderByDescending(w => w.Citizen?.SkillsComponent?.GetSkill(selectedBuilding.SkillId)?.CurrentLevel ?? 0).ToList();
        for (int i = 0; i < sortedWidgets.Count; i++) {
            sortedWidgets[i].transform.SetSiblingIndex(i);
        }
    }

    public void AddWidget(CitizenWidget widget)
    {
        spawnedWidgets.Add(widget);
    }

    public void RemoveWidget(CitizenWidget widget)
    {
        spawnedWidgets.Remove(widget);
    }
}