using UnityEngine;

public class StarvationSystem : MonoBehaviour
{
    public static StarvationSystem Instance { get; private set; }

    [SerializeField] private float damagePerSecond = 0.1f;

    [SerializeField] private float damageFrequence = 5f;
    private float currentTime = 0f;

    public bool IsUnderStarvation { get; private set; } = false;

    private void Awake()
    {
        if (Instance) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        int id = (int)ItemID.Food;
        if (CityStorage.Instance.Inventory.GetItem(id).item.Amount > 0) {
            TrySetStarvation(false);
            return;
        }

        currentTime += Time.deltaTime;
        if (currentTime < damageFrequence) return;

        DamageCitizens();
        ResetCurrentTime();
        TrySetStarvation(true);
    }

    private void DamageCitizens()
    {
        foreach (var citizen in CreaturesManager.Instance.Citizens) {
            citizen.HealthComponent.RemoveHealth(damagePerSecond * damageFrequence);
        }
    }

    private void ResetCurrentTime()
    {
        currentTime = 0f;
    }

    private void TrySetStarvation(bool value)
    {
        if (IsUnderStarvation == value) return;

        IsUnderStarvation = value;
    }
}