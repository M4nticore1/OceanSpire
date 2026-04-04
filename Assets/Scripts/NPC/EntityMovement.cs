using System;
using UnityEngine;
using UnityEngine.AI;

public class EntityMovement : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navAgent;
    public NavMeshAgent NavAgent => navAgent;

    private bool isMoving = false;

    public event Action onReachedPath;
    public event Action onStopped;

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
        isMoving = true;
        return navAgent.SetDestination(position);
    }

    public void StopMoving()
    {
        if (!isMoving) {
            Debug.LogWarning("NPC is already not moving.");
            return;
        }

        navAgent.isStopped = true;
        navAgent.ResetPath();
        isMoving = false;
        onStopped?.Invoke();
    }

    public void SetAgentEnabled(bool enabled)
    {
        navAgent.enabled = enabled;
    }

    private bool CanMove()
    {
        return navAgent.enabled;
    }

    private void OnReachedPath()
    {
        StopMoving();
        onReachedPath?.Invoke();
    }

    private bool CheckDistancePathPosition()
    {
        if (!isMoving) return false;
        if (!navAgent.enabled) return false;
        if (navAgent.pathPending) return false;

        if (navAgent.pathStatus != NavMeshPathStatus.PathComplete)
            return false;

        if (navAgent.remainingDistance > navAgent.stoppingDistance)
            return false;

        return true;
    }
}