using System;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionComponent : MonoBehaviour
{
    [SerializeField] private float constructionTime = 300f;
    private float currentConstructionTime = 0f;

    private bool isUnderConstruction = false;

    public event Action onConstructionFinished;

    private void Update()
    {
        if (!isUnderConstruction) return;

        currentConstructionTime += Time.deltaTime;
        if (currentConstructionTime < constructionTime) return;

        FinishConstruction();
    }

    public void Init(ConstructionData data)
    {
        if (data != null) {
            constructionTime = data.ConstructionTime;
            isUnderConstruction = data.UnderConstruction;

            if (isUnderConstruction) {
                StartConstruction();
            }
        }
    }

    public void StartConstruction()
    {
        isUnderConstruction = true;
    }

    public void FinishConstruction()
    {
        currentConstructionTime = 0f;
        isUnderConstruction = false;
        onConstructionFinished?.Invoke();
    }
}
