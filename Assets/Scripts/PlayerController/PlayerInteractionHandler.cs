using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInteractionHandler : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerInputHandler playerInputHandler;

    private void OnEnable()
    {
        playerInputHandler.OnPrimaryInteractionReleased += OnPrimaryInteractionReleased;
    }

    private void OnDisable()
    {
        playerInputHandler.OnPrimaryInteractionReleased -= OnPrimaryInteractionReleased;
    }

    private void Interact(Vector2 interactionPosition)
    {
        if (PointerUtils.GetRaycastHit(out var hit)) {
            var go = hit.gameObject;
            if (go == null) return;

            if (!IsHittedUI() && !IsPointerAtStartPosition()) return;

            var clickables = go.GetComponents<IClickable>();
            foreach (var clickable in clickables) {
                if (clickable == null) continue;
                if (!clickable.ShouldClick()) continue;

                clickable.Click();
            }

            EventBus.InvokeClicked(hit.gameObject);
        }
        else {
            EventBus.InvokeClicked(null);
        }
    }

    private void OnPrimaryInteractionReleased()
    {
        Interact(playerInputHandler.primaryInteractionPosition);
    }

    private bool IsHittedUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    private bool IsPointerAtStartPosition()
    {
        return playerInputHandler.primaryInteractionPosition == playerInputHandler.primaryInteractionStartPosition;
    }
}
