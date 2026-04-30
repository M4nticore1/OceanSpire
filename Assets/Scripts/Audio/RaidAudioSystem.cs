using UnityEngine;

public class RaidAudioSystem : MonoBehaviour
{
    [SerializeField] private AudioSource raidAlarmSource;

    private bool isSubscribed = false;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
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

        RaidManager.Instance.onRaidStarted += OnRaidStarted;
        RaidManager.Instance.onRaidEnded += OnRaidEnded;

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;
        if (!RaidManager.Instance) return;

        RaidManager.Instance.onRaidStarted -= OnRaidStarted;
        RaidManager.Instance.onRaidEnded -= OnRaidEnded;

        isSubscribed = false;
    }
}