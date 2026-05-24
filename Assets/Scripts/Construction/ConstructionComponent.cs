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
        if (CurrentConstructionTime < ConstructionTime) return;

        FinishConstruction();
    }

    public void Init(ConstructionData data)
    {
        if (data != null) {
            CurrentConstructionTime = data.CurrentConstructionTime;
            IsUnderConstruction = data.IsUnderConstruction;

            if (IsUnderConstruction) {
                StartConstruction(data.ConstructionTime);
            }
        }
    }

    public void StartConstruction(float constructionTime)
    {
        ConstructionTime = constructionTime;
        IsUnderConstruction = true;

        OnConstructionStarted?.Invoke();
        OnGlobalConstructionStarted?.Invoke(this);
    }

    public void FinishConstruction()
    {
        CurrentConstructionTime = 0f;
        IsUnderConstruction = false;

        OnConstructionCompleted?.Invoke();
        OnGlobalConstructionCompleted?.Invoke(this);
    }
}
