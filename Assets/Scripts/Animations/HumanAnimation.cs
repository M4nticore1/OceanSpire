using UnityEngine;

public class HumanAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Human human;

    private void OnEnable()
    {
        human.onStartedIdle += OnStartedIdle;
        human.onStoppedIdle += OnStoppedIdle;

        human.Movement.onMovementStarted += OnMovementStarted;
        human.Movement.onMovementStopped += OnMovementStopped;

        human.BoatRider.onStartedFloating += OnStartedFloating;
        human.BoatRider.onStoppedFloating += OnStoppedFloating;

        human.InteractComponent.onInteractionStarted += OnInteractionStarted;
        human.InteractComponent.onInteractionStopped += OnInteractionStopped;

        human.AttackComponent.onAttackStarted += OnStartedAttacking;
        human.AttackComponent.onAttackStopped += OnStoppedAttacking;

        human.ReviveComponent.onRevived += OnRevived;
        human.HealthComponent.onDied += OnDied;
    }

    private void OnDisable()
    {
        human.onStartedIdle -= OnStartedIdle;
        human.onStoppedIdle -= OnStoppedIdle;

        human.Movement.onMovementStarted -= OnMovementStarted;
        human.Movement.onMovementStopped -= OnMovementStopped;

        human.Movement.onMovementStarted -= OnMovementStarted;
        human.Movement.onMovementStopped -= OnMovementStopped;

        human.InteractComponent.onInteractionStarted -= OnInteractionStarted;
        human.InteractComponent.onInteractionStopped -= OnInteractionStopped;

        human.AttackComponent.onAttackStarted -= OnStartedAttacking;
        human.AttackComponent.onAttackStopped -= OnStoppedAttacking;

        human.ReviveComponent.onRevived -= OnRevived;
        human.HealthComponent.onDied -= OnDied;
    }

    private void OnStartedIdle()
    {
        animator.SetBool("isIdle", true);
    }

    private void OnStoppedIdle()
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

    private void OnStartedFloating()
    {
        animator.SetBool("isFloating", true);
    }

    private void OnStoppedFloating()
    {
        animator.SetBool("isFloating", false);
    }

    private void OnInteractionStarted()
    {
        switch (human.CurrentStatusEnum) {
            case HumanStatusEnum.Citizen:
                animator.SetBool("isWorking", true);
                break;
            case HumanStatusEnum.Raider:
                animator.SetBool("isRaiding", true);
                break;
        }
    }

    private void OnInteractionStopped()
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