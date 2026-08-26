using System.Collections.Generic;
using UnityEngine;

public class PulseEffectsManager : MonoBehaviour
{
    public static PulseEffectsManager Instance { get; private set; }

    private List<PulseEffect> pulseEffects = new();

    private void Awake()
    {
        if (Instance != null) {
            Debug.LogError($"[{nameof(ProgressDisplayControllersManager)}] There's another Pulse Effects Manager on the scene!");
            Destroy(gameObject);
        }
        else {
            Instance = this;
        }
    }

    private void Update()
    {
        for (int i = pulseEffects.Count - 1; i >= 0; i--) {
            var effect = pulseEffects[i];
            if (effect == null) {
                pulseEffects.RemoveAt(i);
                continue;
            }

            effect.Tick();
        }
    }

    public void RegisterEffect(PulseEffect effect)
    {
        if (effect == null) return;
        if (pulseEffects.Contains(effect)) return;

        pulseEffects.Add(effect);
    }

    public void UnregisterEffect(PulseEffect effect)
    {
        pulseEffects.Remove(effect);
    }
}