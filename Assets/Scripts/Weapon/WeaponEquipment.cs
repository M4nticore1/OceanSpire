using UnityEngine;

public class WeaponEquipment : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    public WeaponDefinition CurrentDefinition { get; private set; }
    public Weapon spawnedWeapon { get; private set; }

    public void Init(EquipmentData data)
    {
        int id = data.WeaponId;
        WeaponDefinition definition = ItemsList.Instance.GetItem(id) as WeaponDefinition;
        SetWeaponAndApply(definition);
    }

    public void SetWeaponAndApply(WeaponDefinition definition)
    {
        SetWeaponDefinition(definition);
        RemoveWeapon();
        SpawnWeapon();
    }

    public int GetDamage()
    {
        return CurrentDefinition.Damage;
    }

    private void SetWeaponDefinition(WeaponDefinition definition)
    {
        CurrentDefinition = definition;
    }

    private void SpawnWeapon()
    {
        Weapon prefab = CurrentDefinition.WeaponPrefab;
        if (!prefab) return;

        Weapon weapon = WeaponFactory.CreateWeapon(prefab, spawnPoint);
    }

    private void RemoveWeapon()
    {
        if (!spawnedWeapon) return;

        Destroy(spawnedWeapon);
    }
}