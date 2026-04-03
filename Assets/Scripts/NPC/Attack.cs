using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] private EntityMovement movement;

    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackFrequency = 1f;
    private float currentAttackTime = 0f;

    private Health currentTarget;
    private bool isAtacking = false;

    private void OnEnable()
    {
        movement.onStopped += OnStopped;
    }

    private void OnDisable()
    {
        movement.onStopped -= OnStopped;
    }

    private void Update()
    {
        if (!isAtacking) return;

        currentAttackTime += Time.deltaTime;
        if (currentAttackTime < attackFrequency) return;

        AttackTarget();
    }

    public void SetTarget(Health target)
    {
        currentTarget = target;
    }

    public void MoveToTarget()
    {
        movement.TryMoveTo(currentTarget.transform.position);
    }

    public void AttackTarget()
    {
        currentTarget.RemoveHealth(damage);
        currentAttackTime = 0f;
        Debug.Log(currentTarget.CurrentHealth);
    }

    private void StartAtacking()
    {
        isAtacking = true;
    }

    private void StopAtacking()
    {
        isAtacking = false;
    }

    private void OnStopped()
    {
        if (!currentTarget) return;

        StartAtacking();
    }
}