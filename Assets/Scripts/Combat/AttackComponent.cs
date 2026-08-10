using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AttackComponent : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private EquipmentComponent weaponComponent;
    [SerializeField] private Movement movement;
    [SerializeField] private HealthComponent health;

    [Header("Parameters")]
    [SerializeField] private float attackFrequency = 1f;
    private float currentAttackTime = 0f;

    [SerializeField] private float stopMovingDistance = 1f;
    [SerializeField] private float rotationSpeed = 1f;

    [Header("Targets")]
    [field: SerializeField] public AttackComponent CurrentTarget { get; private set; }
    [field: SerializeField] public List<AttackComponent> CurrentAttackers { get; private set; } = new();
    public bool IsAttacking { get; private set; } = false;

    public event Action OnAttackStarted;
    public event Action OnAttackStopped;
    public event Action OnAttacked;

    public static event Action<AttackComponent> OnGlobalAttackStarted;
    public static event Action<AttackComponent> OnGlobalAttackStopped;
    public static event Action<AttackComponent> OnGlobalAttacked;

    private void OnEnable()
    {
        CombatManager.Instance.Register(this);

        movement.OnDestinationReached += HandleDestinationReached;
        health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        CombatManager.Instance.Unregister(this);

        movement.OnDestinationReached -= HandleDestinationReached;
        health.OnDied -= HandleDied;
    }

    public void Tick()
    {
        if (CurrentTarget != null) {
            if (!movement.IsReachedPosition(CurrentTarget.transform.position)) {
                MoveToTarget();
            }

            CorrectRotation();

            if (IsAttacking) {
                currentAttackTime += Time.deltaTime;
                if (currentAttackTime >= attackFrequency) {
                    AttackTarget();
                }
            }
        }
    }

    public void SetTarget(AttackComponent target)
    {
        if (target == null) {
            Debug.LogError($"[{nameof(AttackComponent)}] Attack target is not valid");
            return;
        }

        if (target == this) {
            Debug.LogError($"[{nameof(AttackComponent)}] Combat Target is this component!");
            return;
        }

        CurrentTarget = target;
        target.AddAttacker(this);
        MoveToTarget();
    }

    public void RemoveTarget()
    {
        CurrentTarget = null;
        StopAttacking();
    }

    public void AddAttacker(AttackComponent attacker)
    {
        if (attacker == null) {
            Debug.LogError($"[{nameof(AttackComponent)}] Attacker is not valid");
            return;
        }

        if (attacker == this) {
            Debug.LogError($"[{nameof(AttackComponent)}] Attacker is this component!");
            return;
        }

        if (CurrentAttackers.Contains(attacker)) return;

        CurrentAttackers.Add(attacker);
        if (CurrentAttackers.Count == 1 && CurrentTarget == null) {
            SetTarget(attacker);
        }

        attacker.AddAttacker(this);
    }

    public void AddAttackers(List<AttackComponent> attackers)
    {
        foreach (var attacker in attackers) {
            AddAttacker(attacker);
        }
    }

    public void RemoveAllAttackers()
    {
        var attackersCopy = new List<AttackComponent>(CurrentAttackers);
        foreach (var attacker in attackersCopy) {
            RemoveAttacker(attacker);
        }
    }

    public void RemoveAttacker(AttackComponent attacker)
    {
        CurrentAttackers.Remove(attacker);

        var attackerTarget = attacker.CurrentTarget;
        if (attackerTarget && attackerTarget == this) {
            attacker.RemoveTarget();
        }
    }

    public void MoveToTarget()
    {
        if (!CurrentTarget) return;

        movement.TryMoveTo(CurrentTarget.transform.position);
    }

    public void AttackTarget()
    {
        if (CurrentTarget == null) return;

        var healthComponent = CurrentTarget.health;
        if (healthComponent == null) return;

        healthComponent.RemoveHealth(GetDamage());
        currentAttackTime = 0f;

        if (CurrentTarget != null) {
            CurrentTarget.HandleAttacked(this);
        }

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

    private void StartAttacking()
    {
        if (IsAttacking) return;

        IsAttacking = true;
        currentAttackTime = 0f;

        OnAttackStarted?.Invoke();
        OnGlobalAttackStarted?.Invoke(this);
    }

    private void StopAttacking()
    {
        if (!IsAttacking) return;

        IsAttacking = false;

        OnAttackStopped?.Invoke();
        OnGlobalAttackStopped?.Invoke(this);
    }

    private bool TryStopMoving()
    {
        if (CurrentTarget == null) return false;
        if (!movement.IsReachedPosition(CurrentTarget.transform.position)) return false;

        movement.TryStopMoving();
        return true;
    }

    private void CorrectRotation()
    {
        if (movement.IsMoving) return;
        if (CurrentTarget == null) return;

        var direction = CurrentTarget.transform.position - transform.position;
        if (direction == Vector3.zero) return;

        var rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }

    private void HandleAttacked(AttackComponent attackComponent)
    {
        if (CurrentTarget == null) {
            SetTarget(attackComponent);
        }
    }

    private void HandleDestinationReached()
    {
        if (CurrentTarget != null) {
            StartAttacking();
        }
    }

    private void HandleDied()
    {
        RemoveTarget();

        var attackersCopy = new List<AttackComponent>(CurrentAttackers);
        foreach (var attacker in attackersCopy) {
            attacker.OnTargetDied();
        }
    }

    private float GetDamage()
    {
        return weaponComponent != null ? weaponComponent.GetPower() : 0f;
    }
}