# if UNITY_EDITOR
using System.Collections;
using UnityEngine;

public class EditorItemsFill : MonoBehaviour
{
    [SerializeField] private CityStorage CityStorage;

    private void OnEnable()
    {
        StartCoroutine(FillItemCoroutine());
    }

    private void Start()
    {
        StartCoroutine(FillItemCoroutine());
    }

    private void FillItems()
    {
        foreach (var item in CityStorage.Inventory.Items) {
            item.SetAmount(item.Stack.Amount);
        }
    }

    private IEnumerator FillItemCoroutine()
    {
        yield return new WaitForEndOfFrame();

        FillItems();
    }
}
#endif