using System;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] private EntityMovement movement;

    [SerializeField] private Health health;
    public Health Health => health;

    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackFrequency = 1f;
    private float currentAttackTime = 0f;

    private Attack currentTarget;
    private List<Attack> currentAttackers = new List<Attack>();
    private bool isAtacking = false;

    public event Action onStartedAttacking;
    public event Action onStoppedAttacking;

    private void OnEnable()
    {
        movement.onStopped += OnStopped;
        health.onDeath += OnDeath;
    }

    private void OnDisable()
    {
        movement.onStopped -= OnStopped;
        health.onDeath -= OnDeath;
    }

    private void Update()
    {
        if (!isAtacking) return;

        currentAttackTime += Time.deltaTime;
        if (currentAttackTime < attackFrequency) return;

        AttackTarget();
    }

    public void SetTarget(Attack target)
    {
        Debug.Log("Set target");
        currentTarget = target;
        target.HandleBecomeTarget(this);
    }

    public void RemoveTarget()
    {
        currentTarget = null;
        HandleStopBeingTarget(this);
    }

    public void AddAttacker(Attack attacker)
    {
        currentAttackers.Add(attacker);
    }

    public void RemoveAttacker(Attack attacker)
    {
        currentAttackers.Remove(attacker);
    }

    public void MoveToTarget()
    {
        movement.TryMoveTo(currentTarget.transform.position);
    }

    public void AttackTarget()
    {
        Health health = currentTarget.health;

        health.RemoveHealth(damage);
        currentAttackTime = 0f;
        Debug.Log(health.currentHealth);
    }

    public void HandleBecomeTarget(Attack target)
    {
        if (currentAttackers.Contains(target)) return;

        AddAttacker(target);
        SetTarget(target);
        MoveToTarget();
    }

    public void HandleStopBeingTarget(Attack target)
    {
        RemoveAttacker(target);
    }

    public void HandleTargetDeath()
    {
        RemoveTarget();
        StopAtacking();
    }

    private void StartAtacking()
    {
        isAtacking = true;
        onStartedAttacking?.Invoke();
    }

    private void StopAtacking()
    {
        Debug.Log("StopAttacking");
        isAtacking = false;
        onStoppedAttacking?.Invoke();
    }

    private void OnStopped()
    {
        if (!currentTarget) return;

        StartAtacking();
    }

    private void OnDeath()
    {
        foreach (var attacker in currentAttackers) {
            attacker.HandleTargetDeath();
        }
    }
}