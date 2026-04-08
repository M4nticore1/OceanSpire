using UnityEngine;

public class HumanAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Human human;

    private void OnEnable()
    {
        human.Movement.onStartedMoving += OnStartedMoving;
        human.Movement.onStoppedMoving += OnStoppedMoving;

        human.Attack.onStartedAttacking += OnStartedAttacking;
        human.Attack.onStoppedAttacking += OnStoppedAttacking;

        human.Health.onRevived += OnRevived;
        human.Health.onDied += OnDied;
    }

    private void OnDisable()
    {
        human.Movement.onStartedMoving -= OnStartedMoving;
        human.Movement.onStoppedMoving -= OnStoppedMoving;

        human.Attack.onStartedAttacking -= OnStartedAttacking;
        human.Attack.onStoppedAttacking -= OnStoppedAttacking;

        human.Health.onRevived -= OnRevived;
        human.Health.onDied -= OnDied;
    }

    private void OnStartedMoving()
    {
        animator.SetBool("isMoving", true);
    }

    private void OnStoppedMoving()
    {
        animator.SetBool("isMoving", false);
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