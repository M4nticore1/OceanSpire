using UnityEngine;

public class WindManager : MonoBehaviour
{
    public static WindManager Instance;

    [Header("Speed")]
    [SerializeField] private float windSpeed = 15.0f;
    [SerializeField] private float windChangingSpeed = 0.05f;

    [Header("Change Direction")]
    [SerializeField] private float minWindDirectionChangeFreqency = 300.0f;
    [SerializeField] private float maxWindDirectionChangeFreqency = 600.0f;

    [SerializeField] private float currentWindDirectionChangeFreqency = 0.0f;
    public float CurrentWindDirectionChangeFreqency => currentWindDirectionChangeFreqency;

    [SerializeField] private float currentWindDirectionChangeTime = 0.0f;
    public float CurrentWindDirectionChangeTime => currentWindDirectionChangeTime;

    [field: Header("Check")]
    [field: SerializeField] public Vector3 WindDirection { get; private set; } = Vector3.forward;
    [field: SerializeField] public Vector3 TargetWindDirection { get; private set; } = Vector3.forward;
    [field: SerializeField] public float WindRotation { get; private set; } = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        ProcessChangingWind();
    }

    public void Init()
    {
        var randomData = WindData.Random(this);

        if (randomData != null) {
            Init(randomData);
        }
        else {
            Init(WindData.Default());
        }
    }

    public void Init(WindData windData)
    {
        if (windData == null) {
            Debug.LogError($"[{nameof(WindManager)}] WindData is not valid!");
            Init();
            return;
        }

        WindDirection = windData.WindDirection.Vector3();
        TargetWindDirection = windData.TargetWindDirection.Vector3();
        currentWindDirectionChangeFreqency = windData.CurrentWindDirectionChangeFreqency;
        currentWindDirectionChangeTime = windData.CurrentWindDirectionChangeTime;
    }

    public float GetRandomDirectionChangeFreqency()
    {
        return UnityEngine.Random.Range(minWindDirectionChangeFreqency, maxWindDirectionChangeFreqency);
    }

    private void ProcessChangingWind()
    {
        currentWindDirectionChangeTime += Time.deltaTime;

        if (currentWindDirectionChangeTime >= currentWindDirectionChangeFreqency) {
            ChangeWind();
            currentWindDirectionChangeFreqency = GetRandomDirectionChangeFreqency();
            currentWindDirectionChangeTime = 0f;
        }

        WindDirection = Vector3.Lerp(WindDirection, TargetWindDirection, windChangingSpeed * Time.deltaTime);
    }

    private void ChangeWind()
    {
        var x = UnityEngine.Random.Range(-1f, 1f);
        var y = UnityEngine.Random.Range(-1f, 1f);
        var z = UnityEngine.Random.Range(-1f, 1f);

        TargetWindDirection = new Vector3(x, y, z).normalized;
    }
}