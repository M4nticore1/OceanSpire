using UnityEngine;

public class PierWorkersUnassignedMessageController : MonoBehaviour
{
    [SerializeField] private PierWorkersUnassignedMessage pierWorkersUnassignedMessage;
    [SerializeField] private Building pierBuilding;

    private void OnEnable()
    {
        pierBuilding.CitizensHandler.OnInteractorAdded += HandlePierInteractorAdded;
        SwimmingDriftingLoot.OnLootFocusChanged += HandleLootFocusedChanged;
    }

    private void OnDisable()
    {
        if (pierBuilding && pierBuilding.CitizensHandler) {
            pierBuilding.CitizensHandler.OnInteractorAdded -= HandlePierInteractorAdded;
        }
        SwimmingDriftingLoot.OnLootFocusChanged -= HandleLootFocusedChanged;
    }

    private void Start()
    {
        pierWorkersUnassignedMessage.Hide();
    }

    private void Update()
    {
        pierWorkersUnassignedMessage.Tick();
    }

    private void UpdateShown()
    {
        if (ShouldShown()) {
            pierWorkersUnassignedMessage.Show();
        }
        else {
            pierWorkersUnassignedMessage.Hide();
        }
    }

    private void HandleLootFocusedChanged(SwimmingDriftingLoot loot, bool focused)
    {
        if (loot == null) return;
        if (!focused) return;

        UpdateShown();
    }

    private void HandlePierInteractorAdded(Human human)
    {
        if (human == null) return;

        UpdateShown();
    }

    private bool ShouldShown()
    {
        if (pierBuilding.CitizensHandler.Interactors.Count > 0) return false;

        return true;
    }
}