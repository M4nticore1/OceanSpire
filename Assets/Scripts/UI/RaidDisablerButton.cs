using UnityEngine;

public class RaidDisablerButton : MonoBehaviour
{
    [SerializeField] private CustomButton button;

    private void OnEnable()
    {
        if (!RaidManager.Instance) return;

        UpdateButtonEnabled();

        RaidManager.Instance.OnRaidStarted += OnRaidStarted;
        RaidManager.Instance.OnRaidEnded += OnRaidEnded;
    }

    private void OnDisable()
    {
        if (!RaidManager.Instance) return;

        RaidManager.Instance.OnRaidStarted -= OnRaidStarted;
        RaidManager.Instance.OnRaidEnded -= OnRaidEnded;
    }

    private void UpdateButtonEnabled()
    {
        if (!RaidManager.Instance) return;

        button.SetState(RaidManager.Instance.IsUnderRaid ? CustomButtonState.Disabled : CustomButtonState.Idle);
    }

    private void OnRaidStarted()
    {
        UpdateButtonEnabled();
    }

    private void OnRaidEnded(RaidEndedResult raidResult)
    {
        UpdateButtonEnabled();
    }
}