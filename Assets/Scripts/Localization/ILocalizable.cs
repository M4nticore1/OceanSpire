using System.Collections.Generic;
using UnityEngine;

public interface ILocalizable
{
    public Dictionary<string, string> Localization { get; }
}
