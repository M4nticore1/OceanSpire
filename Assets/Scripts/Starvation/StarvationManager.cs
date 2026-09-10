using System;
using System.Collections;
using UnityEngine;

public class StarvationManager : MonoBehaviour
{
    [SerializeField] private CityStorage cityStorage;

    public bool IsUnderStarvation { get; private set; } = false;

    public event Action OnStarvationStarted;
    public event Action OnStarvationEnded;

    private void Awake()
    {
        if (cityStorage == null) {
            Debug.LogError($"[{nameof(StarvationManager)}] City Storage is not valid!");
        }
        if (cityStorage.Inventory == null) {
            Debug.LogError($"[{nameof(StarvationManager)}] Inventory is not valid!");
        }
    }

    private void OnEnable()
    {
        if (cityStorage.Inventory != null) {
            cityStorage.Inventory.OnItemAmountChanged += HandleItemAmountChanged;
        }
    }

    private void OnDisable()
    {
        if (cityStorage.Inventory != null) {
            cityStorage.Inventory.OnItemAmountChanged -= HandleItemAmountChanged;
        }
    }

    private void Start()
    {
        StartCoroutine(UpdateUnderStarvationEndOfFrame());
    }

    private void UpdateUnderStarvation()
    {
        SetUnderStarvation(ShouldSetUnderStarvation());
    }

    private void SetUnderStarvation(bool value)
    {
        if (value == IsUnderStarvation) return;

        if (value) {
            IsUnderStarvation = true;
            OnStarvationStarted?.Invoke();
        }
        else {
            IsUnderStarvation = false;
            OnStarvationEnded?.Invoke();
        }
    }

    private void HandleItemAmountChanged(ItemInstance itemInstance)
    {
        if (itemInstance == GetFoodItem()) {
            StartCoroutine(UpdateUnderStarvationEndOfFrame());
        }
    }

    private bool ShouldSetUnderStarvation()
    {
        var item = GetFoodItem();
        if (item == null) return false;

        return item.Amount <= 0;
    }

    private ItemInstance GetFoodItem()
    {
        if (cityStorage == null) return null;

        var inventory = cityStorage.Inventory;
        if (inventory == null) return null;

        var foodItem = inventory.GetInventoryItem(ItemID.Food);

        return foodItem;
    }

    private IEnumerator UpdateUnderStarvationEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        UpdateUnderStarvation();
    }
}