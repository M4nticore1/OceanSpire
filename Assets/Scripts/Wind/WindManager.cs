using Unity.Mathematics;
using UnityEngine;

public class WindManager : MonoBehaviour
{
    public static WindManager Instance;

    [Header("Parameters")]
    [SerializeField] private float windSpeed = 15.0f;
    [SerializeField] private float windChangingSpeed = 0.05f;
    [SerializeField] private float windDirectionChangeFreqency = 300.0f;
    [SerializeField] private float windDirectionChangeTime = 0.0f;

    [field: Header("Check")]
    [field: SerializeField] public Vector3 WindDirection { get; private set; } = Vector3.zero;
    [SerializeField] private Vector3 newWindDirection = Vector3.zero;
    [field: SerializeField] public float windRotation { get; private set; } = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        ProcessChangingWind();
    }

    public void Init(WindData windData)
    {
        if (windData != null) {
            WindDirection = windData.WindDirection.Vector3();
        }
        else {
            ChangeWind();
            WindDirection = newWindDirection;
        }
    }

    private void ProcessChangingWind()
    {
        if (Time.time > windDirectionChangeTime + windDirectionChangeFreqency) {
            ChangeWind();
        }

        WindDirection = math.lerp(WindDirection, newWindDirection, windChangingSpeed * Time.deltaTime);
    }

    private void ChangeWind()
    {
        var x = UnityEngine.Random.Range(-1f, 1f);
        var y = UnityEngine.Random.Range(-1f, 1f);
        var z = UnityEngine.Random.Range(-1f, 1f);

        newWindDirection = new Vector3(x, y, z);
        newWindDirection.Normalize();

        windDirectionChangeTime = Time.time;
    }
}