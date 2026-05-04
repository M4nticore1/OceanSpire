using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BonusChestMenu : MonoBehaviour
{
    [SerializeField] private BonusChestRewardWidget bonusChestRewardWidgetPrefab;
    [SerializeField] private GridLayoutGroup layoutGroup;

    private List<BonusChestRewardWidget> spawnedWidgets = new();

    private void OnEnable()
    {
        BonusChestManager.Instance.onBonusChestUpdated += OnChestUpdated;
    }

    private void OnDisable()
    {
        BonusChestManager.Instance.onBonusChestUpdated -= OnChestUpdated;
    }

    private void Start()
    {
        CreateRewardWidgets();
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
        for (int i = 0; i < BonusChestManager.Instance.MaxRewardsCount; i++) {
            var reward = BonusChestManager.Instance.GetCurrentReward(i);
            var widget = BonusChestRewardWidgetFactory.CreateWidget(bonusChestRewardWidgetPrefab, layoutGroup.transform, reward);

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