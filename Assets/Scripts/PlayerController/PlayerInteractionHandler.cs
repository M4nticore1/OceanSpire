using UnityEngine;

public class PlayerInteractionHandler : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInputHandler = null;

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
        if (playerInputHandler.primaryInteractionPosition != playerInputHandler.primaryInteractionStartPosition) return;

        Interact(playerInputHandler.primaryInteractionPosition);
    }
}
