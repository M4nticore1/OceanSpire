using UnityEngine;

[CreateAssetMenu(fileName = "LocalizationsList", menuName = "Localization/LocalizationsList")]
public class LocalizationsList : ScriptableObject
{
    private static LocalizationsList instance = null;
    public static LocalizationsList Instance
    {
        get
        {
            if (instance == null) {
                instance = Resources.Load<LocalizationsList>("Lists/LocalizationsList");
                instance.Init();
            }

            return instance;
        }
    }

    [SerializeField] private LocalizationTable[] localizations = null;
    public LocalizationTable[] Localizations => localizations;

    private bool isInited = false;

    private void Init()
    {
        foreach (var localization in Localizations) {
            if (!localization) {
                Debug.LogError("localization is not valid.");
                continue;
            }

            localization.Init();
        }

        isInited = true;
    }
}
