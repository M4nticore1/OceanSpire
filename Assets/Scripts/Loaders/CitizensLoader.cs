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
        Vector3 rotation = Vector3.zero;

        if (saveData != null) {
            foreach (var data in saveData.citizensData) {
                Human citizen = CreatureFactory.CreateHuman(data);
            }
        }
        else {
            position = entitySpawnPosition.position;
            rotation = entitySpawnPosition.rotation.eulerAngles;

            for (int i = 0; i < startResidentsCount; i++) {
                float x = Random.Range(position.x - maxSpawnRange, position.x + maxSpawnRange);
                float y = position.y;
                float z = Random.Range(position.z - maxSpawnRange, position.z + maxSpawnRange);

                Vector3 finalPosition = new Vector3(x, y, z);

                HumanDataV1 data = HumanDataFactory.CreateRandomCitizenData();
                data.SetPosition(finalPosition);
                data.SetRotation(rotation);

                Human citizen = CreatureFactory.CreateHuman(data);
            }
        }
    }
}