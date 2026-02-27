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
                Boat prefab = BoatsList.Instance.boats[(int)boatIds[i]];
                BoatDockPoint spawnTransform = pier.PierConstruction.BoatDocks[i];
                Vector3 spawnPosition = spawnTransform.DockTransform.position;
                Vector3 spawnRotation = spawnTransform.DockTransform.rotation.eulerAngles;

                BoatEntry data = new BoatEntry { position = spawnPosition, rotation = spawnRotation, health = prefab.MaxHealth };
                Boat boat = BoatFactory.CreateBoat((int)boatIds[i], data);
            }
        }
    }
}
