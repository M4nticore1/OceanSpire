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

    private void DisableAllCondition()
    {
        foreach (var param in animator.parameters) {
            var paramName = param.name;

            var paramType = param.type;
            if (paramType != AnimatorControllerParameterType.Bool) return;

            animator.SetBool(paramName, false);
        }
    }

    private void OnIdleStarted()
    {
        animator.SetBool("isIdle", true);
    }

    private void OnIdleStopped()
    {
        animator.SetBool("isIdle", false);
    }

    private void OnMovementStarted()
    {
        switch (human.Movement.currentMovementMethod) {
            case MovementMethod.Walk:
                animator.SetBool("isWalking", true);
                animator.SetBool("isRunning", false);
                break;
            case MovementMethod.Run:
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", true);
                break;
        }
    }

    private void OnMovementStopped()
    {
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
    }

    private void OnBoatMovementStarted(Boat boat)
    {
        animator.SetBool("isFloating", true);
    }

    private void OnBoatMovementStopped(Boat boat)
    {
        animator.SetBool("isFloating", false);
    }

    private void OnInteractionStarted(Building building)
    {
        if (human.GetComponent<Citizen>()) {
            animator.SetBool("isWorking", true);
        }
        else if (human.GetComponent<Raider>()) {
            animator.SetBool("isRaiding", true);
        }
    }

    private void OnInteractionStopped(Building building)
    {
        animator.SetBool("isWorking", false);
    }

    private void OnStartedAttacking()
    {
        animator.SetBool("isAttacking", true);
    }

    private void OnStoppedAttacking()
    {
        animator.SetBool("isAttacking", false);
    }

    private void OnRevived()
    {
        animator.SetBool("isDied", false);
    }

    private void OnDied()
    {
        animator.SetBool("isDied", true);
    }
}