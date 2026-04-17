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
                int instanceId = InstancesManager.instance.GetNextInstanceId();
                InstancesManager.instance.AddInstanceId(instanceId);

                int humanId = (int)CreatureIdEnum.Human;
                float health = CreaturesList.Instance.Creatures[humanId].GetComponent<Health>().MaxHealth;

                WeaponHandlerData weaponsData = WeaponsDataGenerator.GetRandomDataGenerator(WeaponsDataGenerator.GetMaxWeaponDamage());
                SkillsData skillsData = SkillsGenerator.GetRandomSkillsData(SkillsGenerator.GetLevelsCount());

                HumanEntry data = new HumanEntry(humanId, HumanStatusEnum.Citizen, finalPosition, rotation.eulerAngles, health, weaponsData, skillsData, -1, false);
                Human citizen = CreatureFactory.CreateHuman(data);
            }
        }
    }
}