using UnityEngine;

public class WeaponEquipment : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    private WeaponDefinition currentDefinition;
    private Weapon spawnedWeapon;

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
        return currentDefinition.Damage;
    }

    private void SetWeaponDefinition(WeaponDefinition definition)
    {
        currentDefinition = definition;
    }

    private void SpawnWeapon()
    {
        Weapon prefab = currentDefinition.WeaponPrefab;
        if (!prefab) return;

        Weapon weapon = WeaponFactory.CreateWeapon(prefab, spawnPoint);
    }

    private void RemoveWeapon()
    {
        if (!spawnedWeapon) return;

        Destroy(spawnedWeapon);
    }
}