using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CameraMovement cameraMovement;
    public CameraMovement CameraMovement => cameraMovement;

    public void Init()
    {
        var playerControllerData = PlayerControllerData.Create(this);

        Init(playerControllerData);
    }

    public void Init(PlayerControllerData playerControllerData)
    {
        if (playerControllerData == null) {
            Debug.LogError("playerControllerData is not valid");
            return;
        }

        CameraMovement.Init(Quaternion.Euler(playerControllerData.cameraRotation.Vector3()));
    }
}