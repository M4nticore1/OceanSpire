using System;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] private WeaponHandler weaponHandler;
    [SerializeField] private Movement movement;
    [SerializeField] private Health health;

    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackFrequency = 1f;
    private float currentAttackTime = 0f;

    private Attack currentTarget;
    private List<Attack> currentAttackers = new List<Attack>();
    public bool isAttacking { get; private set; } = false;

    public event Action onStartedAttacking;
    public event Action onStoppedAttacking;

    private void OnEnable()
    {
        movement.onStoppedMoving += OnStopped;
        health.onDied += OnDied;
    }

    private void OnDisable()
    {
        movement.onStoppedMoving -= OnStopped;
        health.onDied -= OnDied;
    }

    private void Update()
    {
        if (!isAttacking) return;

        currentAttackTime += Time.deltaTime;
        if (currentAttackTime < attackFrequency) return;

        AttackTarget();
    }

    public void SetTarget(Attack target)
    {
        currentTarget = target;
        target.HandleBecomeTarget(this);
    }

    public void RemoveTarget()
    {
        currentTarget = null;
        StopAtacking();
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

        health.RemoveHealth(GetDamage());
        currentAttackTime = 0f;
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
    }

    private void StartAtacking()
    {
        isAttacking = true;
        onStartedAttacking?.Invoke();
    }

    private void StopAtacking()
    {
        isAttacking = false;
        onStoppedAttacking?.Invoke();
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
            attacker.HandleTargetDeath();
        }
    }

    private int GetDamage()
    {
        return weaponHandler.GetDamage();
    }
}