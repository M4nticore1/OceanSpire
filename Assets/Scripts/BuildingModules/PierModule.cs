using System.Linq;
using UnityEngine;

public class PierModule : BuildingModule
{
    public PierLevelData CurrentPierLevelData => LevelData ? LevelData as PierLevelData : null;
    public PierConstruction PierConstruction => OwnedBuilding.SpawnedConstruction as PierConstruction;

    [SerializeField] private BoatsLoader boatsLoader;
    [SerializeField] private DockPointsLoader docksLoader;

    [SerializeField] private BoatsManager boatsManager;
    [SerializeField] private DockPointsManager boatDocksManager;
    [SerializeField] private Boat citizenBoatPrefab;

    protected override void Subscribe()
    {
        base.Subscribe();

        OwnedBuilding.UpgradeComponent.OnUpgradeStarted += OnUpgradeStarted;
        OwnedBuilding.UpgradeComponent.OnUpgradeCompleted += OnUpgradeCompleted;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        OwnedBuilding.UpgradeComponent.OnUpgradeStarted -= OnUpgradeStarted;
        OwnedBuilding.UpgradeComponent.OnUpgradeCompleted -= OnUpgradeCompleted;
    }

    private void OnUpgradeStarted()
    {
        if (!docksLoader.IsLoaded) return;

        InitBoatDocks();
        UpdateBoatDocks();
    }

    private void OnUpgradeCompleted()
    {
        if (!boatsLoader.IsLoaded) return;
        if (!docksLoader.IsLoaded) return;

        InitBoatDocks();
        CreateBoats();
        UpdateBoatDocks();
        UpdateBoatPositions();
    }

    private void InitBoatDocks()
    {
        foreach (var boatDock in PierConstruction.BoatDocks) {
            var boatDockData = new BoatDockData()
            {
                InstanceId = InstancesManager.Instance.GetNextInstanceId(),
            };

            boatDock.Init(boatDockData);
        }
    }

    private void CreateBoats()
    {
        int count = PierConstruction.BoatDocks.Count - boatsManager.CitizenBoatsDict.Count;

        for (int i = 0; i < count; i++) {
            var dockIndex = PierConstruction.BoatDocks.Count - count + i;
            var boatDock = PierConstruction.BoatDocks[dockIndex];

            var transform = boatDock.DockTransform;
            var position = transform.position;
            var rotation = transform.rotation;

            var boatData = new BoatData()
            {
                Id = citizenBoatPrefab.Definition.BoatId,
                InstanceId = InstancesManager.Instance.GetNextInstanceId(),
                Position = new Vector3Data(position),
                Rotation = new Vector3Data(rotation.eulerAngles),
                DockInstanceId = boatDock.InstanceId.GetId(),
                Status = HumanStatusEnum.Citizen
            };

            var boat = BoatFactory.CreateBoat(citizenBoatPrefab, position, rotation, boatData);
        }
    }

    private void UpdateBoatDocks()
    {
        var boats = boatsManager.CitizenBoatsDict.Values.ToList();

        for (int i = 0; i < boats.Count; i++) {
            var boat = boats[i];
            if (!boat) {
                Debug.LogError($"boat is not valid by index {i}");
                continue;
            }

            var dockPoint = PierConstruction.BoatDocks[i];
            if (!dockPoint) {
                Debug.LogError($"dockPoint is not valid by index {i}");
                continue;
            }

            boat.SetDockPoint(dockPoint);
        }
    }

    private void UpdateBoatPositions()
    {
        var boats = boatsManager.CitizenBoatsDict.Values.ToList();

        for (int i = 0; i < boats.Count; i++) {
            var boat = boats[i];
            if (!boat) {
                Debug.LogError($"boat is not valid by index {i}");
                continue;
            }

            if (boat.CurrentStateEnum != BoatStateEnum.Idle && boat.CurrentStateEnum != BoatStateEnum.UnloadingLoot) continue;

            var dockPoint = PierConstruction.BoatDocks[i];
            if (!dockPoint) {
                Debug.LogError($"dockPoint is not valid by index {i}");
                continue;
            }

            boat.Movement.NavAgent.Warp(dockPoint.DockTransform.position);
        }
    }
}