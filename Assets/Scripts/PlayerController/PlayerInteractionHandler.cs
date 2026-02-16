using UnityEngine;

public class PlayerInteractionHandler : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInputHandler = null;
    private Vector2 pressedPosition;

    private void OnEnable()
    {
        playerInputHandler.onPrimaryInteractionPressed += OnPrimaryInteractionPressed;
        playerInputHandler.onPrimaryInteractionReleased += OnPrimaryInteractionReleased;
    }

    private void OnDisable()
    {
        playerInputHandler.onPrimaryInteractionPressed -= OnPrimaryInteractionPressed;
        playerInputHandler.onPrimaryInteractionReleased -= OnPrimaryInteractionReleased;
    }

    private void Interact(Vector2 interactionPosition)
    {
        if (PointerUtils.GetCurrentRaycastResult().gameObject) return;

        Ray ray = Camera.main.ScreenPointToRay(interactionPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            GameObject hitted = hit.collider.gameObject;

            if (hit.collider.TryGetComponent<IClickable>(out var clickable)) {
                clickable.Click();
            }
            else {
                SelectManager.Instance.selectedComponent?.Click();
            }
        }
    }

    private void OnPrimaryInteractionPressed(Vector2 interactionPosition)
    {
        pressedPosition = interactionPosition;
    }

    private void OnPrimaryInteractionReleased(Vector2 interactionPosition)
    {
        if (interactionPosition != pressedPosition) return;

        Interact(interactionPosition);
    }
}
