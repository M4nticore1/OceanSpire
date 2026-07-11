using UnityEngine;

public class SpawnArea : MonoBehaviour
{
    [SerializeField] private Vector2 areaSize = Vector2.zero;

    public Vector3 GetRandomSpawnPosition()
    {
        var x = UnityEngine.Random.Range(-areaSize.x / 2, areaSize.x / 2);
        var y = transform.position.y;
        var z = UnityEngine.Random.Range(-areaSize.y / 2, areaSize.y / 2);

        var localPosition = new Vector3(x, y, z);

        return transform.TransformPoint(localPosition);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(areaSize.x, 0, areaSize.y));
    }
}