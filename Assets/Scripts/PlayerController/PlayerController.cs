using UnityEngine;

public class PlayerEntry
{
    public Vector3 cameraRotation;
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerUIManager uiManager;
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private CameraZoomHandler cameraZoomHandler;

    private void OnEnable()
    {
        inputHandler.onCameraZoomPerformed += OnCameraZoomPerformed;
    }

    private void OnDisable()
    {
        inputHandler.onCameraZoomPerformed -= OnCameraZoomPerformed;
    }

    private void Update()
    {
        ProcessCameraMove();
        ProcessCameraZoom();
    }

    private void ProcessCameraMove()
    {
        if (!CanMoveCamera()) return;

        cameraMovement.Tick();
    }

    private void ProcessCameraZoom()
    {
        if (!CanMoveCamera()) return;

        cameraZoomHandler.Tick();
    }

    private bool CanMoveCamera()
    {
        return !uiManager.isWorkersMenuOpened && !uiManager.isManagementMenuOpened;
    }

    private void OnCameraZoomPerformed(float value)
    {
        cameraZoomHandler.AddZoomVelocity(value);
    }
}
