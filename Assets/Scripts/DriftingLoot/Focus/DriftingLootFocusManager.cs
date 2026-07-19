using System.Collections.Generic;
using UnityEngine;

public class DriftingLootFocusManager : MonoBehaviour
{
    public static DriftingLootFocusManager Instance { get; private set; }

    [Header("Main")]
    [SerializeField] private FocusPointer focusPointerPrefab;
    [SerializeField] private FocusManager focusManager;
    [SerializeField] private BoatsManager boatsManager;

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
        FocusComponent.OnFocusedChanged += OnComponentFocusedChanged;
        DriftingLoot.OnLootDestroyed += OnDriftingLootDestroyed;
    }

    private void OnDisable()
    {
        FocusComponent.OnFocusedChanged -= OnComponentFocusedChanged;
        DriftingLoot.OnLootDestroyed -= OnDriftingLootDestroyed;
    }

    public SwimmingDriftingLoot GetNearestAvaliableFocusedDriftingLoot(Boat boat)
    {
        SwimmingDriftingLoot bestLoot = null;
        float bestSqrDistance = float.MaxValue;

        for (int i = focusedDriftingLoot.Count - 1; i >= 0; i--) {
            var loot = focusedDriftingLoot[i];

            if (!loot) {
                focusedDriftingLoot.RemoveAt(i);
                continue;
            }

            if (!loot.FocusComponent || !loot.FocusComponent.IsFocused) {
                Debug.LogError($"[{nameof(DriftingLootFocusManager)}] Loot found in list but it's not focused. Removing.");
                focusedDriftingLoot.RemoveAt(i);
                continue;
            }

            var currentBoatSqrDistance = (loot.transform.position - boat.transform.position).sqrMagnitude;
            if (loot.TargetBoat) {
                var targetBoatSqrDistance = (loot.transform.position - loot.TargetBoat.transform.position).sqrMagnitude;
                if (currentBoatSqrDistance > targetBoatSqrDistance) continue;
            }

            if (currentBoatSqrDistance < bestSqrDistance) {
                bestSqrDistance = currentBoatSqrDistance;
                bestLoot = loot;
            }
        }

        return bestLoot;
    }

    private void AddFocusedDriftingLoot(SwimmingDriftingLoot driftingLoot)
    {
        if (!driftingLoot) return;
        if (focusedDriftingLoot.Contains(driftingLoot)) return;
        if (!driftingLoot.FocusComponent.IsFocused) return;

        focusedDriftingLoot.Add(driftingLoot);
    }

    private void RemoveFocusedDriftingLoot(SwimmingDriftingLoot driftingLoot)
    {
        if (!driftingLoot) return;
        focusedDriftingLoot.Remove(driftingLoot);
    }

    private void TryUnfocusExtraLoot(SwimmingDriftingLoot newFocusedDriftingLoot)
    {
        if (!newFocusedDriftingLoot) return;
        if (focusedDriftingLoot.Count <= GetMaxPointersCount()) return;

        var nearestDriftingLoot = GetNearestDriftingLoot(newFocusedDriftingLoot);

        if (nearestDriftingLoot) {
            float sqrMaxDistance = maxClusterDistance * maxClusterDistance;
            float currentSqrDistance = (nearestDriftingLoot.transform.position - newFocusedDriftingLoot.transform.position).sqrMagnitude;

            if (currentSqrDistance <= sqrMaxDistance) {
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

    private void OnComponentFocusedChanged(FocusComponent focusComponent)
    {
        if (!focusComponent) return;

        var driftingLoot = focusComponent.GetComponent<SwimmingDriftingLoot>();
        if (!driftingLoot) return;

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
        if (!driftingLoot) return;

        for (int i = focusedDriftingLoot.Count - 1; i >= 0; i--) {
            if (focusedDriftingLoot[i] && !focusedDriftingLoot[i].Equals(driftingLoot)) continue;

            focusedDriftingLoot.RemoveAt(i);
        }
    }

    private SwimmingDriftingLoot GetNearestDriftingLoot(SwimmingDriftingLoot startLoot)
    {
        SwimmingDriftingLoot bestLoot = null;
        float bestSqrDistance = float.MaxValue;
        var position = startLoot.transform.position;

        foreach (var loot in focusedDriftingLoot) {
            if (!loot) continue;
            if (loot == startLoot) continue;

            float sqrDistance = (loot.transform.position - position).sqrMagnitude;

            if (sqrDistance < bestSqrDistance) {
                bestSqrDistance = sqrDistance;
                bestLoot = loot;
            }
        }

        return bestLoot;
    }

    private int GetMaxPointersCount()
    {
        if (!boatsManager) return 1;
        if (boatsManager.CitizenBoats == null) return 1;

        return boatsManager.CitizenBoats.Count;
    }
}