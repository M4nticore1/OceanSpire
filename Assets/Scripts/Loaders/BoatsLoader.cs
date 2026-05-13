using UnityEngine;

public class BoatsLoader : Loader
{
    [SerializeField] private InstancesManager instancesManager;
    [SerializeField] private BuildingsManager buildingsManager;
    [SerializeField] private DockPointsManager dockPointsManager;
    [SerializeField] private BoatsList boatsList;

    [SerializeField] private BoatIdEnum[] startBoatIds;

    protected override void Load(WorldData data)
    {
        if (data != null && data.Boats != null) {
            LoadBoats(data.Boats);
        }
        else {
            InitBoats();
        }
    }

    private void LoadBoats(BoatData[] boatsData)
    {
        foreach (var boatData in boatsData) {
            var boatPrefab = boatsList.GetBoat(boatData.Id);

            var boat = BoatFactory.CreateBoat(boatPrefab, boatData);
        }
    }

    private void InitBoats()
    {
        BoatIdEnum[] boatIds = startBoatIds;

        for (int i = 0; i < boatIds.Length; i++) {
            int id = (int)boatIds[i];
            int instanceId = instancesManager.GetNextInstanceId();
            Boat prefab = boatsList.GetBoat(id);

            PierModule pier = buildingsManager.PierBuilding.GetComponent<PierModule>();
            BoatDockPoint spawnTransform = pier.PierConstruction.BoatDocks[i];

            Vector3 position = spawnTransform.DockTransform.position;
            Vector3 rotation = spawnTransform.DockTransform.rotation.eulerAngles;

            float health = prefab.Health.MaxHealth;
            int dockId = dockPointsManager.CitizenBoatDocks[i].InstanceId.Id;

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