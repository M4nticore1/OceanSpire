using System.Collections;
using UnityEngine;

public class BuildingTrigger : MonoBehaviour
{
    private Building building;
    private bool isInited = false;

    private void Awake()
    {
        building = transform.parent.GetComponent<Building>();
    }

    private void Start()
    {
        StartCoroutine(InitNextFrame());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isInited) return;

        var entity = other.GetComponent<CreatureCityNavigator>();
        if (entity == null) return;

        if (building != null)
            entity.OnEnteredBuildingTrigger(building);
        else
            Debug.LogError("building is null.");
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isInited) return;

        var entity = other.GetComponent<CreatureCityNavigator>();
        if (entity == null) return;

        if (building != null)
            entity.OnStayBuildingTrigger(building);
        else
            Debug.LogError("building is null.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isInited) return;

        var entity = other.GetComponent<CreatureCityNavigator>();
        if (entity == null) return;

        if (building != null)
            entity.OnExitedBuildingTrigger(building);
        else
            Debug.LogError("building is null.");
    }

    private IEnumerator InitNextFrame()
    {
        yield return new WaitForFixedUpdate();
        isInited = true;
    }
}