using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "LocalizationTable", menuName = "Localization/LocalizationDictionary")]
public class LocalizationTable : ScriptableObject
{
    public string LanguageCode
    {
        get {
            Debug.Log(name);
            return name;
        }
    }

    [SerializeField] private TextAsset localizationAsset;
    public TextAsset LocalizationAsset => localizationAsset;

    [SerializeField] private TMP_FontAsset[] fonts;
    public TMP_FontAsset[] Fonts => fonts;
}
