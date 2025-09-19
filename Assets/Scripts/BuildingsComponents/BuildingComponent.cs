using UnityEngine;

[AddComponentMenu("")]
public class BuildingComponent : MonoBehaviour
{
    protected GameManager gameManager = null;
    protected CityManager cityManager = null;
    [HideInInspector] public Building ownedBuilding = null;

    protected virtual void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        cityManager = FindAnyObjectByType<CityManager>();
        ownedBuilding = GetComponent<Building>();
    }

    public virtual void Build()
    {

    }

    public virtual void LevelUp()
    {

    }
}
