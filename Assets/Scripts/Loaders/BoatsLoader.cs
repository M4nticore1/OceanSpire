using UnityEngine;

public class BoatsLoader : MonoBehaviour
{
    [SerializeField] private BoatIdEnum[] startBoatIds;
    [SerializeField] PierModule pier;

    private void Start()
    {
        WorldData data = WorldSaveManager.Instance.currentSaveWorldData;

        if (data != null) {

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

                int boatId = InstancesManager.instance.GetNextInstanceId();
                InstancesManager.instance.AddInstanceId(boatId);

                int dockId = DockPointsManager.instance.pierDockPoints[i].InstanceId.id;

                BoatEntry boatData = new BoatEntry(id, boatId, BoatStateEnum.Idle, position, rotation, health, dockId);
                Boat boat = BoatFactory.CreateBoat(boatData);
            }
        }
    }
}