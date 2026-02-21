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

    private void OnPrimaryInteractionReleased()
    {
        if (playerInputHandler.primaryInteractionPosition != playerInputHandler.primaryInteractionStartPosition) return;

        Interact(playerInputHandler.primaryInteractionPosition);
    }
}
