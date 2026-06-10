using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RaidEndedMenu : UIBehaviour
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

    private bool isSubscribed = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        TrySubscribe();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        TryUnsubscribe();
    }

    protected override void Start()
    {
        base.Start();

        TrySubscribe();
    }

    private void Update()
    {
        if (!isOpened) return;
        if (!RaidManager.Instance) return;

        currentVisibilityTime += Time.deltaTime;
        if (currentVisibilityTime < visibilityTime) return;

        Close();
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;
        if (!RaidManager.Instance) return;

        RaidManager.Instance.OnRaidEnded += OnRaidEnded;

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;

        RaidManager.Instance.OnRaidEnded -= OnRaidEnded;

        isSubscribed = false;
    }

    private void OnRaidEnded(RaidEndedResult result)
    {
        Open();
        RemoveLosses();
        CreateLosses();
        currentVisibilityTime = 0f;
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
        int lossesCount = RaidManager.Instance.Inventory.Items.Count;

        if (lossesCount == 0) {
            noLossesText.gameObject.SetActive(true);
        }
        else {
            noLossesText.gameObject.SetActive(false);

            for (int i = 0; i < lossesCount; i++) {
                var widget = Instantiate(resourceWidgetPrefab, layoutGroup.transform);

                var item = RaidManager.Instance.Inventory.GetItemByIndex(i);
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