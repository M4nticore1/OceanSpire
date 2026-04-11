using UnityEngine;

public class HumanAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Human human;

    private void OnEnable()
    {
        human.onStartedIdle += OnStartedIdle;
        human.onStoppedIdle += OnStoppedIdle;

        human.Movement.onStartedMoving += OnStartedMoving;
        human.Movement.onStoppedMoving += OnStoppedMoving;

        human.BoatRider.onStartedFloating += OnStartedFloating;
        human.BoatRider.onStoppedFloating += OnStoppedFloating;

        human.Interactor.onStartedInteracting += OnStartedInteracting;
        human.Interactor.onStoppedInteracting += OnStoppedInteracting;

        human.Attack.onStartedAttacking += OnStartedAttacking;
        human.Attack.onStoppedAttacking += OnStoppedAttacking;

        human.Health.onRevived += OnRevived;
        human.Health.onDied += OnDied;
    }

    private void OnDisable()
    {
        human.onStartedIdle -= OnStartedIdle;
        human.onStoppedIdle -= OnStoppedIdle;

        human.Movement.onStartedMoving -= OnStartedMoving;
        human.Movement.onStoppedMoving -= OnStoppedMoving;

        human.Movement.onStartedMoving -= OnStartedMoving;
        human.Movement.onStoppedMoving -= OnStoppedMoving;

        human.Interactor.onStartedInteracting -= OnStartedInteracting;
        human.Interactor.onStoppedInteracting -= OnStoppedInteracting;

        human.Attack.onStartedAttacking -= OnStartedAttacking;
        human.Attack.onStoppedAttacking -= OnStoppedAttacking;

        human.Health.onRevived -= OnRevived;
        human.Health.onDied -= OnDied;
    }

    private void OnStartedIdle()
    {
        animator.SetBool("isIdle", true);
    }

    private void OnStoppedIdle()
    {
        animator.SetBool("isIdle", false);
    }

    private void OnStartedMoving()
    {
        switch (human.Movement.currentMovementMethod) {
            case MovementMethod.Walk:
                animator.SetBool("isWalking", true);
                break;
            case MovementMethod.Run:
                animator.SetBool("isRunning", true);
                break;
        }
    }

    private void OnStoppedMoving()
    {
        switch (human.Movement.currentMovementMethod) {
            case MovementMethod.Walk:
                animator.SetBool("isWalking", false);
                break;
            case MovementMethod.Run:
                animator.SetBool("isRunning", false);
                break;
        }
    }

    private void OnStartedFloating()
    {
        animator.SetBool("isFloating", true);
    }

    private void OnStoppedFloating()
    {
        animator.SetBool("isFloating", false);
    }

    private void OnStartedInteracting()
    {
        switch (human.currentStatusEnum) {
            case HumanStatusEnum.Citizen:
                animator.SetBool("isWorking", true);
                break;
            case HumanStatusEnum.Raider:
                animator.SetBool("isRaiding", true);
                break;
        }
    }

    private void OnStoppedInteracting()
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