using UnityEngine;

public class HealthDrainer : MonoBehaviour
{
    Health healthComponent = null;

    [SerializeField] private float drainHealthDuration = 1f;
    private double lastDrainHealthTime = 0d;

    private void Awake()
    {
        healthComponent = GetComponent<Health>();
    }

    public void ProcessDrainHealth()
    {
        if (Time.timeAsDouble < lastDrainHealthTime + drainHealthDuration) return;

        DrainHealth();
    }

    private void DrainHealth()
    {
        healthComponent.RemoveHealth(1f);
        lastDrainHealthTime = Time.timeAsDouble;
    }
}
