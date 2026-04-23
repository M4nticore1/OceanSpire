using UnityEngine;

public class PlayerInteractionHandler : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInputHandler = null;

    private void OnEnable()
    {
        playerInputHandler.onPrimaryInteractionReleased += OnPrimaryInteractionReleased;
    }

    private void OnDisable()
    {
        playerInputHandler.onPrimaryInteractionReleased -= OnPrimaryInteractionReleased;
    }

    private void Interact(Vector2 interactionPosition)
    {
        if (PointerUtils.GetRaycastUIResult().gameObject) return;

        if (PointerUtils.GetRaycastColliderHit(out var hit)) {
            IClickable[] clickables = hit.collider.GetComponents<IClickable>();

            foreach (IClickable clickable in clickables) {
                if (!clickable.ShouldClick()) continue;

                clickable.Click();
            }

            EventBus.InvokeClicked(hit.collider.gameObject);
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
