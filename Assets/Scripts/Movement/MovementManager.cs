using System.Collections.Generic;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    public static MovementManager Instance { get; private set; }

    private List<Movement> movementComponents = new();
    public IReadOnlyList<Movement> MovementComponents => movementComponents;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Debug.LogError($"[{nameof(MovementManager)}] There's another Movement Manager in the scene!");
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        for (int i = movementComponents.Count - 1; i >= 0; i--) {
            var movement = movementComponents[i];
            if (movement == null) {
                movementComponents.RemoveAt(i);
                continue;
            }

            movement.Tick();
        }
    }

    public void RegisterMovement(Movement movement)
    {
        if (movement == null) return;
        if (movementComponents.Contains(movement)) return;

        movementComponents.Add(movement);
    }

    public void UnregisterMovement(Movement movement)
    {
        if (movement == null) return;

        movementComponents.Remove(movement);
    }
}