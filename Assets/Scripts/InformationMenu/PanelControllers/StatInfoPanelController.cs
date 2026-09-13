using UnityEngine;
using UnityEngine.UI;

public abstract class StatInfoPanelController : InfoPanelController
{
    [Header("Stat Controller")]
    [SerializeField] private string placeHolderName = "value";
    protected string PlaceHolderName => placeHolderName;
}