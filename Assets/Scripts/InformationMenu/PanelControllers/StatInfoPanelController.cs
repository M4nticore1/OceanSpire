using UnityEngine;

public abstract class StatInfoPanelController : InfoPanelController
{
    [Header("Stat Controller")]
    [SerializeField] private string placeHolderName = "placeHolderName";
    protected string PlaceHolderName => placeHolderName;
}