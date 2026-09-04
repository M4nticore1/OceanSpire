using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaidCombatManager : MonoBehaviour
{
    private void OnEnable()
    {
        Human.OnHumanEnteredBuilding += HandleHumanEnteredBuilding;
        Human.OnHumanCombatStopped += HandleHumanCombatStopped;
    }

    private void OnDisable()
    {
        Human.OnHumanEnteredBuilding -= HandleHumanEnteredBuilding;
        Human.OnHumanCombatStopped -= HandleHumanCombatStopped;
    }

    private void HandleHumanEnteredBuilding(Human human, Building building)
    {
        StartCoroutine(UpdateCombatDelay(human));
    }

    private void HandleHumanCombatStopped(Human human, AttackComponent combatComponent)
    {
        StartCoroutine(UpdateCombatDelay(human));
    }

    private void UpdateHumanCombat(Human human)
    {
        if (human == null) return;

        var healthComponent = human.HealthComponent;
        if (healthComponent == null) return;
        if (!healthComponent.IsAlive) return;

        var building = human.CityNavigator.EnteredBuilding;
        if (building == null) return;

        if (human is Citizen citizen) {
            AssignCombatTarget(citizen, building.RaidersHandler);
        }
        else if (human is Raider raider) {
            AssignCombatTarget(raider, building.CitizensHandler);
        }
    }

    private void AssignCombatTarget(Citizen citizen, BuildingInteractorsHandler targetsHandler)
    {
        if (citizen == null) return;
        if (targetsHandler == null) return;

        var attackComponent = citizen.AttackComponent;
        if (attackComponent == null) return;

        if (attackComponent.CurrentTarget != null) return;

        var bestTarget = GetNearestFreeTarget(citizen.transform.position, targetsHandler);
        if (bestTarget == null) return;

        attackComponent.SetTarget(bestTarget);
    }

    private void AssignCombatTarget(Raider raider, BuildingInteractorsHandler targetsHandler)
    {
        if (raider == null) return;
        if (targetsHandler == null) return;

        var attackComponent = raider.AttackComponent;
        if (attackComponent == null) return;
        if (attackComponent.CurrentTarget != null) return;

        var bestTarget = GetNearestFreeTarget(raider.transform.position, targetsHandler);
        if (bestTarget == null) return;

        attackComponent.SetTarget(bestTarget);

        foreach (var target in GetAllFreeTargets(targetsHandler)) {
            target.SetTarget(attackComponent);
        }
    }

    private AttackComponent GetNearestFreeTarget(Vector3 currentPos, BuildingInteractorsHandler buildingInteractors)
    {
        if (buildingInteractors == null) return null;
        if (buildingInteractors.CurrentInteractors == null) return null;

        AttackComponent nearestTarget = null;
        float minSqDistance = float.MaxValue;

        foreach (var interactor in buildingInteractors.CurrentInteractors) {
            if (interactor == null) continue;
            if (interactor.AttackComponent != null && interactor.AttackComponent.CurrentTarget != null) continue;

            float sqDist = (interactor.transform.position - currentPos).sqrMagnitude;
            if (sqDist < minSqDistance) {
                minSqDistance = sqDist;
                nearestTarget = interactor.AttackComponent;
            }
        }

        return nearestTarget;
    }

    private List<AttackComponent> GetAllFreeTargets(BuildingInteractorsHandler buildingInteractors)
    {
        if (buildingInteractors == null) return null;
        if (buildingInteractors.CurrentInteractors == null) return null;

        var targets = new List<AttackComponent>();
        foreach (var interactor in buildingInteractors.CurrentInteractors) {
            if (interactor == null) continue;

            var attackComponent = interactor.AttackComponent;
            if (attackComponent == null) continue;
            if (attackComponent.CurrentTarget != null) continue;

            targets.Add(attackComponent);
        }

        return targets;
    }

    private IEnumerator UpdateCombatDelay(Human human)
    {
        yield return null;

        UpdateHumanCombat(human);
    }
}