using UnityEngine;

public class RaidAudioSystem : MonoBehaviour
{
    [SerializeField] private AudioSource raidAlarmSource;

    private void OnEnable()
    {
        RaidManager.onRaidStarted += OnRaidStarted;
        RaidManager.onRaidEnded += OnRaidEnded;
    }

    private void OnDisable()
    {
        RaidManager.onRaidStarted -= OnRaidStarted;
        RaidManager.onRaidEnded -= OnRaidEnded;
    }

    private void OnRaidStarted()
    {
        raidAlarmSource.Play();
    }

    private void OnRaidEnded(RaidEndedResult result)
    {
        raidAlarmSource.Stop();
    }
}