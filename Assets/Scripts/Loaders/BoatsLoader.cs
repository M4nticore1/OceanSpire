using UnityEngine;

public class BoatsLoader : WorldLoader
{
    [SerializeField] private BuildingsManager buildingsManager;
    [SerializeField] private DockPointsManager dockPointsManager;
    [SerializeField] private BoatsList boatsList;

    [SerializeField] private BoatIdEnum[] startBoatIds;

    protected override void Load(WorldData worldData)
    {
        var data = worldData?.Boats;

        if (data != null) {
            LoadBoats(data);
        }
        else {
            InitBoats();
        }
    }

    private void LoadBoats(BoatData[] boatsData)
    {
        foreach (var boatData in boatsData) {
            var boatPrefab = boatsList.GetBoat(boatData.Id);
            BoatFactory.CreateBoat(boatPrefab, boatData.Position.Vector3(), Quaternion.Euler(boatData.Rotation.Vector3()), boatData);
        }
    }

    private void InitBoats()
    {
        if (buildingsManager == null || buildingsManager.PierBuilding == null) {
            Debug.LogError($"[BoatsLoader] Cannot init boats: {buildingsManager.PierBuilding} is null. Ensure buildings are loaded first.");
            return;
        }

        var pier = buildingsManager.PierBuilding.GetComponent<PierModule>();
        if (pier == null || pier.PierConstruction == null || pier.PierConstruction.BoatDocks == null) {
            Debug.LogError("[BoatsLoader] PierModule or PierConstruction component is missing on PierBuilding!");
            return;
        }

        var boatIds = startBoatIds;
        if (boatIds == null) return;

        for (int i = 0; i < boatIds.Length; i++) {
            if (pier.PierConstruction.BoatDocks.Count <= i) {
                Debug.LogWarning($"[BoatsLoader] Not enough slots in PierConstruction.BoatDocks ({pier.PierConstruction.BoatDocks.Count}) to spawn start boat index {i}.");
                break;
            }

            if (dockPointsManager.CitizenBoatDocks == null || dockPointsManager.CitizenBoatDocks.Count <= i) {
                Debug.LogWarning($"[BoatsLoader] Not enough slots in dockPointsManager.CitizenBoatDocks to spawn start boat index {i}.");
                break;
            }

            var id = boatIds[i];
            var prefab = boatsList.GetBoat(id);
            if (prefab == null) {
                Debug.LogError($"[BoatsLoader] Start boat prefab with ID {id} not found!");
                continue;
            }

            var spawnTransform = pier.PierConstruction.BoatDocks[i];
            if (spawnTransform == null || spawnTransform.DockTransform == null) {
                Debug.LogError($"[BoatsLoader] BoatDock transform at index {i} is null!");
                continue;
            }

            var position = spawnTransform.DockTransform.position;
            var rotation = spawnTransform.DockTransform.rotation.eulerAngles;

            var citizenDock = dockPointsManager.CitizenBoatDocks[i];
            if (citizenDock == null || citizenDock.InstanceId == null) {
                Debug.LogError($"[BoatsLoader] CitizenBoatDock or InstanceId at index {i} is null!");
                continue;
            }

            var dockId = citizenDock.InstanceId.GetGuid();

            var boatData = new BoatData()
            {
                Id = id,
                Position = new Vector3Data(position),
                Rotation = new Vector3Data(rotation),
                DockInstanceId = dockId,
                Status = HumanStatusEnum.Citizen
            };

            BoatFactory.CreateBoat(prefab, position, Quaternion.Euler(rotation), boatData);
        }
    }
}