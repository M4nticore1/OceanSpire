using System.Collections;
using UnityEngine;

public class UnassignedWorkersDisplay : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private BuildingConstruction buildingConstruction;
    [SerializeField] private CustomButton openWorkersMenuButton;

    private bool isShown => content.activeSelf;
    private Building ownedBuilding;
    private ConstructionComponent constructionComponent;
    private BuildingCitizensHandler citizensHandler;

    private Coroutine updateShownCoroutine;

    private void OnEnable()
    {
        buildingConstruction.OnInit += HandleConstructionInit;
        openWorkersMenuButton.OnReleased.AddListener(HandleOpenWorkersMenuButtonClicked);
    }

    private void OnDisable()
    {
        buildingConstruction.OnInit -= HandleConstructionInit;
        openWorkersMenuButton.OnReleased.RemoveListener(HandleOpenWorkersMenuButtonClicked);
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
        content.SetActive(true);
    }

    private void Hide()
    {
        content.SetActive(false);
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
        else if (ShouldHide()) {
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

    private void HandleOpenWorkersMenuButtonClicked()
    {
        var workersMenu = WorkersControlMenu.Instance;
        if (workersMenu == null) return;

        workersMenu.Show(ownedBuilding);
    }

    private bool ShouldShow()
    {
        if (isShown) return false;
        if (ownedBuilding.ConstructionComponent.IsUnderConstruction) return false;
        if (ownedBuilding.LevelDefinition.MaxHumansCount <= 0) return false;

        return citizensHandler.Interactors.Count <= 0;
    }

    private bool ShouldHide()
    {
        if (!isShown) return false;
        if (ownedBuilding.ConstructionComponent.IsUnderConstruction) return true;
        if (ownedBuilding.LevelDefinition.MaxHumansCount <= 0) return true;

        return citizensHandler.Interactors.Count > 0;
    }

    private IEnumerator UpdateShownCoroutine()
    {
        yield return null;

        updateShownCoroutine = null;
        UpdateShown();
    }
}