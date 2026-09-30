using UnityEngine;

public class WindData
{
    public Vector3Data WindDirection = Vector3Data.Zero();

    public static WindData Random()
    {
        var direction = new Vector3(UnityEngine.Random.Range(0f, 1f), 0, UnityEngine.Random.Range(0f, 1f)).normalized;

        return new WindData()
        {
            WindDirection = new Vector3Data(direction)
        };
    }

    public static WindData Create(WindManager windManager)
    {
        return new WindData()
        {
            WindDirection = new Vector3Data(windManager.WindDirection)
        };
    }
}