#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;

public class SuperScreenshot : MonoBehaviour
{
    [SerializeField] private int resolutionMultiplier = 4;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame) {
            TakeScreenshot();
        }
    }

    private void TakeScreenshot()
    {
        int width = Screen.width * resolutionMultiplier;
        int height = Screen.height * resolutionMultiplier;

        RenderTexture rt = new RenderTexture(width, height, 24);
        Camera.main.targetTexture = rt;

        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        Camera.main.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/Screenshot.png", bytes);

        Debug.Log($"Screenshot has been saved by path {Application.dataPath} + /Screenshot.png!");
    }
}
#endif