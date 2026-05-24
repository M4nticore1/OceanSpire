using System.Collections;
using System.Linq;
using UnityEngine;

public class PierModule : BuildingModule
{
    public PierConstruction PierConstruction => OwnedBuilding.SpawnedConstruction as PierConstruction;

    [SerializeField] private BoatsManager boatsManager;
    [SerializeField] private Boat citizenBoatPrefab;

    protected override void Subscribe()
    {
        base.Subscribe();

        OwnedBuilding.ConstructionComponent.OnConstructionStarted += OnConstructionStarted;
        OwnedBuilding.UpgradeComponent.OnUpgradeCompleted += OnUpgradeCompleted;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        OwnedBuilding.ConstructionComponent.OnConstructionStarted -= OnConstructionStarted;
        OwnedBuilding.UpgradeComponent.OnUpgradeCompleted -= OnUpgradeCompleted;
    }

    protected override void OnInited()
    {
        base.OnInited();

        foreach (var dockPoint in PierConstruction.BoatDocks) {
            DockPointsManager.Instance.RegisterPierDockPoint(dockPoint);
        }
    }

    private void OnConstructionStarted()
    {
        UpdatePier();
    }

    private void OnUpgradeCompleted()
    {
        UpdatePier();
    }

    private void UpdatePier()
    {
        StartCoroutine(UpdatePierCoroutine());
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
                DockInstanceId = boatDock.InstanceId.Id,
                Status = HumanStatusEnum.Citizen
            };

            var boat = BoatFactory.CreateBoat(citizenBoatPrefab, position, rotation, boatData);
        }
    }

    private void InitBoats()
    {
        for (int i = 0; i < boatsManager.CitizenBoats.Count; i++) {
            var boat = boatsManager.CitizenBoats.Values.ToArray()[i];
            var dockPoint = PierConstruction.BoatDocks[i];

            boat.SetDockPoint(dockPoint);
            boat.Movement.NavAgent.Warp(dockPoint.DockTransform.position);
        }
    }

    private IEnumerator UpdatePierCoroutine()
    {
        yield return new WaitForEndOfFrame();

        InitBoatDocks();
        CreateBoats();
        InitBoats();
    }
}