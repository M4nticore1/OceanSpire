using UnityEngine;

public class BoatsLoader : Loader
{
    [SerializeField] private BoatIdEnum[] startBoatIds;

    protected override void Load(WorldData data)
    {
        if (data != null && data.Boats != null) {
            BoatData[] boatsData = data.Boats;

            for (int i = 0; i < boatsData.Length; i++) {
                BoatData boatData = boatsData[i];
                Boat boatPrefab = BoatsList.Instance.GetBoat(boatData.Id);

                Boat boat = BoatFactory.CreateBoat(boatPrefab, boatData);
            }
        }
        else {
            BoatIdEnum[] boatIds = startBoatIds;

            for (int i = 0; i < boatIds.Length; i++) {
                int id = (int)boatIds[i];
                int instanceId = InstancesManager.Instance.GetNextInstanceId();
                Boat prefab = BoatsList.Instance.GetBoat(id);

                PierModule pier = BuildingsManager.Instance.PierBuilding.GetComponent<PierModule>();
                BoatDockPoint spawnTransform = pier.PierConstruction.BoatDocks[i];

                Vector3 position = spawnTransform.DockTransform.position;
                Vector3 rotation = spawnTransform.DockTransform.rotation.eulerAngles;

                float health = prefab.Health.MaxHealth;
                int dockId = DockPointsManager.Instance.CitizenBoatDocks[i].InstanceId.Id;

                BoatData boatData = new BoatData()
                {
                    Id = id,
                    InstanceId = instanceId,
                    Position = new Vector3Data(position),
                    Rotation = new Vector3Data(rotation),
                    Health = health,
                    DockInstanceId = dockId,
                };

                Boat boat = BoatFactory.CreateBoat(prefab, boatData);
            }
        }
    }
}