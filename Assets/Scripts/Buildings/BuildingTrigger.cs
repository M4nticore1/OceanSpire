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
        Entity entity = other.GetComponent<Entity>();

        if (entity && building) {
            if (!entity.isRidingOnElevator) {
                entity.EnterBuilding(building);
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
        Entity entity = other.GetComponent<Entity>();

        if (entity) {
            if (entity.currentBuilding == building)
                entity.ExitBuilding();
        }
    }
}
