using UnityEngine;

public class FoodDrainSystem : MonoBehaviour
{
    [SerializeField] private CreaturesManager entitiesManager;

    [SerializeField] private float drainPerSecond = 0.1f;
    [SerializeField] private float drainFrequency = 10f;
    private float currentTime = 0f;

    private void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime < drainFrequency) return;

        DrainFood();
        ResetCurrentTime();
    }

    private void DrainFood()
    {
        int id = (int)ItemID.Food;
        int amount = (int)(drainPerSecond * drainFrequency * CreaturesManager.Instance.Citizens.Count);

        CityStorage.Instance.Inventory.RemoveItemAmount(id, amount);
    }

    private void ResetCurrentTime()
    {
        currentTime = 0f;
    }
}