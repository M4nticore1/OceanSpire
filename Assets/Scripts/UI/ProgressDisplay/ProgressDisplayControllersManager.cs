using System.Collections.Generic;
using UnityEngine;

public class ProgressDisplayControllersManager : MonoBehaviour
{
    public static ProgressDisplayControllersManager Instance { get; private set; }

    private List<ProgressDisplayController> progressDisplayControllers = new();

    private void Awake()
    {
        if (Instance != null) {
            Debug.LogError($"[{nameof(ProgressDisplayControllersManager)}] There's another Progress Display Controller Manager on the scene!");
            Destroy(gameObject);
        }
        else {
            Instance = this;
        }
    }

    private void Update()
    {
        for (int i = progressDisplayControllers.Count - 1; i >= 0; i--) {
            var controller = progressDisplayControllers[i];
            if (controller == null) {
                progressDisplayControllers.RemoveAt(i);
                continue;
            }

            controller.Tick();
        }
    }

    public void RegisterController(ProgressDisplayController controller)
    {
        if (controller == null) return;
        if (progressDisplayControllers.Contains(controller)) return;

        progressDisplayControllers.Add(controller);
    }

    public void UnregisterController(ProgressDisplayController controller)
    {
        progressDisplayControllers.Remove(controller);
    }
}