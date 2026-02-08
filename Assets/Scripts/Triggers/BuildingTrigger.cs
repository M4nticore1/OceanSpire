using UnityEngine;

public class BuildingTrigger : MonoBehaviour
{
    private Building building = null;

    private void Awake()
{
        building = transform.parent.GetComponent<Building>();
    }

    private void OnTriggerEnter(Collider other)
    {
        EntityCityNavigator entity = other.GetComponent<EntityCityNavigator>();

        if (entity) {
            if (building)
                entity.OnEnteredBuildingTrigger(building);
            else
                Debug.LogError("building is null.");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        EntityCityNavigator entity = other.GetComponent<EntityCityNavigator>();

        if (entity) {
            if (building)
                entity.OnStayBuildingTrigger(building);
            else
                Debug.LogError("building is null.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        EntityCityNavigator entity = other.GetComponent<EntityCityNavigator>();

        if (entity) {
            if (building)
                entity.OnExitedBuildingTrigger(building);
            else
                Debug.LogError("building is null.");
        }
    }
}
