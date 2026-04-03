using UnityEngine;
using UnityEngine.EventSystems;

public class RaidMenu : UIBehaviour
{
    [SerializeField] private GameObject contentRoot;

    protected override void OnEnable()
    {
        base.OnEnable();

        RaidManager.onRaidStarted += OnRaidStarted;
        RaidManager.onRaidFinished += OnRaidFinished;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        RaidManager.onRaidStarted -= OnRaidStarted;
        RaidManager.onRaidFinished -= OnRaidFinished;
    }

    private void Show()
    {
        contentRoot.gameObject.SetActive(true);
    }

    private void Hide()
    {
        contentRoot.gameObject.SetActive(false);
    }

    private void OnRaidStarted()
    {
        Show();
    }

    private void OnRaidFinished()
    {
        Hide();
    }
}