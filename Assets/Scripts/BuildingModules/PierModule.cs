using System.Collections;
using System.Linq;
using UnityEngine;

public class PierModule : BuildingModule
{
    public PierLevelData CurrentPierLevelData => LevelData ? LevelData as PierLevelData : null;
    public PierConstruction PierConstruction => OwnedBuilding.SpawnedConstruction as PierConstruction;

    [SerializeField] private BoatsLoader boatsLoader;
    [SerializeField] private DockPointsLoader docksLoader;

    [SerializeField] private BoatsManager boatsManager;
    [SerializeField] private BoatDocksManager boatDocksManager;
    [SerializeField] private Boat citizenBoatPrefab;

    protected override void Start()
    {
        base.Start();

        CreateBoats();
        UpdateBoatDocks();
        UpdateBoatPositions();
    }

    protected override void Subscribe()
    {
        base.Subscribe();

        OwnedBuilding.OnConstructionChanged += HandleConstructionChanged;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        OwnedBuilding.OnConstructionChanged -= HandleConstructionChanged;
    }

    private void HandleConstructionChanged(BuildingConstruction buildingConstruction)
    {
        if (!boatsLoader.IsLoaded) return;
        if (!docksLoader.IsLoaded) return;

        CreateBoats();
        UpdateBoatDocks();
        UpdateBoatPositions();
    }

    private void CreateBoats()
    {
        if (!boatsLoader.IsLoaded) return;

        int count = PierConstruction.BoatDocks.Count - boatsManager.CitizenBoats.Count;

        for (int i = 0; i < count; i++) {
            var dockIndex = PierConstruction.BoatDocks.Count - count + i;
            var boatDock = PierConstruction.BoatDocks[dockIndex];

            var transform = boatDock.DockTransform;
            var position = transform.position;
            var rotation = transform.rotation;

            var boatData = new BoatData()
            {
                Id = citizenBoatPrefab.Definition.BoatId,
                Position = new Vector3Data(position),
                Rotation = new Vector3Data(rotation.eulerAngles),
                DockInstanceId = boatDock.InstanceId.GetGuid(),
                Status = BoatStatusEnum.Citizen
            };

            var boat = BoatFactory.CreateBoat(citizenBoatPrefab, boatData);
        }
    }

    private void UpdateBoatDocks()
    {
        var boats = boatsManager.CitizenBoats;

        for (int i = 0; i < boats.Count; i++) {
            var boat = boats[i];
            if (boat == null) {
                Debug.LogError($"[{nameof(PierModule)}] Boat is not valid by index {i}");
                continue;
            }

            if (PierConstruction == null) {
                Debug.LogError($"[{nameof(PierModule)}] PierConstruction is not valid!");
                continue;
            }

            var dockPoint = PierConstruction.GetBoatDock(i);
            if (dockPoint == null) {
                Debug.LogError($"[{nameof(PierModule)}] Dock Point is not valid by index {i}");
                continue;
            }

            boat.SetDockPoint(dockPoint);
        }
    }

    private void UpdateBoatPositions()
    {
        var boats = boatsManager.CitizenBoats;

        for (int i = 0; i < boats.Count; i++) {
            var boat = boats[i];
            if (boat == null) {
                Debug.LogError($"[{nameof(PierModule)}] Boat is not valid by index {i}");
                continue;
            }

            var state = boat.CurrentStateEnum;
            if (state == BoatStateEnum.Idle || state == BoatStateEnum.UnloadingLoot || (state == BoatStateEnum.MovingToDock && !boat.CurrentRider)) {
                var dockPoint = boat.DockPoint;
                if (dockPoint == null) {
                    Debug.LogError($"[{nameof(PierModule)}] Dock Point is not valid by index {i}");
                    continue;
                }

                boat.Movement.NavAgent.Warp(dockPoint.DockTransform.position);
                boat.transform.rotation = dockPoint.DockTransform.rotation;
            }
        }
    }
}