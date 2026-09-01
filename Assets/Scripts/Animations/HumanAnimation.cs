using System.Collections;
using UnityEngine;

public class HumanAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Human human;

    private AnimationParam interactionAnimationParam;

    private Coroutine updateParametersCoroutine;

    private void OnEnable()
    {
        human.OnIdleStarted += HandleIdleStarted;
        human.OnIdleStopped += HandleIdleStopped;

        human.Movement.OnMovementStarted += HandleMovementStarted;
        human.Movement.OnMovementStopped += HandleMovementStopped;

        human.BoatRider.OnBoatMovementStarted += HandleBoatMovementStarted;
        human.BoatRider.OnBoatMovementStopped += HandleBoatMovementStopped;

        human.InteractComponent.OnInteractionStarted += HandleInteractionStarted;
        human.InteractComponent.OnInteractionStopped += HandleInteractionStopped;

        human.AttackComponent.OnAttackStarted += HandleStartedAttacking;
        human.AttackComponent.OnAttackStopped += HandleStoppedAttacking;

        human.ReviveComponent.OnRevived += HandleRevived;
        human.HealthComponent.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        human.OnIdleStarted -= HandleIdleStarted;
        human.OnIdleStopped -= HandleIdleStopped;

        human.Movement.OnMovementStarted -= HandleMovementStarted;
        human.Movement.OnMovementStopped -= HandleMovementStopped;

        human.BoatRider.OnBoatMovementStarted -= HandleBoatMovementStarted;
        human.BoatRider.OnBoatMovementStopped -= HandleBoatMovementStopped;

        human.InteractComponent.OnInteractionStarted -= HandleInteractionStarted;
        human.InteractComponent.OnInteractionStopped -= HandleInteractionStopped;

        human.AttackComponent.OnAttackStarted -= HandleStartedAttacking;
        human.AttackComponent.OnAttackStopped -= HandleStoppedAttacking;

        human.ReviveComponent.OnRevived -= HandleRevived;
        human.HealthComponent.OnDied -= HandleDied;
    }

    private void RunUpdateParametersCoroutine()
    {
        if (updateParametersCoroutine == null) {
            updateParametersCoroutine = StartCoroutine(UpdateParametersCoroutine());
        }
    }

    private void UpdateParameters()
    {
        var isAnimating = false;

        if (UpdateIdle()) {
            isAnimating = true;
        }
        if (UpdateWalking()) {
            isAnimating = true;
        }
        if (UpdateRunning()) {
            isAnimating = true;
        }
        if (UpdateInteracting()) {
            isAnimating = true;
        }
        if (UpdateFloating()) {
            isAnimating = true;
        }
        if (UpdateDied()) {
            isAnimating = true;
        }
        if (!isAnimating) {
            animator.SetBool("IsIdle", true);
        }
    }

    private bool UpdateIdle()
    {
        var value = human.IsIdle;
        animator.SetBool("IsIdle", value);
        return value;
    }

    private bool UpdateWalking()
    {
        var value = human.Movement.IsMoving && human.Movement.CurrentMovementMethod == MovementMethod.Walk;
        animator.SetBool("IsWalking", value);
        return value;
    }

    private bool UpdateRunning()
    {
        var value = human.Movement.IsMoving && human.Movement.CurrentMovementMethod == MovementMethod.Run;
        animator.SetBool("IsRunning", value);
        return value;
    }

    private bool UpdateInteracting()
    {
        if (human.InteractComponent.IsInteracting) {
            var cityNavigator = human.CityNavigator;
            var waypoint = cityNavigator.WaypointsComponent.GetCurrentWaypoint();
            var animation = waypoint?.ActionAnimation;
            var paramName = animation?.ParamName;

            var value = human.InteractComponent.IsInteracting && !human.Movement.IsMoving;
            animator.SetBool(string.IsNullOrEmpty(paramName) ? "IsWorking" : paramName, human.InteractComponent.IsInteracting && !human.Movement.IsMoving);
            interactionAnimationParam = animation;
            return value;
        }
        else {
            var paramName = interactionAnimationParam?.ParamName;
            animator.SetBool(interactionAnimationParam ? paramName : "IsWorking", false);
            return false;
        }
    }

    private bool UpdateFloating()
    {
        var ridingBoat = human.BoatRider.RidingBoat;
        var value = ridingBoat && ridingBoat.Movement.IsMoving && human.HealthComponent.IsAlive;
        animator.SetBool("IsFloating", value);
        return value;
    }

    private bool UpdateDied()
    {
        var value = !human.HealthComponent.IsAlive;
        animator.SetBool("IsDied", value);
        return value;
    }

    private void HandleIdleStarted()
    {
        RunUpdateParametersCoroutine();
    }

    private void HandleIdleStopped()
    {
        RunUpdateParametersCoroutine();
    }

    private void HandleMovementStarted()
    {
        RunUpdateParametersCoroutine();
    }

    private void HandleMovementStopped()
    {
        RunUpdateParametersCoroutine();
    }

    private void HandleBoatMovementStarted(Boat boat)
    {
        RunUpdateParametersCoroutine();
    }

    private void HandleBoatMovementStopped(Boat boat)
    {
        RunUpdateParametersCoroutine();
    }

    private void HandleInteractionStarted(Building building)
    {
        RunUpdateParametersCoroutine();
    }

    private void HandleInteractionStopped(Building building)
    {
        RunUpdateParametersCoroutine();
    }

    private void HandleStartedAttacking(AttackComponent attackComponent)
    {
        var weaponDefinition = human.WeaponComponent.EquipmentDefinition as WeaponDefinition;
        if (!weaponDefinition) {
            Debug.LogError("Weapon Definition is not valid");
            return;
        }

        var animationName = weaponDefinition.AttackMethod == AttackMethod.Hands ? "IsAttackingHands" :
            weaponDefinition.AttackMethod == AttackMethod.Light ? "IsAttackingLight" :
            weaponDefinition.AttackMethod == AttackMethod.Heavy ? "IsAttackingHeavy" :
            "";

        animator.SetBool(animationName, true);
    }

    private void HandleStoppedAttacking(AttackComponent attackComponent)
    {
        animator.SetBool("IsAttackingHands", false);
        animator.SetBool("IsAttackingLight", false);
        animator.SetBool("IsAttackingHeavy", false);
    }

    private void HandleRevived()
    {
        RunUpdateParametersCoroutine();
    }

    private void HandleDied()
    {
        RunUpdateParametersCoroutine();
    }

    private IEnumerator UpdateParametersCoroutine()
    {
        yield return new WaitForEndOfFrame();

        updateParametersCoroutine = null;
        UpdateParameters();
    }
}