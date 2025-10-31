using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static event System.Action<Dictionary<string, string>> OnLanguageChanged;

    private void OnEnable()
    {
        PlayerSettings.OnLanguageChanged += ChangeLanguage;
    }

    private void OnDisable()
    {

    }

    private void ChangeLanguage(string json)
    {
        Dictionary<string, string> dictionary = JsonToDictionary(json);
        OnLanguageChanged?.Invoke(dictionary);
    }

    private Dictionary<string, string> JsonToDictionary(string json)
    {
        var dict = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(json)) return dict;

        json = json.Trim().TrimStart('{').TrimEnd('}');
        var entries = json.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var entry in entries)
        {
            var kv = entry.Split(new[] { ':' }, 2);
            if (kv.Length == 2)
            {
                string key = kv[0].Trim().Trim('"');
                string value = kv[1].Trim().Trim('"').Replace("\\\"", "\"").Replace("\\\\", "\\");
                dict[key] = value;
            }
        }
        return dict;
    }
}
