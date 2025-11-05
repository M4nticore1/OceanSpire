using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{

    private void Start()
    {
        LocalizationSystem.LoadLocalizations();
    }
}
