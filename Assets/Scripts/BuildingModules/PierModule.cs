using System.Collections;
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

    public void UpdatePierWorkersBoats()
    {
        foreach (var worker in OwnedBuilding.CitizensHandler.Interactors) {
            if (worker == null) continue;

            var boatRider = worker.GetComponent<BoatRider>();
            if (boatRider == null) return;

            TryAssignFreeBoat(boatRider);
        }
    }

    public void TryAssignFreeBoat(BoatRider boatRider)
    {
        if (boatRider == null) {
            Debug.LogError($"[{nameof(PierBuildingStrategy)}] Boat Rider is not valid!");
            return;
        }

        var boat = GetFirstFreeBoat(boatRider);
        if (boat == null) {
            Debug.LogError($"[{nameof(PierBuildingStrategy)}] Free Boat is not valid for {boatRider}!");
            return;
        }

        boatRider.TrySetTargetBoat(boat);
    }

    public Boat GetFirstFreeBoat(BoatRider boatRider)
    {
        var index = GetFirstFreeBoatIndex(boatRider);
        if (index == null) return null;

        return BoatsManager.Instance.CitizenBoats[index.Value];
    }

    public int? GetFirstFreeBoatIndex(BoatRider boatRider)
    {
        if (boatRider == null) return null;
        var citizenBoats = BoatsManager.Instance.CitizenBoats;

        for (int i = 0; i < citizenBoats.Count; i++) {
            var boat = citizenBoats[i];
            if (boat == null) continue;

            var targetRider = boat.TargetRider;
            var currentRider = boat.CurrentRider;

            if (targetRider != null && targetRider != boatRider) continue;

            if (boat == boatRider.TargetBoat)
                return i;

            if (targetRider == boatRider)
                return i;

            if (targetRider == null && currentRider == null)
                return i;
        }

        for (int i = 0; i < citizenBoats.Count; i++) {
            var boat = citizenBoats[i];
            if (boat == null) continue;

            var targetRider = boat.TargetRider;
            var currentRider = boat.CurrentRider;

            if (targetRider != null && targetRider != boatRider) continue;

            if (currentRider == boatRider && targetRider == null)
                return i;

            if (currentRider == null && targetRider == boatRider)
                return i;

            if (targetRider == null)
                return i;
        }

        return null;
    }
}