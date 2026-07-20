using UnityEngine;

public class BoatOverweightDisplay : MonoBehaviour
{
    [SerializeField] private Boat boat;
    [SerializeField] private GameObject content;

    private void OnEnable()
    {
        boat.OnStateEntered += OnBoatStateEntered;
    }

    private void OnDisable()
    {
        boat.OnStateEntered -= OnBoatStateEntered;
    }

    private void UpdateMenu()
    {
        content.SetActive(boat.CurrentStateEnum == BoatStateEnum.MovingToDock && boat.IsOverweight());
    }

    private void OnBoatStateEntered(BoatState boatState)
    {
        UpdateMenu();
    }
}