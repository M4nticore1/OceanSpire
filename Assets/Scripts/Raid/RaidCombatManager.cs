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
        UpdateHumanCombat(human);
    }

    private void HandleHumanCombatStopped(Human human, AttackComponent combatComponent)
    {
        UpdateHumanCombat(human);
    }

    private void UpdateHumanCombat(Human human)
    {
        if (human == null) return;

        var building = human.CityNavigator.EnteredBuilding;
        if (building == null) return;

        if (human is Citizen citizen) {
            AssignCombatTarget(citizen, building.RaidersHandler);
        }
        else if (human is Raider raider) {
            AssignCombatTarget(raider, building.CitizensHandler);
        }
    }

    private void AssignCombatTarget(Human human, BuildingInteractorsHandler targetsHandler)
    {
        if (human == null) return;
        if (targetsHandler == null) return;

        var attackComponent = human.AttackComponent;
        if (attackComponent == null) return;

        if (attackComponent.CurrentTarget != null) return;

        var bestTarget = GetNearestFreeTarget(human.transform.position, targetsHandler);
        if (bestTarget == null) return;

        attackComponent.SetTarget(bestTarget.AttackComponent);
    }

    private Human GetNearestFreeTarget(Vector3 currentPos, BuildingInteractorsHandler buildingInteractors)
    {
        if (buildingInteractors == null) return null;
        if (buildingInteractors.CurrentInteractors == null) return null;

        Human nearestTarget = null;
        float minSqDistance = float.MaxValue;

        foreach (var interactor in buildingInteractors.CurrentInteractors) {
            if (interactor == null) continue;
            if (interactor.AttackComponent != null && interactor.AttackComponent.CurrentTarget != null) continue;

            float sqDist = (interactor.transform.position - currentPos).sqrMagnitude;
            if (sqDist < minSqDistance) {
                minSqDistance = sqDist;
                nearestTarget = interactor;
            }
        }

        return nearestTarget;
    }
}