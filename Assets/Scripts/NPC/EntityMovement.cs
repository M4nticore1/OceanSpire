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
        isMoving = true;
        return agent.SetDestination(position);
    }

    public void StopMoving()
    {
        if (!isMoving) {
            Debug.LogWarning("NPC is already not moving.");
            return;
        }

        agent.ResetPath();
        isMoving = false;
        onStoppedMoving?.Invoke();
    }

    private void OnReachedPath()
    {
        StopMoving();
        onReachedPath?.Invoke();
    }

    private bool CheckDistancePathPosition()
    {
        return isMoving && agent.enabled && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
    }

    public void SetAgentEnabled(bool enabled)
    {
        agent.enabled = enabled;
    }
}