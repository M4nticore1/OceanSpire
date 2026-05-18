using System;
using UnityEngine;

public class SetEquipmentButton : MonoBehaviour
{
    [SerializeField] EquipmentCategory category;
    [SerializeField] CustomButton button;

    public static event Action<EquipmentCategory> onClicked;

    private void OnEnable()
    {
        button.OnReleased.AddListener(OnClicked);
    }

    private void OnDisable()
    {
        button.OnReleased.RemoveListener(OnClicked);
    }

    private void OnClicked()
    {
        onClicked?.Invoke(category);
    }
}