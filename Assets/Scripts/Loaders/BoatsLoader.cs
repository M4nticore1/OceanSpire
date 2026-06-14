using UnityEngine;

public class BoatsLoader : WorldLoader
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

            var boat = BoatFactory.CreateBoat(boatPrefab, boatData.Position.Vector3(), Quaternion.Euler(boatData.Rotation.Vector3()), boatData);
        }
    }

    private void InitBoats()
    {
        BoatIdEnum[] boatIds = startBoatIds;

        for (int i = 0; i < boatIds.Length; i++) {
            var id = (int)boatIds[i];
            var instanceId = instancesManager.GetNextInstanceId();
            var prefab = boatsList.GetBoat(id);

            var pier = buildingsManager.PierBuilding.GetComponent<PierModule>();
            var spawnTransform = pier.PierConstruction.BoatDocks[i];

            var position = spawnTransform.DockTransform.position;
            var rotation = spawnTransform.DockTransform.rotation.eulerAngles;

            var dockId = dockPointsManager.CitizenBoatDocks[i].InstanceId.GetInstanceId();

            var boatData = new BoatData()
            {
                Id = id,
                InstanceId = instanceId,
                Position = new Vector3Data(position),
                Rotation = new Vector3Data(rotation),
                DockInstanceId = dockId,
                Status = HumanStatusEnum.Citizen
            };

            var boat = BoatFactory.CreateBoat(prefab, position, Quaternion.Euler(rotation), boatData);
        }
    }
}