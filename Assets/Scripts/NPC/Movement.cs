using System;
using TMPro;
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

    private Vector3 targetPosition;

    public MovementMethod currentMovementMethod { get; private set; }

    [Header("Speed")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    public bool IsMoving { get; private set; } = false;

    public event Action OnMovementStarted;
    public event Action OnMovementStopped;
    public event Action OnReachedPath;

    private void Update()
    {
        if (!CheckDistancePathPosition()) return;

        HandleReachedPath();
    }

    public void Move(Vector3 direction, float speed)
    {
        transform.position += direction * speed;
    }

    public bool TryMoveTo(Vector3 position)
    {
        if (!CanMove()) return false;

        targetPosition = position;
        navAgent.isStopped = false;

        if (!navAgent.SetDestination(position)) return false;

        IsMoving = true;
        OnMovementStarted?.Invoke();

        return true;
    }

    public void StopMoving()
    {
        if (!IsMoving) return;

        navAgent.isStopped = true;
        navAgent.ResetPath();
        IsMoving = false;
        OnMovementStopped?.Invoke();
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

    public bool CanMove()
    {
        if (!navAgent.enabled) return false;
        if (!navAgent.isOnNavMesh) return false;

        return true;
    }

    private void HandleReachedPath()
    {
        StopMoving();
        OnReachedPath?.Invoke();
    }

    private bool CheckDistancePathPosition()
    {
        if (!IsMoving) return false;
        if (!navAgent.enabled) return false;
        if (navAgent.pathPending) return false;

        if (navAgent.pathStatus != NavMeshPathStatus.PathComplete)
            return false;

        if (Vector3.Distance(transform.position, targetPosition) > navAgent.stoppingDistance)
            return false;

        return true;
    }
}