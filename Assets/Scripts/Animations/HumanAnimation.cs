using UnityEngine;

public class HumanAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Human human;

    private void OnEnable()
    {
        human.onStartedIdle += OnStartedIdle;
        human.onStoppedIdle += OnStoppedIdle;

        human.Movement.OnMovementStarted += OnMovementStarted;
        human.Movement.OnMovementStopped += OnMovementStopped;

        human.BoatRider.OnBoatMovementStarted += OnBoatMovementStarted;
        human.BoatRider.OnBoatMovementStopped += OnBoatMovementStopped;

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

        human.Movement.OnMovementStarted -= OnMovementStarted;
        human.Movement.OnMovementStopped -= OnMovementStopped;

        human.Movement.OnMovementStarted -= OnMovementStarted;
        human.Movement.OnMovementStopped -= OnMovementStopped;

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