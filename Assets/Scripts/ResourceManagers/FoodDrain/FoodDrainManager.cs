using UnityEngine;

public class FoodDrainManager : MonoBehaviour
{
    [SerializeField] private CreaturesManager creaturesManager;
    [SerializeField] private CityStorage cityStorage;

    [SerializeField] private float drainPerMinute = 0.375f;
    [SerializeField] private float drainFrequency = 10f;

    public float CurrentDrainTime { get; private set; }
    public float DrainAmount { get; private set; }

    private void Update()
    {
        CurrentDrainTime += Time.deltaTime;
        if (CurrentDrainTime < drainFrequency)
            return;

        ApplyDrainAmount();
        TryDrainFood();
        ResetCurrentTime();
    }

    public void Init()
    {
        Init(FoodDrainData.Default());
    }

    public void Init(FoodDrainData foodDrainData)
    {
        if (foodDrainData == null) {
            Debug.LogError($"[{nameof(FoodDrainManager)}] Food Drain Data is not valid!");
            Init();
            return;
        }

        DrainAmount = foodDrainData.DrainAmount;
        CurrentDrainTime = foodDrainData.DrainTime;
    }

    private void ApplyDrainAmount()
    {
        float drainPerSecond = drainPerMinute / 60f;

        DrainAmount += drainPerSecond * drainFrequency * creaturesManager.Citizens.Count;
    }

    private void TryDrainFood()
    {
        if (DrainAmount < 1f)
            return;

        var id = ItemID.Food;
        var amount = Mathf.FloorToInt(DrainAmount);

        cityStorage.Inventory.RemoveItemAmount(id, amount);
        DrainAmount -= amount;
    }

    private void ResetCurrentTime()
    {
        CurrentDrainTime = 0f;
    }
}