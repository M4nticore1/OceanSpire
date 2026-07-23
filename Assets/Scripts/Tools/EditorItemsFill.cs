using System.Collections;
using UnityEngine;

public class EditorItemsFill : MonoBehaviour
{
    [SerializeField] private CityStorage CityStorage;

    private void OnEnable()
    {
#if UNITY_EDITOR
        StartCoroutine(FillItemCoroutine());
#endif
    }

    private void Start()
    {
#if UNITY_EDITOR
        StartCoroutine(FillItemCoroutine());
#endif
    }

    private void FillItems()
    {
#if UNITY_EDITOR
        foreach (var item in CityStorage.Inventory.Items) {
            CityStorage.Inventory.AddItem(item.Definition.ItemId, item.Stack.Amount);
        }
#endif
    }

    private IEnumerator FillItemCoroutine()
    {
#if UNITY_EDITOR
        yield return new WaitForEndOfFrame();

        FillItems();
#else
        yield break;
#endif
    }
}