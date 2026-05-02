using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorkersPanel : UIBehaviour
{
    [SerializeField] private CitizenWidget citizenWidgetPrefab;

    private RectTransform rectTransform;
    private List<CitizenWidget> spawnedWidgets = new List<CitizenWidget>();
    public List<CitizenWidget> SpawnedWidgets => spawnedWidgets;

    [SerializeField] private GridLayoutGroup layoutGroup;
    [SerializeField] private GameObject haveNoCitizensText;

    private Vector2 startSize;

    protected override void Awake()
    {
        base.Awake();

        rectTransform = GetComponent<RectTransform>();
        startSize = rectTransform.sizeDelta;
    }

    public void CreateWidget(Human citizen)
    {
        CitizenWidget widget = CitizenWidgetFactory.CreateWidget(citizenWidgetPrefab, layoutGroup.transform, citizen);
        spawnedWidgets.Add(widget);
    }

    public void ClearWidgets()
    {
        for (int i = spawnedWidgets.Count - 1; i >= 0; i--) {
            Destroy(spawnedWidgets[i].gameObject);
            spawnedWidgets[i].gameObject.transform.SetParent(null);
            spawnedWidgets.RemoveAt(i);
        }
    }

    public void UpdateMenu()
    {
        int widgetsCount = spawnedWidgets.Count;

        if (widgetsCount == 0) {
            rectTransform.sizeDelta = startSize;
        }
        else {
            layoutGroup.GetComponent<RectTransform>().ForceUpdateRectTransforms();
            float ySize = startSize.y + (LayoutGroupUtils.GetRowsCount(layoutGroup) * layoutGroup.cellSize.y);
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, ySize);
        }

        if (haveNoCitizensText) {
            haveNoCitizensText.SetActive(widgetsCount == 0);
        }
    }
}