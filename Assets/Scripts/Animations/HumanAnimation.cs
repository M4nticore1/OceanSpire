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

        human.BoatRider.selectedBoat.Movement.onStartedMoving += OnStartedFloating;
        human.BoatRider.selectedBoat.Movement.onStartedMoving += OnStoppedFloating;

        human.Interactor.onStartedInteracting += OnStartedWorking;
        human.Interactor.onStoppedInteracting += OnStoppedWorking;

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

        human.BoatRider.selectedBoat.Movement.onStartedMoving -= OnStartedFloating;
        human.BoatRider.selectedBoat.Movement.onStartedMoving -= OnStoppedFloating;

        human.Interactor.onStartedInteracting -= OnStartedWorking;
        human.Interactor.onStoppedInteracting -= OnStoppedWorking;

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
        animator.SetBool("isMoving", true);
    }

    private void OnStoppedMoving()
    {
        animator.SetBool("isMoving", false);
    }

    private void OnStartedFloating()
    {
        animator.SetBool("isFloating", true);
    }

    private void OnStoppedFloating()
    {
        animator.SetBool("isFloating", false);
    }

    private void OnStartedWorking()
    {
        animator.SetBool("isWorking", true);
    }

    private void OnStoppedWorking()
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