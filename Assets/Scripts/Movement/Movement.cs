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

    public Vector3? TargetPosition { get; private set; }

    public bool UseTargetRotation { get; private set; }
    public Quaternion TargetRotation { get; private set; } = Quaternion.identity;

    public MovementMethod CurrentMovementMethod { get; private set; }

    [Header("Speed")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    [field: SerializeField] public bool IsMoving { get; private set; }

    private MovementManager movementManager => MovementManager.Instance;

    public event Action OnMovementStarted;
    public event Action OnMovementStopped;
    public event Action OnDestinationReached;

    private void OnEnable()
    {
        if (movementManager) {
            movementManager.RegisterMovement(this);
        }
        else {
            Debug.LogError($"[{nameof(Movement)}] Movement Manager is not valid!");
        }
    }

    private void OnDisable()
    {
        if (movementManager) {
            movementManager.UnregisterMovement(this);
        }
    }

    public void Tick()
    {
        if (!IsMoving) return;

        if (IsDestinationReached()) {
            TryStopMoving();
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
        if (!navAgent) return;

        navAgent.enabled = enabled;
    }

    public bool TryMoveTo(Transform target)
    {
        if (target == null) {
            Debug.LogError($"[{nameof(Movement)}] Target Transform is not valid!");
            return false;
        }

        if (!TryMoveTo(target.position)) return false;

        SetTargetRotation(target.rotation);
        return true;
    }

    public bool TryMoveTo(Vector3 position, bool useReachedPosition = true)
    {
        if (!CanStartMoving()) return false;

        if (IsReachedPosition(position)) {
            if (useReachedPosition) {
                TargetPosition = position;

                IsMoving = false;
                TargetPosition = null;

                OnDestinationReached?.Invoke();
                return true;
            }
            else {
                return false;
            }
        }

        navAgent.isStopped = false;
        TargetPosition = position;
        RemoveTargetRotation();

        if (!navAgent.SetDestination(position)) {
            TargetPosition = null;
            IsMoving = false;
            return false;
        }

        IsMoving = true;
        OnMovementStarted?.Invoke();

        return true;
    }

    public bool TryStopMoving()
    {
        if (!CanStopMoving()) return false;

        bool destinationReached = IsDestinationReached();

        navAgent.isStopped = true;
        IsMoving = false;
        TargetPosition = null;

        if (destinationReached) {
            OnDestinationReached?.Invoke();
        }

        OnMovementStopped?.Invoke();

        return true;
    }

    public bool CanStartMoving()
    {
        if (!navAgent) return false;
        if (!navAgent.enabled) return false;
        if (!navAgent.isOnNavMesh) return false;

        return true;
    }

    public bool CanStopMoving()
    {
        if (!navAgent) return false;
        if (!IsMoving) return false;
        if (!navAgent.enabled) return false;
        if (!navAgent.isOnNavMesh) return false;

        return true;
    }

    public bool IsDestinationReached()
    {
        if (TargetPosition == null) return false;

        return IsReachedPosition(TargetPosition.Value);
    }

    public bool IsReachedPosition(Vector3 position)
    {
        var currentPosition = transform.position;
        return Vector3.Distance(new Vector3(currentPosition.x, currentPosition.y - navAgent.baseOffset, currentPosition.z), position) <= navAgent.stoppingDistance;
    }

    public bool CanReachPosition(Vector3 targetPosition)
    {
        if (!navAgent) return false;
        if (!navAgent.enabled) return false;
        if (!navAgent.isOnNavMesh) return false;

        var path = new NavMeshPath();

        if (!navAgent.CalculatePath(targetPosition, path)) {
            return false;
        }

        return path.status == NavMeshPathStatus.PathComplete;
    }

    public float? GetTargetPositionDistance()
    {
        if (TargetPosition == null) return null;

        return Vector3.Distance(
            navAgent.nextPosition,
            TargetPosition.Value
        );
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