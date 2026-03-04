using UnityEngine;

public class EntitiesLoader : MonoBehaviour
{
    [SerializeField] protected EntitiesManager entitiesManager;
    private const int startResidentsCount = 2;

    [SerializeField] private Transform entitySpawnPosition = null;
    public const float maxSpawnRange = 5f;

    private void Start()
    {
        WorldData saveData = WorldSaveManager.Instance.currentSaveWorldData;
        LoadEntities(saveData);
    }

    private void LoadEntities(WorldData saveData)
    {
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        if (saveData != null) {
            foreach (var data in saveData.citizensData) {
                Human citizen = EntityFactory.CreateCitizen(data) as Human;
                entitiesManager.Register(citizen);
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

                HumanEntry data = new HumanEntry((int)CreatureIdEnum.Citizen, finalPosition, rotation.eulerAngles);
                Human citizen = EntityFactory.CreateCitizen(data) as Human;
                entitiesManager.Register(citizen);
            }
        }
    }
}