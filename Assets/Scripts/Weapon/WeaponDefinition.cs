using UnityEngine;

public enum AttackMethod
{
    Hands,
    Light,
    Heavy
}

[CreateAssetMenu(fileName = "WeaponDefinition", menuName = "Items/WeaponDefinition")]
public class WeaponDefinition : ItemData
{
    [Header("Weapon")]
    [SerializeField] private Weapon weaponPrefab;
    public Weapon WeaponPrefab => weaponPrefab;

    [SerializeField] private int damage;
    public int Damage => damage;

    [SerializeField] private AttackMethod attackMethods;
    public AttackMethod AttackMethods => attackMethods;
}