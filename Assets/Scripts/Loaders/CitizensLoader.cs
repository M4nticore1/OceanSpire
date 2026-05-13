using UnityEngine;

public class CitizensLoader : Loader
{
    [SerializeField] private int startResidentsCount = 2;
    [SerializeField] private Transform entitySpawnPosition;
    [SerializeField] public float maxSpawnRange = 5f;

    protected override void Load(WorldData data)
    {
        if (data != null && data.Citizens != null) {
            LoadHumans(data.Citizens);
        }
        else {
            InitCitizens();
        }

        if (data != null && data.Wanderers != null) {
            LoadHumans(data.Wanderers);
        }

        if (data != null && data.Raiders != null) {
            LoadHumans(data.Raiders);
        }
    }

    private void LoadHumans(HumanData[] humansData)
    {
        foreach (var data in humansData) {
            if (data == null) continue;

            var citizen = CreatureFactory.CreateHuman(data);
        }
    }

    private void InitCitizens()
    {
        var position = entitySpawnPosition.position;
        var rotation = entitySpawnPosition.rotation.eulerAngles;

        for (int i = 0; i < startResidentsCount; i++) {
            float x = Random.Range(position.x - maxSpawnRange, position.x + maxSpawnRange);
            float y = position.y;
            float z = Random.Range(position.z - maxSpawnRange, position.z + maxSpawnRange);

            var finalPosition = new Vector3(x, y, z);

            var data = HumanDataFactory.CreateRandomCitizenData();
            data.Position = new Vector3Data(finalPosition);
            data.Rotation = new Vector3Data(rotation);

            var citizen = CreatureFactory.CreateHuman(data);
        }
    }
}