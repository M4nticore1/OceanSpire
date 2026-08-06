using System;
using System.Collections;
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
    public event Action OnDestinationReached;

    private void Update()
    {
        if (IsMoving && IsDestinationReached()) {
            StopMoving();
        }
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
            Debug.LogError($"[{nameof(Movement)}] Transform is not valid!");
            return false;
        }

        if (!TryMoveTo(transform.position)) return false;

        SetTargetRotation(transform.rotation);
        return true;
    }

    public bool TryMoveTo(Vector3 position, bool useReachedPosition = true)
    {
        if (!CanStartMoving()) return false;
        if (!useReachedPosition && IsReachedPosition(position)) return false;

        //navAgent.ResetPath();
        //navAgent.Warp(transform.position);

        TargetPosition = position;
        RemoveTargetRotation();

        if (IsReachedPosition(position)) {
            IsMoving = false;
            OnDestinationReached?.Invoke();
            return true;
        }

        if (navAgent.SetDestination(position)) {
            IsMoving = true;
            OnMovementStarted?.Invoke();
            return true;
        }

        return false;
    }

    public bool StopMoving()
    {
        if (!CanStopMoving()) return false;

        navAgent.ResetPath();
        var lastIsMoving = IsMoving;
        IsMoving = false;

        if (lastIsMoving) {
            OnMovementStopped?.Invoke();
        }

        if (IsDestinationReached()) {
            OnDestinationReached?.Invoke();
        }

        return true;
    }

    public bool CanStartMoving()
    {
        if (!navAgent.enabled) {
            //Debug.LogError($"[{nameof(Movement)}] Nav Agent is not enabled! Movement blocked.");
            return false;
        }
        if (!navAgent.isOnNavMesh) {
            //Debug.LogError($"[{nameof(Movement)}] Nav Agent is not on nav mesh! Movement blocked.");
            return false;
        }

        return true;
    }

    public bool CanStopMoving()
    {
        if (!IsMoving) return false;
        if (!navAgent.enabled) return false;
        if (!navAgent.isOnNavMesh) return false;

        return true;
    }

    public bool IsDestinationReached()
    {
        if (IsReachedPosition(TargetPosition)) return true;

        return false;
    }

    public bool IsReachedPosition(Vector3 position)
    {
        return Vector3.Distance(transform.position, position) <= navAgent.stoppingDistance;
    }

    public bool CanReachPosition(Vector3 targetPosition)
    {
        var path = new NavMeshPath();

        if (navAgent.CalculatePath(targetPosition, path)) {
            if (path.status == NavMeshPathStatus.PathComplete) {
                return true;
            }
        }

        return false;
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