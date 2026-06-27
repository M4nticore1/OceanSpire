using UnityEngine;

public class ElevatorSpeedController : MonoBehaviour
{
    [SerializeField] private ElevatorCabinConstruction elevatorCabin;
    [SerializeField] private float normalSpeed = 1f;
    [SerializeField] private float raidSpeed = 2f;
    [SerializeField] private float energyShortageSpeedMultiplier = 0.5f;

    private RaidManager raidManager => RaidManager.Instance;
    private EnergyShortageNotificationController energyShortageNotification => EnergyShortageNotificationController.Instance;

    private void OnEnable()
    {
        elevatorCabin.OnMovementStarted += OnCabinMovementStarted;

        raidManager.OnRaidStarted += OnRaidStarted;
        raidManager.OnRaidEnded += OnRaidEnded;

        energyShortageNotification.OnNotificated += OnEnertyShortageNotificated;
        energyShortageNotification.OnUnnotificated += OnEnertyShortageUnnotificated;
    }

    private void OnDisable()
    {
        elevatorCabin.OnMovementStarted -= OnCabinMovementStarted;

        raidManager.OnRaidStarted -= OnRaidStarted;
        raidManager.OnRaidEnded -= OnRaidEnded;

        energyShortageNotification.OnNotificated -= OnEnertyShortageNotificated;
        energyShortageNotification.OnUnnotificated -= OnEnertyShortageUnnotificated;
    }

    private void Start()
    {
        UpdateSpeed();
    }

    private void UpdateSpeed()
    {
        float speed = normalSpeed;

        if (raidManager.IsUnderRaid) {
            speed = raidSpeed;
        }

        if (energyShortageNotification.IsNotificated) {
            speed *= energyShortageSpeedMultiplier;
        }

        elevatorCabin.SetMoveSpeed(speed);
    }

    private void OnCabinMovementStarted()
    {
        UpdateSpeed();
    }

    private void OnRaidStarted()
    {
        UpdateSpeed();
    }

    private void OnRaidEnded(RaidEndedResult result)
    {
        UpdateSpeed();
    }

    private void OnEnertyShortageNotificated()
    {
        UpdateSpeed();
    }

    private void OnEnertyShortageUnnotificated()
    {
        UpdateSpeed();
    }
}