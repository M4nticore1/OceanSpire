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

    [SerializeField] private string boatName = "";
    public string BoatName => boatName;

    //[SerializeField] private int maxHealth = 0;
    //public int MaxHealth => maxHealth;

    //[SerializeField] private int speed = 0;
    //public int Speed => speed;

    [SerializeField] private float maxHealth = 0;
    public float MaxHealth => maxHealth;

    [SerializeField] private int maxWeight = 0;
    public int MaxWeight => maxWeight;

    [SerializeField] private int lootCollectTime = 0;
    public int LootCollectTime => lootCollectTime;

    public const float unloadLootSpeed = 20.0f;
    public const float healthDrainInterval = 10f;
    public const float healthDisplayThreshold = 0.25f;
    public const float correctDockRotationSpeed = 0.5f;

    [Header("UI")]
    [SerializeField] private ContextMenuUI detailsMenuWidget = null;
    public ContextMenuUI DetailsMenuWidget => detailsMenuWidget;
}
