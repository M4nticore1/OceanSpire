using System.Collections.Generic;
using UnityEngine;

public class PlayerSettings : MonoBehaviour
{
    private string currentLanguageKey = "en";

    public static event System.Action<string> OnLanguageChanged;
}
