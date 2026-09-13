using System.Collections.Generic;
using UnityEngine;

public class StatInfoPanelData : InfoPanelData, ILocalizable
{
    public string Value { get; private set; }
    private string placeHolderName = "";

    public StatInfoPanelData(string value, string placeHolderName) : base()
    {
        Value = value;
        this.placeHolderName = placeHolderName;
    }

    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>() {
            { placeHolderName, Value }
        };
    }
}