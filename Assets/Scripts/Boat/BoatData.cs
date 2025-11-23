using UnityEngine;

[CreateAssetMenu(fileName = "BoatData", menuName = "Scriptable Objects/BoatData")]
public class BoatData : ScriptableObject
{
    [SerializeField] private int boatId = 0;
    public int BoatId => boatId;

    [SerializeField] private int maxHealth = 0;
    public int MaxHealth => maxHealth;

    [SerializeField] private int speed = 0;
    public int Speed => speed;

    [SerializeField] private int maxWeight = 0;
    public int MaxWeight { get { return maxWeight; } }
}
