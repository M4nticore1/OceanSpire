using UnityEngine;

public class BuildingTrigger : MonoBehaviour
{
    private Building building;

    private void Awake()
{
        building = transform.parent.GetComponent<Building>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Creature entity = other.GetComponent<Creature>();

        if (entity && building) {
            if (!entity.IsRidingOnElevator) {
                entity.EnterBuilding(building);
                entity.DecideAction();
            }
        }
        else {
            if (!building) {
                Debug.LogError("BuildingTrigger: Building is NULL");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Creature entity = other.GetComponent<Creature>();

        if (entity) {
            if (entity.CurrentBuilding == building)
                entity.ExitBuilding();
        }
    }
}
