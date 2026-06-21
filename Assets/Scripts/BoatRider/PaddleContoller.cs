using UnityEngine;

public class PaddleContoller : MonoBehaviour
{
    [SerializeField] private BoatRider boatRider;
    [SerializeField] private EquipmentComponent equipmentComponent;
    [SerializeField] private GameObject paddle;

    private void Awake()
    {
        SetPaddleVisible(false);
    }

    private void OnEnable()
    {
        boatRider.OnBoatMovementStarted += OnBoatMovementStarted;
        boatRider.OnBoatMovementStopped += OnBoatMovementStopped;
    }

    private void OnDisable()
    {
        boatRider.OnBoatMovementStarted -= OnBoatMovementStarted;
        boatRider.OnBoatMovementStopped -= OnBoatMovementStopped;
    }

    private void SetPaddleVisible(bool value)
    {
        paddle.SetActive(value);
        equipmentComponent.SetCurrentEquipmentVisible(!value);
    }

    private void OnBoatMovementStarted(Boat boat)
    {
        SetPaddleVisible(true);
    } 

    private void OnBoatMovementStopped(Boat boat)
    {
        SetPaddleVisible(false);
    }
}