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

    protected override void Subscribe()
    {
        base.Subscribe();

        OwnedBuilding.UpgradeComponent.OnUpgradeStarted += OnUpgradeStarted;
        OwnedBuilding.UpgradeComponent.OnUpgradeFinished += OnUpgradeCompleted;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        OwnedBuilding.UpgradeComponent.OnUpgradeStarted -= OnUpgradeStarted;
        OwnedBuilding.UpgradeComponent.OnUpgradeFinished -= OnUpgradeCompleted;
    }

    private void OnUpgradeStarted()
    {
        if (!docksLoader.IsLoaded) return;

        UpdateBoatDocks();
    }

    private void OnUpgradeCompleted()
    {
        if (!boatsLoader.IsLoaded) return;
        if (!docksLoader.IsLoaded) return;

        CreateBoats();
        UpdateBoatDocks();
        UpdateBoatPositions();
    }

    private void CreateBoats()
    {
        Debug.Log(OwnedBuilding.SpawnedConstruction);
        if (!boatsLoader.IsLoaded) return;

        Debug.Log(PierConstruction.BoatDocks.Count);
        Debug.Log(boatsManager.CitizenBoats.Count);
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
                Status = HumanStatusEnum.Citizen
            };

            var boat = BoatFactory.CreateBoat(citizenBoatPrefab, boatData);
        }
    }

    private void UpdateBoatDocks()
    {
        var boats = boatsManager.CitizenBoats;
        Debug.Log(boats.Count);

        for (int i = 0; i < boats.Count; i++) {
            var boat = boats[i];
            if (boat == null) {
                Debug.LogError($"[{nameof(PierModule)}] Boat is not valid by index {i}");
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