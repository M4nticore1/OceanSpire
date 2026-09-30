using System;
using System.Collections;
using UnityEngine;

public abstract class BuildingIndicatorController : MonoBehaviour, IClickable
{
    [SerializeField] private BuildingIndicator indicator;

    // Clickable
    [SerializeField] private bool isClickable = true;
    public bool IsClickable
    {
        get {
            return isClickable;
        }
        set {
            isClickable = value;
        }
    }

    protected Building building
    {
        get {
            if (indicator == null) {
                Debug.LogError($"[{nameof(BuildingIndicatorController)}] Indicator is not valid at {this}!");
                return null;
            }

            var construction = indicator.BuildingConstruction;
            if (construction == null) {
                Debug.LogError($"[{nameof(BuildingIndicatorController)}] Construction is not valid at {this}!");
                return null;
            }

            var building = construction.OwnedBuilding;
            if (building == null) {
                Debug.LogError($"[{nameof(BuildingIndicatorController)}] Building is not valid at {construction}!");
                return null;
            }

            return building;
        }
    }

    protected ConstructionComponent constructionComponent => building?.ConstructionComponent;

    private Coroutine updateShownCoroutine;

    public event Action OnClicked;

    protected virtual void OnDestroy()
    {
        Unsubscribe();
    }

    protected virtual void Start()
    {
        StartCoroutine(StartEndOfFrame());
        RunUpdateShownEndOfFrame();
    }

    protected virtual void Subscribe()
    {
        if (constructionComponent != null) {
            constructionComponent.OnConstructionStarted += HandleConstructionStarted;
            constructionComponent.OnConstructionFinished += HandleConstructionFinished;
        }
        else {
            Debug.LogError($"[{nameof(BuildingIndicatorController)}] Construction Component is not valid!");
        }
    }

    protected virtual void Unsubscribe()
    {
        if (constructionComponent != null) {
            constructionComponent.OnConstructionStarted -= HandleConstructionStarted;
            constructionComponent.OnConstructionFinished -= HandleConstructionFinished;
        }
    }

    protected abstract void HandleClick();

    protected abstract bool ShouldShow();

    protected virtual Texture GetIndicatorTexture()
    {
        return indicator.IndicatorTexture;
    }

    // IClickable
    public void Click()
    {
        HandleClick();

        OnClicked?.Invoke();
    }

    public bool ShouldClick()
    {
        return IsClickable;
    }

    // Update Shown
    protected void RunUpdateShownEndOfFrame()
    {
        if (updateShownCoroutine == null) {
            updateShownCoroutine = StartCoroutine(UpdateShownEndOfFrame());
        }
    }

    private void UpdateShown()
    {
        if (ShouldShow()) {
            indicator.Show(GetIndicatorTexture());
        }
        else {
            indicator.Hide();
        }
    }

    // Events
    private void HandleConstructionStarted()
    {
        RunUpdateShownEndOfFrame();
    }

    private void HandleConstructionFinished()
    {
        RunUpdateShownEndOfFrame();
    }

    // Coroutine
    private IEnumerator UpdateShownEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        updateShownCoroutine = null;
        UpdateShown();
    }

    private IEnumerator StartEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        if (indicator == null) {
            Debug.LogError($"[{nameof(BuildingIndicatorController)}] Indicator is not valid at {this}!");
        }

        Subscribe();
    }
}