using UnityEngine;

public class RaidAudioSystem : AudioSystem
{
    [SerializeField] private AudioSource raidAlarmSource;

    private bool isSubscribed = false;

    protected override void Subscribe()
    {
        TrySubscribe();
    }

    protected override void Unsubscribe()
    {
        TryUnsubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void OnRaidStarted()
    {
        raidAlarmSource.Play();
    }

    private void OnRaidEnded(RaidEndedResult result)
    {
        raidAlarmSource.Stop();
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;
        if (!RaidManager.Instance) return;

        RaidManager.Instance.OnRaidStarted += OnRaidStarted;
        RaidManager.Instance.OnRaidEnded += OnRaidEnded;

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;
        if (!RaidManager.Instance) return;

        RaidManager.Instance.OnRaidStarted -= OnRaidStarted;
        RaidManager.Instance.OnRaidEnded -= OnRaidEnded;

        isSubscribed = false;
    }
}