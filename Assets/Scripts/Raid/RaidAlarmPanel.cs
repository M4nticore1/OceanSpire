using UnityEngine;
using UnityEngine.EventSystems;

public class RaidAlarmPanel : UIBehaviour
{
    [SerializeField] private RaidManager raidManager;
    [SerializeField] private GameObject contentRoot;

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
        Hide();
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

    private void OnRaidEnded(RaidEndedResult result)
    {
        Hide();
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;

        raidManager.OnRaidStarted += OnRaidStarted;
        raidManager.OnRaidEnded += OnRaidEnded;

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;

        raidManager.OnRaidStarted -= OnRaidStarted;
        raidManager.OnRaidEnded -= OnRaidEnded;

        isSubscribed = false;
    }
}