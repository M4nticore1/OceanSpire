using UnityEngine;
using UnityEngine.EventSystems;

public class RaidMenu : UIBehaviour
{
    [SerializeField] private GameObject contentRoot;

    protected override void OnEnable()
    {
        base.OnEnable();

        RaidManager.onRaidStarted += OnRaidStarted;
        RaidManager.onRaidEnded += OnRaidEnded;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        RaidManager.onRaidStarted -= OnRaidStarted;
        RaidManager.onRaidEnded -= OnRaidEnded;
    }

    protected override void Start()
    {
        base.Start();

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
}