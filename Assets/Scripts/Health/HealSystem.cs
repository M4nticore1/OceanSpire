using UnityEngine;

public class HealSystem : MonoBehaviour
{
    [SerializeField] private float healPerSecond = 0.1f;

    [SerializeField] private float healFrequence = 10f;
    private float currentTime = 0f;

    private void Update()
    {
        if (StarvationSystem.Instance.IsUnderStarvation) return;

        currentTime += Time.deltaTime;
        if (currentTime < healFrequence) return;

        HealCitizens();
        ResetCurrentTime();
    }

    private void HealCitizens()
    {
        foreach (var citizen in CreaturesManager.Instance.Citizens) {
            citizen.HealthComponent.AddHealth(healPerSecond * healFrequence);
        }
    }

    private void ResetCurrentTime()
    {
        currentTime = 0f;
    }
}