using System;
using UnityEngine;

public class ConstructionComponent : MonoBehaviour
{
    public long? ConstructionStartTime { get; private set; } = 0;
    public long? ConstructionFinishTime { get; private set; } = 0;

    [SerializeField] private bool isConstructable = true;
    public bool IsConstructable => isConstructable;

    public bool IsUnderConstruction { get; private set; } = false;

    public event Action OnConstructionStarted;
    public event Action OnConstructionCompleted;

    public static event Action<ConstructionComponent> OnGlobalConstructionStarted;
    public static event Action<ConstructionComponent> OnGlobalConstructionFinished;

    private void Update()
    {
        if (!IsUnderConstruction) return;

        TryFinishConstruction();
    }

    public void Init()
    {
        var constructionData = ConstructionData.Default();
        Init(constructionData);
    }

    public void Init(ConstructionData constructionData)
    {
        if (constructionData == null) {
            Debug.LogError("ConstructionData is not valid");
            Init();
        }

        ConstructionStartTime = constructionData.ConstructionStartTime;
        ConstructionFinishTime = constructionData.ConstructionFinishTime;
        IsUnderConstruction = constructionData.IsUnderConstruction;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var constructionTime = ConstructionFinishTime - currentTime;

        if (ShouldFinishConstruction()) {
            FinishConstruction();
        }
        else if (IsUnderConstruction) {
            StartConstruction((int)constructionTime);
        }
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

    public void TryFinishConstruction()
    {
        if (!ShouldFinishConstruction()) return;

        FinishConstruction();
    }

    public void FinishConstruction()
    {
        IsUnderConstruction = false;

        OnConstructionCompleted?.Invoke();
        OnGlobalConstructionFinished?.Invoke(this);
    }

    public int? GetRemainingConstructionTime()
    {
        if (ConstructionFinishTime == null) return null;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (currentTime > ConstructionFinishTime) return null;

        return (int?)(ConstructionFinishTime - currentTime);
    }

    private bool ShouldFinishConstruction()
    {
        if (!IsUnderConstruction) return false;
        if (ConstructionFinishTime == null) return false;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (currentTime < ConstructionFinishTime) return false;

        return true;
    }
}