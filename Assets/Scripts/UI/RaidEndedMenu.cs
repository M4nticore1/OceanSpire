using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RaidEndedMenu : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private ResourceWidget resourceWidgetPrefab;
    [SerializeField] private RaidManager raidManager;

    [Header("UI")]
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
        raidManager.OnRaidEnded += OnRaidEnded;
    }

    private void OnDisable()
    {
        raidManager.OnRaidEnded -= OnRaidEnded;
    }

    private void Update()
    {
        if (isOpened) {
            currentVisibilityTime += Time.deltaTime;

            if (currentVisibilityTime >= visibilityTime) {
                Close();
            }
        }
    }

    private void OnRaidEnded(RaidEndedResult result)
    {
        Open();
        RemoveLosses();
        CreateLosses();
    }

    private void Open()
    {
        isOpened = true;
        slidePanel.Show();
    }

    private void Close()
    {
        isOpened = false;
        slidePanel.Hide();
        currentVisibilityTime = 0f;
    }

    private void CreateLosses()
    {
        int lossesCount = raidManager.Inventory.Items.Count;

        if (lossesCount == 0) {
            noLossesText.gameObject.SetActive(true);
        }
        else {
            noLossesText.gameObject.SetActive(false);

            for (int i = 0; i < lossesCount; i++) {
                var widget = Instantiate(resourceWidgetPrefab, layoutGroup.transform);

                var item = raidManager.Inventory.TryGetItemByIndex(i);
                widget.SetItem(item);
                widget.AddAmount(item);
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