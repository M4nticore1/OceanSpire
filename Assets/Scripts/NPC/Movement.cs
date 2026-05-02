using System;
using UnityEngine;
using UnityEngine.AI;

public enum MovementMethod
{
    Walk,
    Run
}

public class Movement : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navAgent;
    public NavMeshAgent NavAgent => navAgent;

    public MovementMethod currentMovementMethod { get; private set; }

    [Header("Speed")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    public bool IsMoving { get; private set; } = false;

    public event Action onMovementStarted;
    public event Action onMovementStopped;
    public event Action onReachedPath;

    private void Update()
    {
        if (CheckDistancePathPosition()) {
            OnReachedPath();
        }
    }

    public void Move(Vector3 direction, float speed)
    {
        transform.position += direction * speed;
    }

    public bool TryMoveTo(Vector3 position)
    {
        if (!CanMove()) return false;

        navAgent.isStopped = false;

        if (navAgent.SetDestination(position)) {
            IsMoving = true;
            onMovementStarted?.Invoke();

            return true;
        }

        return false;
    }

    public void StopMoving()
    {
        if (!IsMoving) return;

        navAgent.isStopped = true;
        navAgent.ResetPath();
        IsMoving = false;
        onMovementStopped?.Invoke();
    }

    public void SetMovementMethod(MovementMethod method)
    {
        if (!navAgent) return;

        currentMovementMethod = method;

        switch (method) {
            case MovementMethod.Walk:
                navAgent.speed = walkSpeed;
                break;
            case MovementMethod.Run:
                navAgent.speed = runSpeed;
                break;
        }
    }

    public void SetAgentEnabled(bool enabled)
    {
        navAgent.enabled = enabled;
    }

    private void OnReachedPath()
    {
        StopMoving();
        onReachedPath?.Invoke();
    }

    private bool CanMove()
    {
        return navAgent.enabled;
    }

    private bool CheckDistancePathPosition()
    {
        if (!IsMoving) return false;
        if (!navAgent.enabled) return false;
        if (navAgent.pathPending) return false;

        if (navAgent.pathStatus != NavMeshPathStatus.PathComplete)
            return false;

        if (navAgent.remainingDistance > navAgent.stoppingDistance)
            return false;

        return true;
    }
}