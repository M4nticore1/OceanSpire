using System.Collections.Generic;
using UnityEngine;

public abstract class StatInfoSectionData : InfoSectionData, ILocalizable
{
    public string Value { get; private set; }
    public LocalizationItem LocalizationItem { get; private set; }
    public Sprite Sprite { get; private set; }

    public StatInfoSectionData(string powerConsumption, LocalizationItem powerLocalizationItem, Sprite sprite) : base()
    {
        Value = powerConsumption;
        LocalizationItem = powerLocalizationItem;
        Sprite = sprite;
    }

    public abstract string GetValuePlaceHolderName();

    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>() {
            { GetValuePlaceHolderName(), Value }
        };
    }
}