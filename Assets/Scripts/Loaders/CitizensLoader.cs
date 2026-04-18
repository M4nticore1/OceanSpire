using UnityEngine;

public class CitizensLoader : MonoBehaviour
{
    private const int startResidentsCount = 2;

    [SerializeField] private Transform entitySpawnPosition;
    [SerializeField] public float maxSpawnRange = 5f;

    private void Start()
    {
        WorldData saveData = WorldSaveManager.Instance.currentSaveWorldData;

        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        if (saveData != null) {
            foreach (var data in saveData.citizensData) {
                Human citizen = CreatureFactory.CreateHuman(data);
            }
        }
        else {
            position = entitySpawnPosition.position;
            rotation = entitySpawnPosition.rotation;

            for (int i = 0; i < startResidentsCount; i++) {
                float x = Random.Range(position.x - maxSpawnRange, position.x + maxSpawnRange);
                float y = position.y;
                float z = Random.Range(position.z - maxSpawnRange, position.z + maxSpawnRange);

                Vector3 finalPosition = new Vector3(x, y, z);

                int id = (int)CreatureIdEnum.Human;
                int instanceId = InstancesManager.instance.GetNextInstanceId();
                float health = CreaturesList.Instance.Creatures[id].GetComponent<Health>().MaxHealth;

                int damage = WeaponsDataGenerator.GetMinWeaponDamageId();
                WeaponHandlerData weaponsData = WeaponsDataGenerator.GetRandomDataGenerator(damage, damage);

                SkillsData skillsData = SkillsGenerator.GetRandomSkillsData(SkillsGenerator.GetLevelsCount());

                HumanEntry data = new HumanEntry(id, instanceId, finalPosition, rotation.eulerAngles, HumanStatusEnum.Citizen, health, -1, -1, false, weaponsData, skillsData);
                Human citizen = CreatureFactory.CreateHuman(data);
            }
        }
    }
}