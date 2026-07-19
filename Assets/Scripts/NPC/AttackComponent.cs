using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackComponent : MonoBehaviour
{
    [SerializeField] private EquipmentComponent weaponComponent;
    [SerializeField] private Movement movement;
    [SerializeField] private HealthComponent health;

    [SerializeField] private float attackFrequency = 1f;
    private float currentAttackTime = 0f;

    [SerializeField] private float stopMovingDistance = 1f;
    [SerializeField] private float rotationSpeed = 1f;

    private AttackComponent currentTarget;
    private List<AttackComponent> currentAttackers = new List<AttackComponent>();
    public bool IsAttacking { get; private set; } = false;

    public event Action OnAttackStarted;
    public event Action OnAttackStopped;
    public event Action OnAttacked;

    public static event Action<AttackComponent> OnGlobalAttackStarted;
    public static event Action<AttackComponent> OnGlobalAttackStopped;
    public static event Action<AttackComponent> OnGlobalAttacked;

    private void OnEnable()
    {
        movement.OnMovementStopped += OnStopped;
        health.OnDied += OnDied;
    }

    private void OnDisable()
    {
        movement.OnMovementStopped -= OnStopped;
        health.OnDied -= OnDied;
    }

    private void Update()
    {
        if (!currentTarget) return;

        TryStopMoving();
        CorrentRotation();

        if (!IsAttacking) return;

        currentAttackTime += Time.deltaTime;
        if (currentAttackTime < attackFrequency) return;

        AttackTarget();
    }

    public void SetTarget(AttackComponent target)
    {
        if (!target) {
            Debug.LogError("Attack target is not valid");
            return;
        }

        currentTarget = target;
        target.AddAttacker(this);
    }

    public void RemoveTarget()
    {
        currentTarget = null;
        StopAtacking();
    }

    public void AddAttacker(AttackComponent attacker)
    {
        currentAttackers.Add(attacker);

        if (currentAttackers.Count > 1) return;

        SetTarget(attacker);
        MoveToTarget();
    }

    public void RemoveAllAttackers()
    {
        foreach (var attacker in currentAttackers) {
            RemoveAttacker(attacker);
        }
    }

    public void RemoveAttacker(AttackComponent attacker)
    {
        currentAttackers.Remove(attacker);

        var attackerTarget = attacker.currentTarget;
        if (attackerTarget && attackerTarget == this) {
            attacker.RemoveTarget();
        }
    }

    public void MoveToTarget()
    {
        movement.TryMoveTo(currentTarget.transform.position);
    }

    public void AttackTarget()
    {
        var health = currentTarget.health;

        health.RemoveHealth(GetDamage());
        currentAttackTime = 0f;

        OnAttacked?.Invoke();
        OnGlobalAttacked?.Invoke(this);
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

        OnAttackStarted?.Invoke();
        OnGlobalAttackStarted?.Invoke(this);
    }

    private void StopAtacking()
    {
        IsAttacking = false;

        OnAttackStopped?.Invoke();
        OnGlobalAttackStopped?.Invoke(this);
    }

    private void TryStopMoving()
    {
        if (!movement.IsReachedPosition(currentTarget.transform.position)) return;

        movement.StopMoving();
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