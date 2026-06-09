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

    public Vector3 TargetPosition { get; private set; }

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
        if (!IsDestinationReached()) return;

        TryStopMoving();
    }

    public void Move(Vector3 direction, float speed)
    {
        transform.position += direction * speed;
    }

    public bool TryMoveTo(Vector3 position)
    {
        if (!CanStartMoving()) return false;

        Debug.Log("MoveTo");
        TargetPosition = position;

        if (IsDestinationReached()) {
            OnReachedPath?.Invoke();
            return true;
        }

        navAgent.isStopped = false;
        if (!navAgent.SetDestination(position))
            return false;

        IsMoving = true;
        OnMovementStarted?.Invoke();

        return true;
    }

    public bool TryStopMoving()
    {
        if (!CanStopMoving()) return false;

        navAgent.isStopped = true;
        navAgent.ResetPath();
        IsMoving = false;

        OnMovementStopped?.Invoke();
        OnReachedPath?.Invoke();

        return true;
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

    public bool CanStartMoving()
    {
        if (!navAgent.enabled) return false;
        if (!navAgent.isOnNavMesh) return false;

        return true;
    }

    public bool CanStopMoving()
    {
        if (!IsMoving) return false;

        return true;
    }

    public bool IsDestinationReached()
    {
        if (IsReachedPosition(TargetPosition)) return true;
        //if (navAgent.pathStatus == NavMeshPathStatus.PathComplete) return true;

        return false;
    }

    public bool IsReachedPosition(Vector3 position)
    {
        return Vector3.Distance(transform.position, position) <= navAgent.stoppingDistance;
    }

    public float GetTargetPositionDistance()
    {
        return Vector3.Distance(transform.position, TargetPosition);
    }
}