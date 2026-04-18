using UnityEngine;

[CreateAssetMenu(fileName = "HumanNamesList", menuName = "Lists/HumanNamesList")]
public class HumanNamesList : ScriptableObject
{
    private static HumanNamesList instance;
    public static HumanNamesList Instance
    {
        get
        {
            if (instance == null) {
                instance = Resources.Load<HumanNamesList>("Lists/HumanNamesList");
            }
            return instance;
        }
    }

    [Header("Male Names")]
    [SerializeField] private LocalizationItem[] maleFirstNames;
    [SerializeField] private LocalizationItem[] maleLastNames;

    [Header("Female Names")]
    [SerializeField] private LocalizationItem[] femaleFirstNames;
    [SerializeField] private LocalizationItem[] femaleLastNames;

    // Getters
    public LocalizationItem GetMaleFirstName(int index)
    {
        return maleFirstNames[index];
    }

    public LocalizationItem GetMaleLastName(int index)
    {
        return maleLastNames[index];
    }

    public LocalizationItem GetFemaleFirstName(int index)
    {
        return femaleFirstNames[index];
    }

    public LocalizationItem GetFemaleLastName(int index)
    {
        return femaleLastNames[index];
    }

    // Random Localizations
    public LocalizationItem GetRandomMaleFirstName()
    {
        return GetRandomLocalization(maleFirstNames);
    }

    public LocalizationItem GetRandomMaleLastName()
    {
        return GetRandomLocalization(maleLastNames);
    }

    public LocalizationItem GetRandomFemaleFirstName()
    {
        return GetRandomLocalization(femaleFirstNames);
    }

    public LocalizationItem GetRandomFemaleLastName()
    {
        return GetRandomLocalization(femaleLastNames);
    }

    // Random Indexes
    public int GetRandomMaleFirstNameId()
    {
        return GetRandomLocalizationIndex(maleFirstNames);
    }

    public int GetRandomMaleLastNameId()
    {
        return GetRandomLocalizationIndex(maleLastNames);
    }

    public int GetRandomFemaleFirstNameId()
    {
        return GetRandomLocalizationIndex(femaleFirstNames);
    }

    public int GetRandomFemaleLastNameId()
    {
        return GetRandomLocalizationIndex(femaleLastNames);
    }

    // Internal
    private int GetRandomLocalizationIndex(LocalizationItem[] array)
    {
        int index = Random.Range(0, array.Length);
        return index;
    }

    private LocalizationItem GetRandomLocalization(LocalizationItem[] array)
    {
        return array[GetRandomLocalizationIndex(array)];
    }
}