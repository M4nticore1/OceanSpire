using Unity.Mathematics;
using UnityEngine;

public class WindManager : MonoBehaviour
{
    public static WindManager Instance;

    public Vector3 windDirection { get; private set; } = Vector3.zero;
    public float windRotation { get; private set; } = 0;
    private Vector3 newWindDirection = Vector3.zero;

    public const float windSpeed = 15.0f;
    private const float windChangingSpeed = 0.05f;
    private float windDirectionChangeFreqency = 300.0f;
    private float windDirectionChangeTime = 0.0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ChangeWind();
        windDirection = newWindDirection;
    }

    private void Update()
    {
        //ChangingWind();
    }

    private void ChangingWind()
    {
        if (Time.time > windDirectionChangeTime + windDirectionChangeFreqency) {
            ChangeWind();
        }
        windDirection = math.lerp(windDirection, newWindDirection, windChangingSpeed * Time.deltaTime);
    }

    private void ChangeWind()
    {
        float x = UnityEngine.Random.Range(-1f, 1f);
        float y = UnityEngine.Random.Range(-1f, 1f);
        float z = UnityEngine.Random.Range(-1f, 1f);
        newWindDirection = new Vector3(x, y, z);
        newWindDirection.Normalize();

        windDirectionChangeTime = Time.time;
    }
}
