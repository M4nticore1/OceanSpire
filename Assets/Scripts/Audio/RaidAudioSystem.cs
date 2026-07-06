using UnityEngine;
using UnityEngine.Audio;

public class RaidAudioSystem : AudioSystem
{
    [SerializeField] private RaidManager raidManager;

    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private AudioSource raidAlarmSource;
    [SerializeField] private AudioClip raidRandedVictory;
    [SerializeField] private AudioClip raidRandedDefeat;

    protected override void Subscribe()
    {
        raidManager.OnRaidStarted += OnRaidStarted;
        raidManager.OnRaidEnded += OnRaidEnded;
    }

    protected override void Unsubscribe()
    {
        raidManager.OnRaidStarted -= OnRaidStarted;
        raidManager.OnRaidEnded -= OnRaidEnded;
    }

    private void OnRaidStarted()
    {
        raidAlarmSource.Play();
    }

    private void OnRaidEnded(RaidEndedResult result)
    {
        raidAlarmSource.Stop();

        if (result.IsRepeled) {
            AudioUtils.PlaySFX(raidRandedVictory, mixerGroup);
        }
        else {
            AudioUtils.PlaySFX(raidRandedDefeat, mixerGroup);
        }
    }
}