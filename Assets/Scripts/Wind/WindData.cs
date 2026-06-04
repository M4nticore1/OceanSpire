using UnityEngine;

public class WindData
{
    public Vector3Data WindDirection;

    public static WindData Create(WindManager windManager)
    {
        return new WindData()
        {
            WindDirection = new Vector3Data(windManager.WindDirection)
        };
    }
}