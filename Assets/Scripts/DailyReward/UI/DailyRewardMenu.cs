using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardMenu : MonoBehaviour, IOpenable
{
    [SerializeField] private DailyRewardManager dailyRewardManager;
    [SerializeField] private DailyRewardWidget bonusChestRewardWidgetPrefab;

    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private CustomButton openButton;
    [SerializeField] private CustomButton closeButton;
    [SerializeField] private GridLayoutGroup layoutGroup;
    [SerializeField] private TextLocalizer resetTimeText;

    private List<DailyRewardWidget> spawnedWidgets = new();

    public bool IsShowed { get; private set; } = false;

    public event Action OnShowed;
    public event Action OnHidden;

    private void OnEnable()
    {
        dailyRewardManager.OnDailyRewardReset += OnDailyRewardReset;
        slidePanel.OnHidden += HandleHidden;
        openButton.OnReleased.AddListener(OnOpenButtonClicked);
        closeButton.OnReleased.AddListener(OnCloseButtonClicked);
    }

    private void OnDisable()
    {
        dailyRewardManager.OnDailyRewardReset -= OnDailyRewardReset;
        slidePanel.OnHidden -= HandleHidden;
        openButton.OnReleased.RemoveListener(OnOpenButtonClicked);
        closeButton.OnReleased.RemoveListener(OnCloseButtonClicked);
    }

    private void Start()
    {
        StartCoroutine(CreateWidgetsCoroutine());
    }

    private void Update()
    {
        resetTimeText.UpdateText();
    }

    public void Show()
    {
        IsShowed = true;
        slidePanel.Show();
        dailyRewardManager.SetRewardViewed(true);
        InputStateManager.Instance.AddBlockTarget(this);

        OnShowed?.Invoke();
    }

    public void Hide()
    {
        slidePanel.Hide();
    }

    private void HandleHidden()
    {
        IsShowed = false;
        InputStateManager.Instance.RemoveBlockTarget(this);

        OnHidden?.Invoke();
    }

    private void CreateRewardWidgets()
    {
        foreach (var reward in dailyRewardManager.CurrentRewards) {
            var widget = DailyRewardWidgetFactory.CreateWidget(bonusChestRewardWidgetPrefab, layoutGroup.transform, reward);
            spawnedWidgets.Add(widget);
        }
    }

    private void RemoveRewardWidgets()
    {
        for (int i = spawnedWidgets.Count - 1; i >= 0; i--) {
            var widget = spawnedWidgets[i];
            if (widget) {
                Destroy(widget.gameObject);
            }
            
            spawnedWidgets.RemoveAt(i);
        }
    }

    private void OnDailyRewardReset()
    {
        RemoveRewardWidgets();
        CreateRewardWidgets();
    }

    private void OnOpenButtonClicked()
    {
        Show();
    }

    private void OnCloseButtonClicked()
    {
        Hide();
    }

    private IEnumerator CreateWidgetsCoroutine()
    {
        yield return new WaitForEndOfFrame();

        RemoveRewardWidgets();
        CreateRewardWidgets();
    }
}