using UnityEngine;

public class HealSystem : MonoBehaviour
{
    [SerializeField] private CreaturesManager creaturesManager;
    [SerializeField] private StarvationSystem starvationSystem;
    [SerializeField] private float healPerSecond = 0.1f;

    [SerializeField] private float healFrequence = 10f;
    private float currentTime = 0f;

    private void Update()
    {
        if (starvationSystem.IsUnderStarvation) return;

        currentTime += Time.deltaTime;
        if (currentTime < healFrequence) return;

        HealCitizens();
        ResetCurrentTime();
    }

    private void HealCitizens()
    {
        foreach (var citizen in creaturesManager.Citizens) {
            if (!ShouldHeal(citizen)) continue;

            citizen.HealthComponent.AddHealth(healPerSecond * healFrequence);
        }
    }

    private void ResetCurrentTime()
    {
        currentTime = 0f;
    }

    private bool ShouldHeal(Human human)
    {
        if (!human.HealthComponent.IsAlive) return false;

        return true;
    }
}