using UnityEngine;

public class HealthDrainer : MonoBehaviour
{
    Health healthComponent = null;

    [SerializeField] private float damagePerSecond = 1.0f;
    private const float DRAIN_DURATION = 1f;
    private double lastDrainHealthTime = 0d;

    private void Awake()
    {
        healthComponent = GetComponent<Health>();
    }

    public void ProcessDrainHealth()
    {
        if (Time.timeAsDouble < lastDrainHealthTime + DRAIN_DURATION) return;

        DrainHealth();
    }

    private void DrainHealth()
    {
        healthComponent.RemoveHealth(damagePerSecond);
        lastDrainHealthTime = Time.timeAsDouble;
    }
}
