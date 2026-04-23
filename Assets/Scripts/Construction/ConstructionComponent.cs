using System;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionComponent : MonoBehaviour
{
    [SerializeField] private float constructionTime = 300f;
    private float currentConstructionTime = 0f;

    [SerializeField] private bool isConstructable = true;
    public bool IsConstructable => isConstructable;

    public bool IsUnderConstruction { get; private set; } = false;

    public event Action onConstructionStarted;
    public event Action onConstructionFinished;

    private void Update()
    {
        if (!IsUnderConstruction) return;

        currentConstructionTime += Time.deltaTime;
        if (currentConstructionTime < constructionTime) return;

        FinishConstruction();
    }

    public void Init(ConstructionData data)
    {
        if (data != null) {
            currentConstructionTime = data.ConstructionTime;
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
        currentConstructionTime = 0f;
        IsUnderConstruction = false;
        onConstructionFinished?.Invoke();
        Debug.Log("Finish");
    }
}
