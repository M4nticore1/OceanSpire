using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackComponent : MonoBehaviour
{
    [SerializeField] private WeaponEquipment weaponHandler;
    [SerializeField] private Movement movement;
    [SerializeField] private HealthComponent health;

    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackFrequency = 1f;
    private float currentAttackTime = 0f;

    private AttackComponent currentTarget;
    private List<AttackComponent> currentAttackers = new List<AttackComponent>();
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
            attacker.OnTargetDied();
        }
    }

    private int GetDamage()
    {
        return weaponHandler.GetDamage();
    }
}