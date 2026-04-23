using System;
using UnityEngine;

public class ConstructionComponent : MonoBehaviour
{
    [SerializeField] private float constructionTime = 300f;
    public float ConstructionTime => constructionTime;

    public float CurrentConstructionTime { get; private set; } = 0f;

    [SerializeField] private bool isConstructable = true;
    public bool IsConstructable => isConstructable;

    public bool IsUnderConstruction { get; private set; } = false;

    public event Action onConstructionStarted;
    public event Action onConstructionFinished;

    private void Update()
    {
        if (!IsUnderConstruction) return;

        CurrentConstructionTime += Time.deltaTime;
        if (CurrentConstructionTime < constructionTime) return;

        FinishConstruction();
    }

    public void Init(ConstructionData data)
    {
        if (data != null) {
            CurrentConstructionTime = data.ConstructionTime;
            IsUnderConstruction = data.UnderConstruction;

            if (IsUnderConstruction) {
                StartConstruction();
            }
        }
    }

    public void StartConstruction()
    {
        IsUnderConstruction = true;
        onConstructionStarted?.Invoke();
    }

    public void FinishConstruction()
    {
        CurrentConstructionTime = 0f;
        IsUnderConstruction = false;
        onConstructionFinished?.Invoke();
    }
}
