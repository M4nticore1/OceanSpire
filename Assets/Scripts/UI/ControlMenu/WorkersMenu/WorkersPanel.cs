using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorkersPanel : UIBehaviour
{
    [SerializeField] private CitizenWidget citizenWidgetPrefab = null;

    private RectTransform rectTransform = null;
    private List<CitizenWidget> spawnedCitizenWidgets = new List<CitizenWidget>();
    public List<CitizenWidget> SpawnedCitizenWidgets => spawnedCitizenWidgets;

    [SerializeField] private GridLayoutGroup citizensLayoutGroup = null;
    [SerializeField] private GameObject haveNoCitizensText = null;

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
            int rowsCount = Mathf.CeilToInt((float)widgetsCount / citizensLayoutGroup.constraintCount);
            float ySize = startSize.y + (rowsCount * citizensLayoutGroup.cellSize.y);
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, ySize);

            if (haveNoCitizensText)
                haveNoCitizensText.SetActive(false);
        }
    }
}
