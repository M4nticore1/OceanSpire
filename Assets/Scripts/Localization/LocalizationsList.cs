using UnityEngine;

[CreateAssetMenu(fileName = "LocalizationsList", menuName = "Localization/LocalizationsList")]
public class LocalizationsList : ScriptableObject
{
    private static LocalizationsList instance = null;
    public static LocalizationsList Instance
    {
        get {
            if (instance == null) {
                instance = Resources.Load<LocalizationsList>("Lists/LocalizationsList");
            }
            return instance;
        }
    }

    [SerializeField] private LocalizationTable[] localizations = null;
    public LocalizationTable[] Localizations => localizations;
}
