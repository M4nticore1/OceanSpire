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

    [field: SerializeField] public bool IsMoving { get; private set; } = false;

    public Vector3? TargetPosition { get; private set; } = null;
    public Quaternion? TargetRotation { get; private set; } = null;

    public MovementMethod CurrentMovementMethod { get; private set; }

    [Header("Speed")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    [Header("Rotation")]
    [field: SerializeField] public bool UseTargetRotation { get; private set; } = true;
    [SerializeField] private float rotationSpeed = 1f;

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
        if (IsMoving && IsDestinationReached()) {
            TryStopMoving();
        }

        ProcessRotation();
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
        if (navAgent == null) return;

        navAgent.enabled = enabled;
    }

    // Move To
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
            IsMoving = false;
            TargetPosition = null;

            if (useReachedPosition) {
                OnDestinationReached?.Invoke();
                return true;
            }
            else {
                return false;
            }
        }

        IsMoving = true;
        navAgent.isStopped = false;
        TargetPosition = position;
        RemoveTargetRotation();

        if (!navAgent.SetDestination(position)) {
            TargetPosition = null;
            IsMoving = false;
            return false;
        }

        //Debug.Log($"MoveTo {gameObject}");
        OnMovementStarted?.Invoke();
        return true;
    }

    // Stop
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

        //Debug.Log($"StopMoving {gameObject}");
        OnMovementStopped?.Invoke();
        return true;
    }

    // Check
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

    public bool IsReachedPosition(Vector3 targetPosition)
    {
        return GetDistanceToPosition(targetPosition) <= navAgent.stoppingDistance;
    }

    public bool CanReachPosition(Vector3 targetPosition)
    {
        if (navAgent == null) return false;
        if (!navAgent.enabled) return false;
        if (!navAgent.isOnNavMesh) return false;

        var path = new NavMeshPath();
        if (!navAgent.CalculatePath(targetPosition, path)) {
            return false;
        }

        return path.status == NavMeshPathStatus.PathComplete;
    }

    public float GetDistanceToPosition(Vector3 targetPosition)
    {
        var currentPosition = transform.position;
        return Vector3.Distance(new Vector3(currentPosition.x, currentPosition.y - navAgent.baseOffset, currentPosition.z), targetPosition);
    }

    public float? GetTargetPositionDistance()
    {
        if (TargetPosition == null) return null;

        return Vector3.Distance(
            navAgent.nextPosition,
            TargetPosition.Value
        );
    }

    // Rotation
    private void ProcessRotation()
    {
        if (!UseTargetRotation) return;
        if (TargetRotation == null) return;

        transform.rotation = Quaternion.Lerp(transform.rotation, TargetRotation.Value, rotationSpeed * Time.deltaTime);
    }

    private void SetTargetRotation(Quaternion value)
    {
        TargetRotation = value;
    }

    private void RemoveTargetRotation()
    {
        TargetRotation = null;
    }

    public void SetUseRotation(bool value)
    {
        UseTargetRotation = value;
    }
}