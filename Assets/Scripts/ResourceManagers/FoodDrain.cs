using UnityEngine;

public class FoodDrain : MonoBehaviour
{
    [SerializeField] private EntitiesManager entitiesManager;
    [SerializeField] private CityStorage cityStorage;

    private float currentAmountToDrain = 0;
    private const float citizenFoodDrain = 0.1f;

    private void Update()
    {
        int id = (int)ItemID.Food;
        float amount = entitiesManager.citizens.Count * citizenFoodDrain;
        currentAmountToDrain += amount * Time.deltaTime;

        if (currentAmountToDrain >= 1) {
            cityStorage.Inventory.RemoveItemAmount(id, (int)currentAmountToDrain);
            currentAmountToDrain = 0;
        }
    }
}