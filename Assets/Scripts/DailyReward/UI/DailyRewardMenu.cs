using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardMenu : MonoBehaviour, IOpenable
{
    [SerializeField] private DailyRewardManager dailyRewardManager;
    [SerializeField] private DailyRewardWidget bonusChestRewardWidgetPrefab;

    [SerializeField] private GameObject content;
    [SerializeField] private GridLayoutGroup layoutGroup;
    [SerializeField] private TextLocalizer resetTimeText;
    [SerializeField] private CustomButton closeButton;

    private List<DailyRewardWidget> spawnedWidgets = new();

    private void OnEnable()
    {
        dailyRewardManager.OnDailyRewardReset += OnDailyRewardReset;
        closeButton.OnReleased.AddListener(OnCloseButtonClicked);
    }

    private void OnDisable()
    {
        dailyRewardManager.OnDailyRewardReset -= OnDailyRewardReset;
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

    public void Display()
    {
        content.gameObject.SetActive(true);
        dailyRewardManager.SetRewardViewed(true);
    }

    public void Hide()
    {
        content.gameObject.SetActive(false);
    }

    private void CreateRewardWidgets()
    {
        foreach (var reward in dailyRewardManager.CurrentRewards) {
            var widget = DailyRewardWidgetFactory.CreateWidget(bonusChestRewardWidgetPrefab, layoutGroup.transform, reward);
            spawnedWidgets.Add(widget);
        }
    }

    private void DestroyRewardWidgets()
    {
        for (int i = spawnedWidgets.Count - 1; i >= 0; i--) {
            Destroy(spawnedWidgets[i].gameObject);
            spawnedWidgets.RemoveAt(i);
        }
    }

    private void OnDailyRewardReset()
    {
        DestroyRewardWidgets();
        CreateRewardWidgets();
    }

    private void OnCloseButtonClicked()
    {
        Hide();
    }

    private IEnumerator CreateWidgetsCoroutine()
    {
        yield return new WaitForEndOfFrame();

        CreateRewardWidgets();
    }
}