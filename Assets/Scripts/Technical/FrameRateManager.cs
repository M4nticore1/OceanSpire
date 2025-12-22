using UnityEngine;
using System.Collections;
using System.Threading;

public class FrameRateManager : MonoBehaviour
{
    [SerializeField] private int targetFrameRate = 60;
    private int maxRate = 9999;
    private float currentFrameTime = 0;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = maxRate;
        currentFrameTime = Time.realtimeSinceStartup;
        StartCoroutine("WaitForNextFrame");
    }

    private IEnumerator WaitForNextFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            currentFrameTime += 1f / targetFrameRate;
            float t = Time.realtimeSinceStartup;
            float sleepTime = currentFrameTime - t - 0.01f;

            if (sleepTime > 0)
                Thread.Sleep((int)(sleepTime * 1000));
            
            while (t < currentFrameTime)
                t = Time.realtimeSinceStartup;
        }
    }
}
