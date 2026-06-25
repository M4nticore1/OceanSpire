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

    public Vector3 TargetPosition { get; private set; } = Vector3.zero;

    public bool UseTargetRotation { get; private set; } = false;
    public Quaternion TargetRotation { get; private set; } = Quaternion.identity;

    public MovementMethod CurrentMovementMethod { get; private set; }

    [Header("Speed")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    public bool IsMoving { get; private set; } = false;

    public event Action OnMovementStarted;
    public event Action OnMovementStopped;
    public event Action OnReachedDestination;

    private void Update()
    {
        if (!IsMoving) return;
        if (!IsDestinationReached()) return;

        TryStopMoving();
    }

    public void Move(Vector3 direction, float speed)
    {
        transform.position += direction * speed;
    }

    public void SetMovementMethod(MovementMethod method)
    {
        if (!navAgent) return;

        CurrentMovementMethod = method;

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

    public bool TryMoveTo(Transform transform)
    {
        if (!transform) {
            Debug.LogError("Transform is not valid!");
            return false;
        }

        if (!TryMoveTo(transform.position)) return false;

        SetTargetRotation(transform.rotation);
        return true;
    }

    public bool TryMoveTo(Vector3 position)
    {
        if (!CanStartMoving()) return false;

        TargetPosition = position;
        RemoveTargetRotation();

        if (IsDestinationReached()) {
            OnReachedDestination?.Invoke();
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

        var lastIsMoving = IsMoving;
        IsMoving = false;

        if (lastIsMoving) {
            OnMovementStopped?.Invoke();
        }

        if (IsDestinationReached()) {
            OnReachedDestination?.Invoke();
        }

        return true;
    }

    public bool CanStartMoving()
    {
        if (!navAgent.enabled) return false;
        if (!navAgent.isOnNavMesh) return false;

        return true;
    }

    public bool CanStopMoving()
    {
        if (!navAgent.enabled) return false;
        if (!navAgent.isOnNavMesh) return false;
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

    private void SetTargetRotation(Quaternion value)
    {
        TargetRotation = value;
        UseTargetRotation = true;
    }

    private void RemoveTargetRotation()
    {
        UseTargetRotation = false;
    }
}