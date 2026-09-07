using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackComponent : MonoBehaviour, ILevelBonusable
{
    [Header("Main")]
    [SerializeField] private EquipmentComponent weaponComponent;
    [SerializeField] private Movement movement;
    [SerializeField] private HealthComponent health;

    public HealthComponent Health => health;

    [Header("Parameters")]
    [SerializeField, Min(0.01f)] private float attackFrequency = 1f;
    private float currentAttackTime;

    [Header("Targets")]
    [field: SerializeField]
    public AttackComponent CurrentTarget { get; private set; }

    [field: SerializeField]
    public List<AttackComponent> CurrentAttackers { get; private set; } = new();

    public bool IsAttacking { get; private set; }

    [Header("Bonus")]
    public float LevelBonus { get; private set; }

    public event Action<AttackComponent> OnTargetSet;
    public event Action<AttackComponent> OnTargetRemoved;

    public event Action<AttackComponent> OnAttackStarted;
    public event Action<AttackComponent> OnAttackStopped;
    public event Action<AttackComponent> OnAttacked;

    public static event Action<AttackComponent> OnGlobalAttackStarted;
    public static event Action<AttackComponent> OnGlobalAttackStopped;
    public static event Action<AttackComponent> OnGlobalAttacked;

    public static event Action<AttackComponent> OnInited;
    public static event Action<AttackComponent> OnDestroyed;

    private void OnEnable()
    {
        if (CombatManager.Instance != null)
            CombatManager.Instance.Register(this);

        if (movement != null)
            movement.OnDestinationReached += HandleDestinationReached;

        if (health != null)
            health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        if (CombatManager.Instance != null)
            CombatManager.Instance.Unregister(this);

        if (movement != null)
            movement.OnDestinationReached -= HandleDestinationReached;

        if (health != null)
            health.OnDied -= HandleDied;
    }

    private void Start()
    {
        OnInited?.Invoke(this);
    }

    private void OnDestroy()
    {
        OnDestroyed?.Invoke(this);
    }

    // Tick
    public void Tick()
    {
        if (CurrentTarget == null)
            return;

        UpdateIsAttacking();
        ProcessMovement();
        ProcessRotation();
        ProcessAttacking();
    }

    // Target
    public void SetTarget(AttackComponent target)
    {
        if (target == null) {
            Debug.LogError(
                $"[{nameof(AttackComponent)}] Attack target is not valid."
            );
            return;
        }

        if (target == this) {
            Debug.LogError(
                $"[{nameof(AttackComponent)}] Cannot target itself."
            );
            return;
        }

        if (!IsAttackerAvailable())
            return;

        if (!target.IsAttackerAvailable())
            return;

        if (CurrentTarget == target)
            return;

        RemoveTarget();

        CurrentTarget = target;
        target.AddAttacker(this);

        OnTargetSet?.Invoke(target);
    }

    public void RemoveTarget()
    {
        if (CurrentTarget == null)
            return;

        var lastTarget = CurrentTarget;

        CurrentTarget = null;

        lastTarget.RemoveAttacker(this);

        StopAttacking();

        OnTargetRemoved?.Invoke(lastTarget);
    }

    // Attackers
    public void AddAttacker(AttackComponent attacker)
    {
        if (attacker == null)
            return;

        if (attacker == this)
            return;

        if (!IsAttackerAvailable())
            return;

        if (!attacker.IsAttackerAvailable())
            return;

        if (CurrentAttackers.Contains(attacker))
            return;

        CurrentAttackers.Add(attacker);

        if (CurrentTarget == null) {
            SetTarget(attacker);
        }
    }

    public void AddAttackers(List<AttackComponent> attackers)
    {
        if (attackers == null)
            return;

        if (!IsAttackerAvailable())
            return;

        foreach (var attacker in attackers) {
            AddAttacker(attacker);
        }
    }

    public void RemoveAttacker(AttackComponent attacker)
    {
        if (attacker == null)
            return;

        CurrentAttackers.Remove(attacker);

        // If this attacker was targeting us,
        // make sure they stop targeting us.
        if (attacker.CurrentTarget == this) {
            attacker.RemoveTarget();
        }
    }

    public void RemoveAllAttackers()
    {
        var attackersCopy = new List<AttackComponent>(CurrentAttackers);

        foreach (var attacker in attackersCopy) {
            RemoveAttacker(attacker);
        }
    }

    // External events
    public void HandleStoppedBeingTarget(AttackComponent target)
    {
        RemoveAttacker(target);
    }

    public void HandleTargetDied()
    {
        RemoveTarget();
    }

    private void HandleAttacked(AttackComponent attacker)
    {
        if (attacker == null)
            return;

        if (CurrentTarget == null) {
            SetTarget(attacker);
        }
    }

    // Availability
    public bool IsAttackerAvailable()
    {
        if (health != null && !health.IsAlive)
            return false;

        return true;
    }

    public float GetDamage()
    {
        var weaponDamage = weaponComponent != null ? weaponComponent.GetPower() : 0f;

        return weaponDamage * (1f + LevelBonus);
    }

    // Attacking
    private void StartAttacking()
    {
        if (IsAttacking)
            return;

        if (CurrentTarget == null)
            return;

        IsAttacking = true;
        currentAttackTime = 0f;

        movement?.TryStopMoving();

        OnAttackStarted?.Invoke(CurrentTarget);
        OnGlobalAttackStarted?.Invoke(this);
    }

    private void StopAttacking()
    {
        if (!IsAttacking)
            return;

        IsAttacking = false;

        OnAttackStopped?.Invoke(CurrentTarget);
        OnGlobalAttackStopped?.Invoke(this);
    }

    private void ProcessAttacking()
    {
        if (!IsAttacking)
            return;

        currentAttackTime += Time.deltaTime;

        if (currentAttackTime < attackFrequency)
            return;

        AttackTarget();
    }

    private void AttackTarget()
    {
        if (CurrentTarget == null)
            return;

        if (!CurrentTarget.IsAttackerAvailable()) {
            HandleTargetDied();
            return;
        }

        var targetHealth = CurrentTarget.Health;

        if (targetHealth == null)
            return;

        targetHealth.RemoveHealth(GetDamage());

        currentAttackTime = 0f;

        var target = CurrentTarget;

        if (target != null) {
            target.HandleAttacked(this);
        }

        OnAttacked?.Invoke(target);
        OnGlobalAttacked?.Invoke(this);
    }

    // Movement
    private void ProcessMovement()
    {
        if (movement == null)
            return;

        if (CurrentTarget == null)
            return;

        if (movement.IsReachedPosition(CurrentTarget.transform.position)) {
            movement.TryStopMoving();
        }
        else {
            movement.TryMoveTo(CurrentTarget.transform.position);
        }
    }

    private void ProcessRotation()
    {
        if (movement == null)
            return;

        if (movement.IsMoving)
            return;

        if (CurrentTarget == null)
            return;

        var direction =
            CurrentTarget.transform.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        var rotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            rotation,
            movement.RotationSpeed * Time.deltaTime
        );
    }

    private void UpdateIsAttacking()
    {
        if (CurrentTarget == null) {
            StopAttacking();
            return;
        }

        if (movement == null) {
            StartAttacking();
            return;
        }

        if (movement.IsReachedPosition(CurrentTarget.transform.position)) {
            StartAttacking();
        }
        else {
            StopAttacking();
        }
    }

    private void HandleDestinationReached()
    {
        if (CurrentTarget != null) {
            StartAttacking();
        }
    }

    // Death
    private void HandleDied()
    {
        RemoveTarget();

        var attackersCopy = new List<AttackComponent>(CurrentAttackers);
        foreach (var attacker in attackersCopy) {
            attacker.HandleTargetDied();
        }

        CurrentAttackers.Clear();
    }

    // Bonus
    public void SetLevelBonus(float bonus)
    {
        LevelBonus = bonus;
    }
}