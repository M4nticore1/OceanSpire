using UnityEngine;

public class WeaponHandlerData
{
    public int weaponId { get; private set; } = 0;

    public WeaponHandlerData(int weaponId)
    {
        this.weaponId = weaponId;
    }
}

public class WeaponEquipment : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    private WeaponDefinition currentDefinition;
    private Weapon spawnedWeapon;

    public void Init(WeaponHandlerData data)
    {
        int id = data.weaponId;
        WeaponDefinition definition = ItemsList.Instance.GetItemData(id) as WeaponDefinition;
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