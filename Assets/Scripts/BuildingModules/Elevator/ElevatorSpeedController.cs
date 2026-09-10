using UnityEngine;

public class ElevatorSpeedController : MonoBehaviour
{
    [SerializeField] private ElevatorCabinConstruction elevatorCabin;
    [SerializeField] private float normalSpeed = 1f;
    [SerializeField] private float raidSpeed = 2f;
    [SerializeField] private float energyShortageSpeedMultiplier = 0.5f;

    private RaidManager raidManager => RaidManager.Instance;
    private EnergyShortageManager energyShortageManager => EnergyShortageManager.Instance;

    private void OnEnable()
    {
        elevatorCabin.OnMovementStarted += OnCabinMovementStarted;

        raidManager.OnRaidStarted += OnRaidStarted;
        raidManager.OnRaidEnded += OnRaidEnded;

        energyShortageManager.OnEnergyShortageStarted += HandleEnergyShortageStarted;
        energyShortageManager.OnEnergyShortageEnded += HandleEnergyShortageEnded;
    }

    private void OnDisable()
    {
        elevatorCabin.OnMovementStarted -= OnCabinMovementStarted;

        raidManager.OnRaidStarted -= OnRaidStarted;
        raidManager.OnRaidEnded -= OnRaidEnded;

        energyShortageManager.OnEnergyShortageStarted -= HandleEnergyShortageStarted;
        energyShortageManager.OnEnergyShortageEnded -= HandleEnergyShortageEnded;
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

        if (energyShortageManager.IsUnderEnergyShortage) {
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

    private void HandleEnergyShortageStarted()
    {
        UpdateSpeed();
    }

    private void HandleEnergyShortageEnded()
    {
        UpdateSpeed();
    }
}