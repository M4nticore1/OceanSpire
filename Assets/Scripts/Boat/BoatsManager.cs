using System.Collections.Generic;
using UnityEngine;

public class BoatsManager : MonoBehaviour
{
    public static BoatsManager Instance { get; private set; } = null;

    public List<Boat> citizenBoats { get; private set; } = new List<Boat>();

    private void OnEnable()
    {
        EventBus.onBoatCreated += OnBoatCreated;
    }

    private void OnDisable()
    {
        EventBus.onBoatCreated -= OnBoatCreated;
    }

    private void Awake()
    {
        Instance = this;
    }

    public Boat GetBoatByInteractorIndex(int index)
    {
        if (citizenBoats.Count <= index) return null;

        Boat boat = citizenBoats[index];
        return boat;
    }

    private void OnBoatCreated(Boat boat)
    {
        citizenBoats.Add(boat);
    }
}
