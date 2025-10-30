using TMPro;
using UnityEngine;

[System.Serializable]
public class LocalizedText
{
    string languageName = "";
    public LocalizedText(string languageName)
    {
        this.languageName = languageName;
    }
}

public class LocalizeText : MonoBehaviour
{
    private TextMeshProUGUI text;

    private static string[] languageNames = { "English", "Spanish" };
    [SerializeField] private LocalizedText[] localizedTexts = new LocalizedText[0];

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {

    }
}
