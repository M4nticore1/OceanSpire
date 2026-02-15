using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "LocalizationTable", menuName = "Localization/LocalizationDictionary")]
public class LocalizationTable : ScriptableObject
{
    [SerializeField] private SystemLanguage language;
    public SystemLanguage Language => language;

    [SerializeField] private TMP_FontAsset[] fonts = null;
    public TMP_FontAsset[] Fonts => fonts;
    [SerializeField] private LocalizationEntry[] items;
    public Dictionary<LocalizationItem, LocalizationEntry> itemsDict = new Dictionary<LocalizationItem, LocalizationEntry>();

    public void Init()
    {
        foreach (var item in items) {
            if (itemsDict.ContainsKey(item.Item)) {
                Debug.LogError($"{item.Item} is already in the dictionary.");
                continue;
            }

            itemsDict.Add(item.Item, item);
        }
    }
}
