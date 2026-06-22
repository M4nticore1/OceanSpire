using UnityEngine;
using UnityEngine.UI;

public class NotificationsPanel : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup layoutGroup;
    public GridLayoutGroup LayoutGroup => layoutGroup;

    public void AddNotification(GameObject notification)
    {
        notification.transform.SetParent(layoutGroup.transform);
    }
}