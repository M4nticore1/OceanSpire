using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardMenu : MonoBehaviour, IOpenable
{
    [SerializeField] private DailyRewardManager dailyRewardManager;
    [SerializeField] private DailyRewardWidget bonusChestRewardWidgetPrefab;

    [SerializeField] private GameObject content;
    [SerializeField] private CustomButton openButton;
    [SerializeField] private CustomButton closeButton;
    [SerializeField] private GridLayoutGroup layoutGroup;
    [SerializeField] private TextLocalizer resetTimeText;

    private List<DailyRewardWidget> spawnedWidgets = new();

    public event Action OnShowed;
    public event Action OnHidden;

    private void OnEnable()
    {
        dailyRewardManager.OnDailyRewardReset += OnDailyRewardReset;
        openButton.OnReleased.AddListener(OnOpenButtonClicked);
        closeButton.OnReleased.AddListener(OnCloseButtonClicked);
    }

    private void OnDisable()
    {
        dailyRewardManager.OnDailyRewardReset -= OnDailyRewardReset;
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
        content.gameObject.SetActive(true);
        dailyRewardManager.SetRewardViewed(true);
        InputStateManager.Instance.AddBlockTarget();

        OnShowed?.Invoke();
    }

    public void Hide()
    {
        content.gameObject.SetActive(false);
        InputStateManager.Instance.RemoveBlockTarget();

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