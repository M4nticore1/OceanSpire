using UnityEngine;

public class RaidDisablerButton : MonoBehaviour
{
    [SerializeField] private CustomButton button;

    private RaidManager raidManager => RaidManager.Instance;

    private void OnEnable()
    {
        if (raidManager == null) return;

        UpdateButtonEnabled();

        raidManager.OnRaidStarted += OnRaidStarted;
        raidManager.OnRaidEnded += OnRaidEnded;
        button.OnStateChanged += OnButtonStateChanged;
    }

    private void OnDisable()
    {
        if (raidManager == null) return;

        raidManager.OnRaidStarted -= OnRaidStarted;
        raidManager.OnRaidEnded -= OnRaidEnded;
        button.OnStateChanged -= OnButtonStateChanged;
    }

    private void UpdateButtonEnabled()
    {
        if (raidManager == null) return;

        button.SetState(raidManager.IsUnderRaid ? CustomButtonState.Disabled : CustomButtonState.Idle);
    }

    private void OnRaidStarted()
    {
        UpdateButtonEnabled();
    }

    private void OnRaidEnded(RaidEndedResult raidResult)
    {
        UpdateButtonEnabled();
    }

    private void OnButtonStateChanged(CustomButtonState state)
    {
        if (state == CustomButtonState.Idle) {
            UpdateButtonEnabled();
        }
    }
}