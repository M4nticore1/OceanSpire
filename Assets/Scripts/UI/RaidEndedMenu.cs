using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RaidEndedMenu : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private ItemWidget resourceWidgetPrefab;
    [SerializeField] private RaidManager raidManager;

    [Header("UI")]
    [SerializeField] private SlideAnimatedPanel slidePanel;
    [SerializeField] private FitSizeToContent fitSize;
    [SerializeField] private TextMeshProUGUI noLossesText;
    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private Color loseColor;

    [SerializeField] private float visibilityTime = 0f;
    private float currentVisibilityTime = 0f;

    private bool isShown => slidePanel.IsShown;
    private List<ItemWidget> spawnedResourceWidgets = new();

    private void OnEnable()
    {
        raidManager.OnRaidEnded += HandleRaidEnded;
    }

    private void OnDisable()
    {
        raidManager.OnRaidEnded -= HandleRaidEnded;
    }

    private void Update()
    {
        if (isShown) {
            currentVisibilityTime += Time.deltaTime;

            if (currentVisibilityTime >= visibilityTime) {
                Close();
            }
        }
    }

    private void HandleRaidEnded(RaidEndedResult result)
    {
        Hide();
        RemoveLosses();
        UpdateLossesPanel(result.Losses);
        fitSize.UpdateSize();
    }

    private void Hide()
    {
        slidePanel.Show();
    }

    private void Close()
    {
        slidePanel.Hide();
        currentVisibilityTime = 0f;
    }

    private void UpdateLossesPanel(List<ItemInstance> items)
    {
        if (items == null || items.Count == 0) {
            noLossesText.gameObject.SetActive(true);
        }
        else {
            noLossesText.gameObject.SetActive(false);

            for (int i = 0; i < items.Count; i++) {
                var item = items[i];
                if (item == null) continue;
                if (item.Amount <= 0) continue;

                var widget = Instantiate(resourceWidgetPrefab, layoutGroup.transform);
                widget.SetItemInstance(item);
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