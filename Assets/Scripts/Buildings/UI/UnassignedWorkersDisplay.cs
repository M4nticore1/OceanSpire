using System;
using System.Collections;
using UnityEngine;

public class UnassignedWorkersDisplay : MonoBehaviour, IClickable
{
    [SerializeField] private GameObject content;
    [SerializeField] private Collider collider;
    [SerializeField] private BuildingConstruction buildingConstruction;

    private bool isShown => content.activeSelf;
    private Building ownedBuilding;
    private ConstructionComponent constructionComponent;
    private BuildingCitizensHandler citizensHandler;

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

    private Coroutine updateShownCoroutine;
    public event Action OnClicked;

    private void OnEnable()
    {
        buildingConstruction.OnInit += HandleConstructionInit;
    }

    private void OnDisable()
    {
        buildingConstruction.OnInit -= HandleConstructionInit;
    }

    public void Click()
    {
        var workersMenu = WorkersControlMenu.Instance;
        if (workersMenu == null) return;

        workersMenu.Show(ownedBuilding);
        OnClicked?.Invoke();
    }

    public bool ShouldClick()
    {
        return IsClickable;
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        citizensHandler.OnInteractorAdded += HandleWorkerAdded;
        citizensHandler.OnInteractorRemoved += HandleWorkerRemoved;

        constructionComponent.OnConstructionStarted += HandleConstructionStarted;
        constructionComponent.OnConstructionFinished += HandleConstructionFinished;
    }

    private void Unsubscribe()
    {
        if (citizensHandler != null) {
            citizensHandler.OnInteractorAdded -= HandleWorkerAdded;
            citizensHandler.OnInteractorRemoved -= HandleWorkerRemoved;
        }

        if (constructionComponent != null) {
            constructionComponent.OnConstructionStarted -= HandleConstructionStarted;
            constructionComponent.OnConstructionFinished -= HandleConstructionFinished;
        }
    }

    private void Show()
    {
        if (isShown) return;

        content.SetActive(true);
        collider.enabled = true;
    }

    private void Hide()
    {
        if (!isShown) return;

        content.SetActive(false);
        collider.enabled = false;
    }

    private void RunUpdateShownCoroutine()
    {
        if (updateShownCoroutine == null) {
            updateShownCoroutine = StartCoroutine(UpdateShownCoroutine());
        }
    }

    private void UpdateShown()
    {
        if (ShouldShow()) {
            Show();
        }
        else {
            Hide();
        }
    }

    private void HandleConstructionInit()
    {
        ownedBuilding = buildingConstruction.OwnedBuilding;
        constructionComponent = ownedBuilding.ConstructionComponent;
        citizensHandler = ownedBuilding.CitizensHandler;

        Subscribe();
        UpdateShown();
    }

    private void HandleWorkerAdded(Human human)
    {
        RunUpdateShownCoroutine();
    }

    private void HandleWorkerRemoved(Human human)
    {
        RunUpdateShownCoroutine();
    }

    private void HandleConstructionStarted()
    {
        RunUpdateShownCoroutine();
    }

    private void HandleConstructionFinished()
    {
        RunUpdateShownCoroutine();
    }

    private bool ShouldShow()
    {
        if (!ownedBuilding.Definition.IsWorkable) return false;
        if (constructionComponent.IsUnderConstruction) return false;
        if (ownedBuilding.LevelDefinition.MaxHumansCount <= 0) return false;

        return citizensHandler.Interactors.Count <= 0;
    }

    private IEnumerator UpdateShownCoroutine()
    {
        yield return null;

        updateShownCoroutine = null;
        UpdateShown();
    }
}