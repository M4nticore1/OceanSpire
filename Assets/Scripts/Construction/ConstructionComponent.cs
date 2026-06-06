using System;
using UnityEngine;

public class ConstructionComponent : MonoBehaviour
{
    public float ConstructionTime { get; private set; } = 0f;
    public float CurrentConstructionTime { get; private set; } = 0f;

    [SerializeField] private bool isConstructable = true;
    public bool IsConstructable => isConstructable;

    public bool IsUnderConstruction { get; private set; } = false;

    public event Action OnConstructionStarted;
    public event Action OnConstructionCompleted;

    public static event Action<ConstructionComponent> OnGlobalConstructionStarted;
    public static event Action<ConstructionComponent> OnGlobalConstructionCompleted;

    private void Update()
    {
        if (!IsUnderConstruction) return;

        CurrentConstructionTime += Time.deltaTime;

        TryCompleteConstruction();
    }

    public void Init(ConstructionData data)
    {
        if (data != null) {
            ConstructionTime = data.ConstructionTime;
            CurrentConstructionTime = data.CurrentConstructionTime;
            IsUnderConstruction = data.IsUnderConstruction;

            if (IsUnderConstruction) {
                StartConstruction(data.ConstructionTime);
            }
        }

        TryCompleteConstruction();
    }

    public void StartConstruction(float constructionTime)
    {
        ConstructionTime = constructionTime;
        IsUnderConstruction = true;

        OnConstructionStarted?.Invoke();
        OnGlobalConstructionStarted?.Invoke(this);
    }

    public void TryCompleteConstruction()
    {
        if (!ShouldCompleteConstruction()) return;

        CompleteConstruction();
    }

    public void CompleteConstruction()
    {
        CurrentConstructionTime = 0f;
        IsUnderConstruction = false;

        OnConstructionCompleted?.Invoke();
        OnGlobalConstructionCompleted?.Invoke(this);
    }

    private bool ShouldCompleteConstruction()
    {
        if (!IsUnderConstruction) return false;
        if (CurrentConstructionTime < ConstructionTime) return false;

        return true;
    }
}
