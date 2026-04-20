using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorkersPanel : UIBehaviour
{
    [SerializeField] private CitizenWidget citizenWidgetPrefab;

    private RectTransform rectTransform;
    private List<CitizenWidget> spawnedCitizenWidgets = new List<CitizenWidget>();
    public List<CitizenWidget> SpawnedCitizenWidgets => spawnedCitizenWidgets;

    [SerializeField] private GridLayoutGroup citizensLayoutGroup;
    [SerializeField] private GameObject haveNoCitizensText;

    private Vector2 startSize;

    public void Init()
    {
        rectTransform = GetComponent<RectTransform>();
        startSize = rectTransform.sizeDelta;
    }

    public void CreateWidget(Human citizen)
    {
        CitizenWidget widget = Instantiate(citizenWidgetPrefab, citizensLayoutGroup.transform);
        spawnedCitizenWidgets.Add(widget);
        widget.Init(citizen);

        UpdateMenuSize();
    }

    public void ClearWidgets()
    {
        foreach (var widget in spawnedCitizenWidgets) {
            Destroy(widget.gameObject);
        }

        spawnedCitizenWidgets.Clear();
    }

    private void UpdateMenuSize()
    {
        int widgetsCount = spawnedCitizenWidgets.Count;

        if (widgetsCount == 0) {
            rectTransform.sizeDelta = startSize;

            if (haveNoCitizensText)
                haveNoCitizensText.SetActive(true);
        }
        else {
            float ySize = startSize.y + (LayoutGroupUtils.GetRowsCount(citizensLayoutGroup) * citizensLayoutGroup.cellSize.y);
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, ySize);

            if (haveNoCitizensText)
                haveNoCitizensText.SetActive(false);
        }
    }
}