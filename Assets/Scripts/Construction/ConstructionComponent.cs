using System;
using UnityEngine;

public class ConstructionComponent : MonoBehaviour
{
    public long ConstructionStartTime { get; private set; } = 0;
    public long ConstructionFinishTime { get; private set; } = 0;

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

        TryCompleteConstruction();
    }

    public void Init(ConstructionData data)
    {
        if (data != null) {
            ConstructionStartTime = data.ConstructionStartTime;
            ConstructionFinishTime = data.ConstructionFinishTime;
            IsUnderConstruction = data.IsUnderConstruction;

            var constructionTime = ConstructionFinishTime - ConstructionStartTime;

            if (IsUnderConstruction) {
                StartConstruction((int)constructionTime);
            }
        }

        TryCompleteConstruction();
    }

    public void StartConstruction(int constructionTime)
    {
        IsUnderConstruction = true;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        ConstructionStartTime = currentTime;
        ConstructionFinishTime = currentTime + constructionTime;

        OnConstructionStarted?.Invoke();
        OnGlobalConstructionStarted?.Invoke(this);
    }

    public void TryCompleteConstruction()
    {
        if (!ShouldCompleteConstruction()) return;

        FinishConstruction();
    }

    public void FinishConstruction()
    {
        IsUnderConstruction = false;

        OnConstructionCompleted?.Invoke();
        OnGlobalConstructionCompleted?.Invoke(this);
    }

    private bool ShouldCompleteConstruction()
    {
        if (!IsUnderConstruction) return false;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (currentTime < ConstructionFinishTime) return false;

        return true;
    }
}
