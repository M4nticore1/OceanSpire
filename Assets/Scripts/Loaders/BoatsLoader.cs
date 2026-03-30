using UnityEngine;

public class BoatsLoader : MonoBehaviour
{
    [SerializeField] private BoatIdEnum[] startBoatIds;
    [SerializeField] PierModule pier;

    private void Start()
    {
        WorldData data = WorldSaveManager.Instance.currentSaveWorldData;
        LoadBoats(data);
    }

    private void LoadBoats(WorldData saveData)
    {
        if (saveData != null) {

        }
        else {
            BoatIdEnum[] boatIds = startBoatIds;

            for (int i = 0; i < boatIds.Length; i++) {
                int id = (int)boatIds[i];
                Boat prefab = BoatsList.Instance.boats[id];
                BoatDockPoint spawnTransform = pier.PierConstruction.BoatDocks[i];
                Vector3 position = spawnTransform.DockTransform.position;
                Vector3 rotation = spawnTransform.DockTransform.rotation.eulerAngles;
                float health = prefab.Health.MaxHealth;

                BoatEntry data = new BoatEntry (id, position, rotation, health);
                Boat boat = BoatFactory.CreateBoat(data);
            }
        }
    }
}
