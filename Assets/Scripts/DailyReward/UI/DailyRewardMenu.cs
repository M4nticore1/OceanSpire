using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardMenu : MonoBehaviour, IOpenable
{
    [SerializeField] private DailyRewardWidget bonusChestRewardWidgetPrefab;
    [SerializeField] private GridLayoutGroup layoutGroup;
    [SerializeField] private TextLocalizer resetTimeText;

    private List<DailyRewardWidget> spawnedWidgets = new();

    private void OnEnable()
    {
        DailyRewardManager.Instance.OnDailyRewardReset += OnChestUpdated;
    }

    private void OnDisable()
    {
        DailyRewardManager.Instance.OnDailyRewardReset -= OnChestUpdated;
    }

    private void Start()
    {
        CreateRewardWidgets();
    }

    private void Update()
    {
        resetTimeText.UpdateText();
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void CreateRewardWidgets()
    {
        foreach (var reward in DailyRewardManager.Instance.CurrentRewards) {
            var widget = DailyRewardWidgetFactory.CreateWidget(bonusChestRewardWidgetPrefab, layoutGroup.transform, reward);

            spawnedWidgets.Add(widget);
        }
    }

    private void ClearRewardWidgets()
    {
        for (int i = spawnedWidgets.Count; i >= 0; i--) {
            spawnedWidgets.RemoveAt(i);
        }
    }

    private void OnChestUpdated()
    {
        ClearRewardWidgets();
        CreateRewardWidgets();
    }
}