using System.Collections;
using System.Linq;
using UnityEngine;

public class PierModule : BuildingModule
{
    public PierConstruction PierConstruction => OwnedBuilding.SpawnedConstruction as PierConstruction;

    [SerializeField] private BoatsLoader boatsLoader;
    [SerializeField] private DockPointsLoader docksLoader;

    [SerializeField] private BoatsManager boatsManager;
    [SerializeField] private DockPointsManager boatDocksManager;
    [SerializeField] private Boat citizenBoatPrefab;

    protected override void Subscribe()
    {
        base.Subscribe();

        OwnedBuilding.ConstructionComponent.OnConstructionStarted += OnConstructionStarted;
        OwnedBuilding.LevelComponent.OnLevelChanged += OnLevelChanged;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        OwnedBuilding.OnInited -= OnConstructionStarted;
        OwnedBuilding.ConstructionComponent.OnConstructionStarted -= OnConstructionStarted;
        OwnedBuilding.LevelComponent.OnLevelChanged -= OnLevelChanged;
    }

    protected override void OnInited()
    {
        base.OnInited();

        RegisterBoatDocks();
        UpdatePier();
    }

    private void OnConstructionStarted()
    {
        UpdatePier();
    }

    private void OnLevelChanged()
    {
        UpdatePier();
    }

    private void UpdatePier()
    {
        if (!boatsLoader.IsLoaded) return;
        if (!docksLoader.IsLoaded) return;

        UnregisterBoatDocks();
        RegisterBoatDocks();
        InitBoatDocks();
        CreateBoats();
        InitBoats();
    }

    private void RegisterBoatDocks()
    {
        foreach (var dock in PierConstruction.BoatDocks) {
            boatDocksManager.RegisterCitizenDockPoint(dock);
        }
    }

    private void UnregisterBoatDocks()
    {
        for (int i = boatDocksManager.CitizenBoatDocks.Count - 1; i >= 0; i--) {
            var dock = boatDocksManager.CitizenBoatDocks[i];
            boatDocksManager.UnregisterCitizenDockPoint(dock);
        }
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
                InstanceId = InstancesManager.Instance.GetNextInstanceId(),
                Position = new Vector3Data(position),
                Rotation = new Vector3Data(rotation.eulerAngles),
                DockInstanceId = boatDock.InstanceId.GetInstanceId(),
                Status = HumanStatusEnum.Citizen
            };

            var boat = BoatFactory.CreateBoat(citizenBoatPrefab, position, rotation, boatData);
        }
    }

    private void InitBoats()
    {
        var boats = boatsManager.CitizenBoats.Values.ToList();

        for (int i = 0; i < boats.Count; i++) {
            var boat = boats[i];
            var dockPoint = PierConstruction.BoatDocks[i];

            boat.SetDockPoint(dockPoint);
            boat.Movement.NavAgent.Warp(dockPoint.DockTransform.position);
        }
    }
}