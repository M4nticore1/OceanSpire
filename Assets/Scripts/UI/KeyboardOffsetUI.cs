using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class KeyboardOffsetUI : MonoBehaviour
{
    private RectTransform panel;
    [SerializeField] private float openedPositionOffsetPercent;
    [SerializeField] private float toggleSpeed = 1f;

    private float moveAlpha = 0f;
    private float startHeight = 0;
    private float targetHeight = 0;
    private bool isOpened = false;

    private void Awake()
    {
        panel = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (!TouchScreenKeyboard.isSupported) return;

        if (TouchScreenKeyboard.visible) {
            if (!isOpened) {
                HandleKeyboardOpened();
            }

            if (moveAlpha < 1f) {
                moveAlpha += toggleSpeed * Time.deltaTime;
                moveAlpha = math.clamp(moveAlpha, 0f, 1f);
            }
        }
        else {
            if (isOpened) {
                HandleKeyboardClosed();
            }

            if (moveAlpha > 0f) {
                moveAlpha -= toggleSpeed * Time.deltaTime;
                moveAlpha = math.clamp(moveAlpha, 0f, 1f);
            }
        }

        MoveTo(targetHeight);
    }

    private void MoveTo(float targetHeight)
    {
        float width = panel.anchoredPosition.x;
        float height = panel.anchoredPosition.y;

        if (TouchScreenKeyboard.visible) {
            float heightOffset = panel.rect.size.y * openedPositionOffsetPercent;
            height = math.lerp(0, targetHeight + heightOffset, moveAlpha);
        }
        else {
            height = math.lerp(startHeight, 0f, 1f - moveAlpha);
        }

        panel.anchoredPosition = new Vector2(width, height);
    }

    private float GetOverlapHeight()
    {
        float keyboardTop = GetKeyboardHeight();
        float bottom = panel.position.y - panel.rect.size.y * panel.pivot.y;
        float overlap = keyboardTop - bottom;

        return Mathf.Max(0f, overlap);
    }

    private void HandleKeyboardOpened()
    {
        targetHeight = GetOverlapHeight();
        isOpened = true;
    }

    private void HandleKeyboardClosed()
    {
        targetHeight = 0f;
        startHeight = panel.anchoredPosition.y;
        isOpened = false;
    }

    private int GetKeyboardHeight()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
            AndroidJavaObject View = UnityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");

            using (AndroidJavaObject Rct = new AndroidJavaObject("android.graphics.Rect")) {
                View.Call("getWindowVisibleDisplayFrame", Rct);

                return Screen.height - Rct.Call<int>("height");
            }
        }
#else
        return 0;
#endif
    }
}