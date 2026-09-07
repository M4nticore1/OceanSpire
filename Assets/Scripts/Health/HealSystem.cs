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
        for (int i = creaturesManager.Citizens.Count - 1; i >= 0; i--) {
            var citizen = creaturesManager.Citizens[i];

            if (!ShouldHeal(citizen)) continue;

            var healthAmount = healPerSecond * healFrequence;
            citizen.HealthComponent.AddHealth(healthAmount, true);
        }
    }

    private void ResetCurrentTime()
    {
        currentTime = 0f;
    }

    private bool ShouldHeal(Human human)
    {
        if (human == null) return false;
        if (!human.HealthComponent.IsAlive) return false;

        return true;
    }
}