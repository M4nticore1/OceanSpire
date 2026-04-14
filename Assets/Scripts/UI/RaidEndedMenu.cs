using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RaidEndedMenu : MonoBehaviour
{
    [SerializeField] private ResourceWidget resourceWidgetPrefab;

    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private TextMeshProUGUI noLossesText;
    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private Color loseColor;

    [SerializeField] private float visibilityTime = 0f;
    private float currentVisibilityTime = 0f;

    private bool isOpened = false;
    private List<ResourceWidget> spawnedResourceWidgets = new();

    private void OnEnable()
    {
        RaidManager.onRaidEnded += OnRaidFinished;
    }

    private void OnDisable()
    {
        RaidManager.onRaidEnded -= OnRaidFinished;
    }

    private void Update()
    {
        if (!isOpened) return;

        currentVisibilityTime += Time.deltaTime;
        if (currentVisibilityTime < visibilityTime) return;

        Close();
    }

    private void OnRaidFinished()
    {
        Open();
        currentVisibilityTime = 0f;
        RemoveLosses();
        CreateLosses();
    }

    private void Open()
    {
        slidePanel.Open();
        isOpened = true;
    }

    private void Close()
    {
        slidePanel.Close();
        isOpened = false;
    }

    private void CreateLosses()
    {
        int lossesAmount = RaidManager.instance.Inventory.items.Count;

        if (lossesAmount == 0) {
            noLossesText.gameObject.SetActive(true);
        }
        else {
            noLossesText.gameObject.SetActive(false);

            foreach (var lose in RaidManager.instance.Inventory.items) {
                ResourceWidget widget = ResourceWidgetFactory.CreateResourceWidget(resourceWidgetPrefab, layoutGroup.transform);
                widget.SetAmount(-lose.item.Amount);
                widget.SetColor(loseColor);
                spawnedResourceWidgets.Add(widget);
            }
        }
    }

    private void RemoveLosses()
    {
        for (int i = spawnedResourceWidgets.Count - 1; i >= 0; i--) {
            Destroy(spawnedResourceWidgets[i].gameObject);
            spawnedResourceWidgets.RemoveAt(i);
        }
    }
}