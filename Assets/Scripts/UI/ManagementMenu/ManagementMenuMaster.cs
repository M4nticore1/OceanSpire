using UnityEngine;

public class ManagementMenuMaster : MonoBehaviour
{
    [SerializeField] private GameObject content;

    private void OnEnable()
    {
        EventBus.OnConstructionStarted += OnConstructionStarted;
    }

    private void OnDisable()
    {
        EventBus.OnConstructionStarted -= OnConstructionStarted;
    }

    private void Open()
    {
        content.SetActive(true);
    }

    private void Close()
    {
        content.SetActive(false);
    }

    private void OnConstructionStarted(Building building)
    {
        Close();
    }
}