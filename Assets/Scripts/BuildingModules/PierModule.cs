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

        UpdateBoatDocks();
        CreateBoats();
        UpdateBoatPositions();
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
        var boats = boatsManager.CitizenBoatsDict.Values.ToList();

        for (int i = 0; i < boats.Count; i++) {
            var boat = boats[i];
            if (!boat) {
                Debug.LogError($"[{nameof(PierModule)}] Boat is not valid by index {i}");
                continue;
            }

            var dockPoint = PierConstruction.BoatDocks[i];
            if (!dockPoint) {
                Debug.LogError($"[{nameof(PierModule)}] Dock Point is not valid by index {i}");
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

            if (boat.CurrentStateEnum == BoatStateEnum.Idle || boat.CurrentStateEnum == BoatStateEnum.UnloadingLoot) {
                var dockPoint = boat.DockPoint;
                if (!dockPoint) {
                    Debug.LogError($"dockPoint is not valid by index {i}");
                    continue;
                }

                boat.Movement.NavAgent.Warp(dockPoint.DockTransform.position);
            }
        }
    }
}