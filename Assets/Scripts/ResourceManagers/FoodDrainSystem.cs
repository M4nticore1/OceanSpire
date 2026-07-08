using UnityEngine;

public class FoodDrainSystem : MonoBehaviour
{
    [SerializeField] private CreaturesManager creaturesManager;
    [SerializeField] private CityStorage cityStorage;

    [SerializeField] private float drainPerSecond = 0.01f;
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
        drainAmount += drainPerSecond * drainFrequency * creaturesManager.Citizens.Count;
    }

    private void TryDrainFood()
    {
        if (drainAmount < 1f) return;

        var id = ItemID.Food;
        var amount = (int)drainAmount;

        cityStorage.Inventory.RemoveItem(id, amount);
        drainAmount -= amount;
    }

    private void ResetCurrentTime()
    {
        currentTime = 0f;
    }
}