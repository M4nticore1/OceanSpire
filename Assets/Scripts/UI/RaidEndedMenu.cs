using TMPro;
using UnityEngine;

public class RaidEndedMenu : MonoBehaviour
{
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private TextMeshProUGUI noLossesText;

    [SerializeField] private float visibilityTime = 0f;
    private float currentVisibilityTime = 0f;

    private bool isOpened = false;

    private void OnEnable()
    {
        RaidManager.onRaidFinished += OnRaidFinished;
    }

    private void OnDisable()
    {
        RaidManager.onRaidFinished -= OnRaidFinished;
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
        InitLosses();
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

    private void InitLosses()
    {

    }
}