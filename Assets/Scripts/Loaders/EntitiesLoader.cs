using UnityEngine;

public class EntitiesLoader : MonoBehaviour
{
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

                HumanEntry data = new HumanEntry((int)CreatureIdEnum.Human, instanceId, HumanStateEnum.Citizen, finalPosition, rotation.eulerAngles, -1, false);
                Human citizen = CreatureFactory.CreateHuman(data);
            }
        }
    }
}