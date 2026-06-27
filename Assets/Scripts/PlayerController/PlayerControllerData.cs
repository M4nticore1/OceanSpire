using System;
using UnityEngine;

[Serializable]
public class PlayerControllerData
{
    public Vector3Data cameraRotation;

    public static PlayerControllerData Create(PlayerController playerController)
    {
        return new PlayerControllerData()
        {
            cameraRotation = new Vector3Data(playerController.CameraMovement.transform.rotation.eulerAngles)
        };
    }
}