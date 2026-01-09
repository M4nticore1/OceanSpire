using UnityEngine;

[CreateAssetMenu(fileName = "BoatData", menuName = "Scriptable Objects/BoatData")]
public class BoatData : ScriptableObject
{
    [SerializeField] private int boatId = 0;
    public int BoatId => boatId;

    [SerializeField] private string boatIdName = "";
    public string BoatIdName => boatIdName;

    [SerializeField] private string boatName = "";
    public string BoatName => boatName;

    [SerializeField] private int maxHealth = 0;
    public int MaxHealth => maxHealth;

    //[SerializeField] private int speed = 0;
    //public int Speed => speed;

    [SerializeField] private int maxWeight = 0;
    public int MaxWeight => maxWeight;

    [SerializeField] private int lootCollectTime = 0;
    public int LootCollectTime => lootCollectTime;

    public const float unloadLootSpeed = 20.0f;
    public const float healthDrainInterval = 10f;
    public const float healthDisplayThreshold = 0.25f;
    public const float correctDockRotationSpeed = 0.5f;

    [Header("UI")]
    [SerializeField] private ContextMenu detailsMenuWidget = null;
    public ContextMenu DetailsMenuWidget => detailsMenuWidget;
}
