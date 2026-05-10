using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackComponent : MonoBehaviour
{
    [SerializeField] private EquipmentComponent weaponComponent;
    [SerializeField] private Movement movement;
    [SerializeField] private HealthComponent health;

    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackFrequency = 1f;
    private float currentAttackTime = 0f;

    [SerializeField] private float stopMovingDistance = 1f;
    [SerializeField] private float rotationSpeed = 1f;

    private AttackComponent currentTarget;
    private List<AttackComponent> currentAttackers = new List<AttackComponent>();
    public bool IsAttacking { get; private set; } = false;

    public event Action onAttackStarted;
    public event Action onAttackStopped;

    private void OnEnable()
    {
        movement.onMovementStopped += OnStopped;
        health.onDied += OnDied;
    }

    private void OnDisable()
    {
        movement.onMovementStopped -= OnStopped;
        health.onDied -= OnDied;
    }

    private void Update()
    {
        if (currentTarget) {
            TryStopMoving();
            CorrentRotation();
        }

        if (!IsAttacking) return;

        currentAttackTime += Time.deltaTime;
        if (currentAttackTime < attackFrequency) return;

        AttackTarget();
    }

    public void SetTarget(AttackComponent target)
    {
        currentTarget = target;
        target.OnBecomeTarget(this);
    }

    public void RemoveTarget()
    {
        currentTarget = null;
        StopAtacking();
    }

    public void AddAttacker(AttackComponent attacker)
    {
        currentAttackers.Add(attacker);
    }

    public void RemoveAttacker(AttackComponent attacker)
    {
        currentAttackers.Remove(attacker);
    }

    public void MoveToTarget()
    {
        movement.TryMoveTo(currentTarget.transform.position);
    }

    public void AttackTarget()
    {
        HealthComponent health = currentTarget.health;

        health.RemoveHealth(GetDamage());
        currentAttackTime = 0f;
    }

    public void OnBecomeTarget(AttackComponent target)
    {
        if (currentAttackers.Contains(target)) return;

        AddAttacker(target);
        SetTarget(target);
        MoveToTarget();
    }

    public void OnStopBeingTarget(AttackComponent target)
    {
        RemoveAttacker(target);
    }

    public void OnTargetDied()
    {
        RemoveTarget();
    }

    private void StartAtacking()
    {
        IsAttacking = true;
        onAttackStarted?.Invoke();
    }

    private void StopAtacking()
    {
        IsAttacking = false;
        onAttackStopped?.Invoke();
    }

    private void TryStopMoving()
    {
        if (!movement.IsMoving) return;

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (distance > stopMovingDistance) return;

        movement.StopMoving();
        Debug.Log("Stopped");
    }

    private void CorrentRotation()
    {
        if (movement.IsMoving) return;

        Vector3 direction = currentTarget.transform.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }

    private void OnStopped()
    {
        if (!currentTarget) return;

        StartAtacking();
    }

    private void OnDied()
    {
        RemoveTarget();

        foreach (var attacker in currentAttackers) {
            attacker.OnTargetDied();
        }
    }

    private float GetDamage()
    {
        return weaponComponent.GetPower();
    }
}