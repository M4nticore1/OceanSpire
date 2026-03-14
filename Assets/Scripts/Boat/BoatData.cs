using UnityEngine;

public enum BoatIdEnum
{
    BasicBoat
}

[CreateAssetMenu(fileName = "BoatData", menuName = "Scriptable Objects/BoatData")]
public class BoatData : ScriptableObject
{
    [SerializeField] private BoatIdEnum boatId = BoatIdEnum.BasicBoat;
    public int BoatId => (int)boatId;

    [SerializeField] private string boatIdName = "";
    public string BoatIdName => boatIdName;

    [SerializeField] private LocalizationItem nameLocalization;
    public LocalizationItem NameLocalization => nameLocalization;

    //[SerializeField] private int speed = 0;
    //public int Speed => speed;

    public const float healthDrainInterval = 10f;
    public const float healthDisplayThreshold = 0.25f;
    public const float correctDockRotationSpeed = 0.5f;

    [Header("UI")]
    [SerializeField] private ContextMenu detailsMenuWidget = null;
    public ContextMenu DetailsMenuWidget => detailsMenuWidget;
}
