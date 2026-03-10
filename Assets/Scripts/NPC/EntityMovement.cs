using System;
using UnityEngine;
using UnityEngine.AI;

public class EntityMovement : MonoBehaviour
{
    private NavMeshAgent agent = null;

    private bool isMoving = false;

    public event Action onReachedPath;
    public event Action onStoppedMoving;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

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

    public bool MoveTo(Vector3 position)
    {
        //if (!CanMove()) return false;

        agent.isStopped = false;
        isMoving = true;
        return agent.SetDestination(position);
    }

    public void StopMoving()
    {
        if (!isMoving) {
            Debug.LogWarning("NPC is already not moving.");
            return;
        }

        agent.isStopped = true;
        agent.ResetPath();
        isMoving = false;
        onStoppedMoving?.Invoke();
    }

    public void SetAgentEnabled(bool enabled)
    {
        agent.enabled = enabled;
    }

    private bool CanMove()
    {
        return agent.enabled;
    }

    private void OnReachedPath()
    {
        StopMoving();
        onReachedPath?.Invoke();
    }

    private bool CheckDistancePathPosition()
    {
        if (!isMoving || !agent.enabled || agent.pathPending)
            return false;

        return agent.remainingDistance <= agent.stoppingDistance;
    }
}