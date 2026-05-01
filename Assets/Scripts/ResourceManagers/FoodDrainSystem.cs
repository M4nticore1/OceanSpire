using UnityEngine;

public class FoodDrainSystem : MonoBehaviour
{
    [SerializeField] private CreaturesManager entitiesManager;

    [SerializeField] private float drainPerSecond = 0.1f;
    [SerializeField] private float drainFrequency = 10f;
    private float currentTime = 0f;

    private float drainAmount = 0;

    private void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime < drainFrequency) return;

        ApplyDrainAmount();
        TryDrainFood();
        ResetCurrentTime();
    }

    private void ApplyDrainAmount()
    {
        drainAmount += drainPerSecond * drainFrequency * CreaturesManager.Instance.Citizens.Count;
    }

    private void TryDrainFood()
    {
        if (drainAmount < 1f) return;

        int id = (int)ItemID.Food;
        int amount = (int)drainAmount;

        CityStorage.Instance.Inventory.RemoveItemAmount(id, amount);
        drainAmount -= amount;
    }

    private void ResetCurrentTime()
    {
        currentTime = 0f;
    }
}