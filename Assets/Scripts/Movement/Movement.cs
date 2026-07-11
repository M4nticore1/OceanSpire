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

    private Coroutine moveCoroutine;

    public bool TryMoveTo(Vector3 position)
    {
        if (!CanStartMoving()) return false;

        if (moveCoroutine != null) {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        moveCoroutine = StartCoroutine(ApplyMovementAtEndOfFrame(position));

        return true;
    }

    private IEnumerator ApplyMovementAtEndOfFrame(Vector3 position)
    {
        yield return new WaitForEndOfFrame();

        moveCoroutine = null;

        if (IsReachedPosition(position)) {
            TargetPosition = position;
            navAgent.ResetPath();
            IsMoving = false;
            OnReachedDestination?.Invoke();
            yield break;
        }

        TargetPosition = position;
        RemoveTargetRotation();

        navAgent.isStopped = false;
        if (navAgent.SetDestination(position)) {
            IsMoving = true;
            OnMovementStarted?.Invoke();
        }
        else {
            Debug.Log("Failed to set destination to: " + position);
        }
    }

    public bool TryStopMoving()
    {
        if (moveCoroutine != null) {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

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
        if (!IsMoving) return false;
        if (!navAgent.enabled) return false;
        if (!navAgent.isOnNavMesh) return false;

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

    public bool CanAgentReachTarget(Vector3 targetPosition)
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