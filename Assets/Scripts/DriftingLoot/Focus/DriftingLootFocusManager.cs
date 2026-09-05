using System.Collections.Generic;
using UnityEngine;

public class DriftingLootFocusManager : MonoBehaviour
{
    public static DriftingLootFocusManager Instance { get; private set; }

    [Header("Main")]
    [SerializeField] private FocusPointer focusPointerPrefab;
    [SerializeField] private FocusManager focusManager;
    [SerializeField] private BoatsManager boatsManager;
    [SerializeField] private BoatDocksManager boatDocksManager;

    [Header("Other")]
    [SerializeField] private float maxClusterDistance = 10f;

    private readonly List<SwimmingDriftingLoot> focusedDriftingLoot = new();
    public IReadOnlyList<SwimmingDriftingLoot> FocusedDriftingLoot => focusedDriftingLoot;

    private void Awake()
    {
        if (Instance) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        FocusComponent.OnComponentFocusedChanged += OnComponentFocusedChanged;
        DriftingLoot.OnLootDestroyed += OnDriftingLootDestroyed;
    }

    private void OnDisable()
    {
        FocusComponent.OnComponentFocusedChanged -= OnComponentFocusedChanged;
        DriftingLoot.OnLootDestroyed -= OnDriftingLootDestroyed;
    }

    public SwimmingDriftingLoot GetNearestAvailableFocusedDriftingLoot(Boat boat)
    {
        if (boat == null) return null;

        SwimmingDriftingLoot bestLoot = null;
        var bestSqrDistance = float.MaxValue;

        for (int i = focusedDriftingLoot.Count - 1; i >= 0; i--) {
            var loot = focusedDriftingLoot[i];

            if (loot == null) {
                focusedDriftingLoot.RemoveAt(i);
                continue;
            }

            if (loot.FocusComponent == null || !loot.FocusComponent.IsFocused) {
                focusedDriftingLoot.RemoveAt(i);
                continue;
            }

            if (!boat.ShouldSetTargetLoot(loot))
                continue;

            if (!CanTargetLoot(boat, loot))
                continue;

            var sqrDistance =
                (loot.transform.position - boat.transform.position).sqrMagnitude;

            if (sqrDistance < bestSqrDistance) {
                bestSqrDistance = sqrDistance;
                bestLoot = loot;
            }
        }

        return bestLoot;
    }

    private void AddFocusedDriftingLoot(SwimmingDriftingLoot driftingLoot)
    {
        if (driftingLoot == null) return;
        if (focusedDriftingLoot.Contains(driftingLoot)) return;
        if (!driftingLoot.FocusComponent.IsFocused) return;

        focusedDriftingLoot.Add(driftingLoot);
    }

    private void RemoveFocusedDriftingLoot(SwimmingDriftingLoot driftingLoot)
    {
        if (driftingLoot == null) return;
        focusedDriftingLoot.Remove(driftingLoot);
    }

    private void TryUnfocusExtraLoot(SwimmingDriftingLoot newFocusedDriftingLoot)
    {
        if (newFocusedDriftingLoot == null) return;
        if (focusedDriftingLoot.Count <= GetMaxPointersCount()) return;

        var nearestDriftingLoot = GetNearestDriftingLoot(newFocusedDriftingLoot);

        if (nearestDriftingLoot) {
            var maxClusterDistance = this.maxClusterDistance * this.maxClusterDistance;
            var lootSqrDistance = (nearestDriftingLoot.transform.position - newFocusedDriftingLoot.transform.position).sqrMagnitude;

            if (lootSqrDistance <= maxClusterDistance) {
                nearestDriftingLoot.FocusComponent.SetFocused(false);
                return;
            }
        }

        if (focusedDriftingLoot.Count > 0) {
            var firstPointer = focusedDriftingLoot[0];
            if (firstPointer && firstPointer != newFocusedDriftingLoot) {
                firstPointer.FocusComponent.SetFocused(false);
            }
        }
    }

    private void OnComponentFocusedChanged(FocusComponent focusComponent, bool focused)
    {
        if (focusComponent == null) return;

        var driftingLoot = focusComponent.GetComponent<SwimmingDriftingLoot>();
        if (driftingLoot == null) return;

        if (focusComponent.IsFocused) {
            AddFocusedDriftingLoot(driftingLoot);
            TryUnfocusExtraLoot(driftingLoot);
        }
        else {
            RemoveFocusedDriftingLoot(driftingLoot);
        }
    }

    private void OnDriftingLootDestroyed(DriftingLoot driftingLoot)
    {
        if (driftingLoot == null) return;

        for (int i = focusedDriftingLoot.Count - 1; i >= 0; i--) {
            if (focusedDriftingLoot[i] && !focusedDriftingLoot[i].Equals(driftingLoot)) continue;

            focusedDriftingLoot.RemoveAt(i);
        }
    }

    private SwimmingDriftingLoot GetNearestDriftingLoot(SwimmingDriftingLoot startLoot)
    {
        SwimmingDriftingLoot bestLoot = null;
        var bestSqrDistance = float.MaxValue;
        var position = startLoot.transform.position;

        foreach (var loot in focusedDriftingLoot) {
            if (loot == null) continue;
            if (loot == startLoot) continue;

            var sqrDistance = (loot.transform.position - position).sqrMagnitude;
            if (sqrDistance < bestSqrDistance) {
                bestSqrDistance = sqrDistance;
                bestLoot = loot;
            }
        }

        return bestLoot;
    }

    private int GetMaxPointersCount()
    {
        if (boatsManager == null) return 1;
        if (boatsManager.CitizenBoats == null) return 1;

        var count = 0;
        foreach (var boat in boatsManager.CitizenBoats) {
            if (boat == null) continue;

            var dockPoint = boat.DockPoint;
            if (dockPoint == null) continue;

            if (!boatDocksManager.CitizenBoatDocks.Contains(dockPoint)) continue;

            count++;
        }

        return count;
    }

    private bool CanTargetLoot(Boat boat, SwimmingDriftingLoot loot)
    {
        var targetBoat = loot.TargetBoat;

        // Никто не таргетит
        if (targetBoat == null)
            return true;

        // Уже наш таргет
        if (targetBoat == boat)
            return true;

        // Другая лодка ближе — нам нельзя забирать
        var ourSqrDistance =
            (loot.transform.position - boat.transform.position).sqrMagnitude;

        var otherSqrDistance =
            (loot.transform.position - targetBoat.transform.position).sqrMagnitude;

        return ourSqrDistance < otherSqrDistance;
    }
}