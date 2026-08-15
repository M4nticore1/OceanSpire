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
        UpdateIdle();
        UpdateWalking();
        UpdateRunning();
        UpdateInteracting();
        UpdateFloating();
        UpdateDied();
    }

    private void UpdateIdle()
    {
        animator.SetBool("IsIdle", human.IsIdle);
    }

    private void UpdateWalking()
    {
        animator.SetBool("IsWalking", human.Movement.IsMoving && human.Movement.CurrentMovementMethod == MovementMethod.Walk);
    }

    private void UpdateRunning()
    {
        animator.SetBool("IsRunning", human.Movement.IsMoving && human.Movement.CurrentMovementMethod == MovementMethod.Run);
    }

    private void UpdateInteracting()
    {
        if (human.InteractComponent.IsInteracting) {
            var cityNavigator = human.CityNavigator;
            var waypoint = cityNavigator.WaypointsComponent.GetCurrentWaypoint();
            var animation = waypoint?.ActionAnimation;
            var paramName = animation?.ParamName;

            animator.SetBool(string.IsNullOrEmpty(paramName) ? "IsWorking" : paramName, human.InteractComponent.IsInteracting && !human.Movement.IsMoving);
            interactionAnimationParam = animation;
        }
        else {
            var paramName = interactionAnimationParam?.ParamName;
            animator.SetBool(interactionAnimationParam ? paramName : "IsWorking", false);
        }
    }

    private void UpdateFloating()
    {
        var ridingBoat = human.BoatRider.RidingBoat;
        animator.SetBool("IsFloating", ridingBoat && ridingBoat.Movement.IsMoving && human.HealthComponent.IsAlive);
    }

    private void UpdateDied()
    {
        animator.SetBool("IsDied", !human.HealthComponent.IsAlive);
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
        UpdateParameters();

        updateParametersCoroutine = null;
    }
}