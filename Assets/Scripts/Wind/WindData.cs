using System;
using UnityEngine;

[Serializable]
public class WindData
{
    public Vector3Data WindDirection = Vector3Data.Zero();
    public Vector3Data TargetWindDirection = Vector3Data.Zero();
    public float CurrentWindDirectionChangeFreqency = 0f;
    public float CurrentWindDirectionChangeTime = 0f;

    public static WindData Default()
    {
        return new WindData();
    }

    public static WindData Random(WindManager windManager)
    {
        if (windManager == null) {
            Debug.LogError($"[{nameof(WindData)}] WindManager is not valid!");
            return null;
        }

        var direction = new Vector3(UnityEngine.Random.Range(0f, 1f), 0, UnityEngine.Random.Range(0f, 1f)).normalized;

        return new WindData()
        {
            WindDirection = new Vector3Data(direction),
            TargetWindDirection = new Vector3Data(direction),
            CurrentWindDirectionChangeFreqency = WindManager.Instance != null ? WindManager.Instance.GetRandomDirectionChangeFreqency() : 0f,
        };
    }

    public static WindData Create(WindManager windManager)
    {
        if (windManager == null) {
            Debug.LogError($"[{nameof(WindData)}] WindManager is not valid!");
            return null;
        }

        return new WindData()
        {
            WindDirection = new Vector3Data(windManager.WindDirection),
            TargetWindDirection = new Vector3Data(windManager.TargetWindDirection),
            CurrentWindDirectionChangeFreqency = windManager.CurrentWindDirectionChangeFreqency,
            CurrentWindDirectionChangeTime = windManager.CurrentWindDirectionChangeTime
        };
    }
}