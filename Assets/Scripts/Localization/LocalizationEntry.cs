using System;
using TMPro;
using UnityEngine;

[Serializable]
public class LocalizationEntry
{
    [SerializeField] private LocalizationItem item;
    public LocalizationItem Item => item;

    [SerializeField] private string value = "";
    public string Value => value;

    [SerializeField] private int fontIndex = 0;
    public int FontIndex => fontIndex;
}
