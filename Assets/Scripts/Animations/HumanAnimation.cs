using UnityEngine;

public class HumanAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Human human;

    private void OnEnable()
    {
        human.OnIdleStarted += OnIdleStarted;
        human.OnIdleStopped += OnIdleStopped;

        human.Movement.OnMovementStarted += OnMovementStarted;
        human.Movement.OnMovementStopped += OnMovementStopped;

        human.BoatRider.OnBoatMovementStarted += OnBoatMovementStarted;
        human.BoatRider.OnBoatMovementStopped += OnBoatMovementStopped;

        human.InteractComponent.OnInteractionStarted += OnInteractionStarted;
        human.InteractComponent.OnInteractionStopped += OnInteractionStopped;

        human.AttackComponent.OnAttackStarted += OnStartedAttacking;
        human.AttackComponent.OnAttackStopped += OnStoppedAttacking;

        human.ReviveComponent.OnRevived += OnRevived;
        human.HealthComponent.OnDied += OnDied;
    }

    private void OnDisable()
    {
        human.OnIdleStarted -= OnIdleStarted;
        human.OnIdleStopped -= OnIdleStopped;

        human.Movement.OnMovementStarted -= OnMovementStarted;
        human.Movement.OnMovementStopped -= OnMovementStopped;

        human.BoatRider.OnBoatMovementStarted -= OnBoatMovementStarted;
        human.BoatRider.OnBoatMovementStopped -= OnBoatMovementStopped;

        human.InteractComponent.OnInteractionStarted -= OnInteractionStarted;
        human.InteractComponent.OnInteractionStopped -= OnInteractionStopped;

        human.AttackComponent.OnAttackStarted -= OnStartedAttacking;
        human.AttackComponent.OnAttackStopped -= OnStoppedAttacking;

        human.ReviveComponent.OnRevived -= OnRevived;
        human.HealthComponent.OnDied -= OnDied;
    }

    private void UpdateParameters()
    {
        UpdateIdle();
        UpdateWalking();
        UpdateRunning();
        UpdateWorking();
        UpdateFloating();
        UpdateDied();
    }

    private void UpdateIdle()
    {
        animator.SetBool("isIdle", human.IsIdle);
    }

    private void UpdateWalking()
    {
        animator.SetBool("isWalking", human.Movement.IsMoving && human.Movement.CurrentMovementMethod == MovementMethod.Walk);
    }

    private void UpdateRunning()
    {
        animator.SetBool("isRunning", human.Movement.IsMoving && human.Movement.CurrentMovementMethod == MovementMethod.Run);
    }

    private void UpdateWorking()
    {
        animator.SetBool("isWorking", human.InteractComponent.IsInteracting && !human.Movement.IsMoving);
    }

    private void UpdateFloating()
    {
        animator.SetBool("isFloating", human.BoatRider.RidingBoat && human.BoatRider.RidingBoat.Movement.IsMoving && human.HealthComponent.IsAlive);
    }

    private void UpdateDied()
    {
        animator.SetBool("isDied", !human.HealthComponent.IsAlive);
    }

    private void OnIdleStarted()
    {
        UpdateParameters();
    }

    private void OnIdleStopped()
    {
        UpdateParameters();
    }

    private void OnMovementStarted()
    {
        UpdateParameters();
    }

    private void OnMovementStopped()
    {
        UpdateParameters();
    }

    private void OnBoatMovementStarted(Boat boat)
    {
        UpdateParameters();
    }

    private void OnBoatMovementStopped(Boat boat)
    {
        UpdateParameters();
    }

    private void OnInteractionStarted(Building building)
    {
        UpdateParameters();
    }

    private void OnInteractionStopped(Building building)
    {
        UpdateParameters();
    }

    private void OnStartedAttacking()
    {
        var weaponDefinition = human.WeaponComponent.EquipmentDefinition as WeaponDefinition;
        if (!weaponDefinition) {
            Debug.LogError("weaponDefinition is not valid");
            return;
        }

        var animationName = weaponDefinition.AttackMethods == AttackMethod.Hands ? "isAttackingHands" :
            weaponDefinition.AttackMethods == AttackMethod.Light ? "isAttackingLight" :
            weaponDefinition.AttackMethods == AttackMethod.Heavy ? "isAttackingHeavy" :
            "";

        animator.SetBool(animationName, true);
    }

    private void OnStoppedAttacking()
    {
        animator.SetBool("isAttackingHands", false);
        animator.SetBool("isAttackingLight", false);
        animator.SetBool("isAttackingHeavy", false);
    }

    private void OnRevived()
    {
        UpdateParameters();
    }

    private void OnDied()
    {
        UpdateParameters();
    }
}