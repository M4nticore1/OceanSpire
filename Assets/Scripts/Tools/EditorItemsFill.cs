# if UNITY_EDITOR
using UnityEngine;

public class EditorItemsFill : MonoBehaviour
{
    [SerializeField] private CityStorage CityStorage;

    private void OnEnable()
    {
        FillItems();
    }

    private void Start()
    {
        FillItems();
    }

    private void FillItems()
    {
        foreach (var item in CityStorage.Inventory.Items) {
            item.SetAmount(item.Stack.Amount);
        }
    }
}
#endif