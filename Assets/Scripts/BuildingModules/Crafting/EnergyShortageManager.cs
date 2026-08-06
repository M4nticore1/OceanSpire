using System;
using UnityEngine;

public class EnergyShortageManager : MonoBehaviour
{
    public static EnergyShortageManager Instance { get; private set; }

    [SerializeField] private CityStorage cityStorage;

    public bool IsUnderEnergyShortage { get; private set; } = false;

    public event Action OnEnergyShortageStarted;
    public event Action OnEnergyShortageEnded;

    private void Awake()
    {
        if (Instance) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        cityStorage.Inventory.OnItemAmountChanged += HandleStorageAmountChanged;
    }

    private void OnDisable()
    {
        cityStorage.Inventory.OnItemAmountChanged -= HandleStorageAmountChanged;
    }

    private void Start()
    {
        UpdateEnergyShortage();
    }

    private void UpdateEnergyShortage()
    {
        var lastShortage = IsUnderEnergyShortage;
        var item = cityStorage.Inventory.GetItem(ItemID.Electricity);
        if (item == null) {
            Debug.LogError($"[{nameof(EnergyShortageManager)}] Electricity Item is not valid!");
            return;
        }

        IsUnderEnergyShortage = item.Amount <= 0;

        if (IsUnderEnergyShortage != lastShortage) {
            if (IsUnderEnergyShortage) {
                OnEnergyShortageStarted?.Invoke();
            }
            else {
                OnEnergyShortageEnded?.Invoke();
            }
        }
    }

    private void HandleStorageAmountChanged(ItemInstance item)
    {
        UpdateEnergyShortage();
    }
}