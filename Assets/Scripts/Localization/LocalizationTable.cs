using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "LocalizationTable", menuName = "Localization/LocalizationDictionary")]
public class LocalizationTable : ScriptableObject
{
    public string LanguageCode => name;

    [SerializeField] private TextAsset localizationAsset;
    public TextAsset LocalizationAsset => localizationAsset;

    [SerializeField] private TMP_FontAsset[] fonts;
    public TMP_FontAsset[] Fonts => fonts;
}
