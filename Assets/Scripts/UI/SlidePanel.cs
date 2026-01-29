using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum CloseMethod
{
    None,
    Click,
    OnePointClick
}

public class SlidePanel : MonoBehaviour, IInputListenable
{
    [SerializeField] private Canvas canvas;
    private RectTransform rectTransform;

    [Header("Slide")]
    [SerializeField] private CloseMethod closeMethod = CloseMethod.Click;
    [SerializeField] private float slideTransitionSpeed = 10f;

    [Header("Screen Position")]
    [SerializeField] private Vector2 openedScreenPositionAlpha = new Vector2(0f, 0.5f);
    [SerializeField] private Vector2 closedScreenPositionAlpha = new Vector2(0f, 0.0f);

    [Header("Panel Position")]
    [SerializeField] private Vector2 openedPanelPositionAlpha = new Vector2(0f, 0.5f);
    [SerializeField] private Vector2 closedPanelPositionAlpha = new Vector2(0f, 0.0f);

    [Header("Background")]
    [SerializeField] Image background;
    [SerializeField] float openedBackgroundAlpha = 0.9f;
    [SerializeField] float alphaTransitionSpeed = 10f;

    [Header("Buttons")]
    [SerializeField] private CustomButton openButton;
    [SerializeField] private CustomButton closeButton;

    private bool isOpened = false;
    private bool isMoving = false;
    private List<Transform> content = new List<Transform>();
    private Vector2 targetPosition = new Vector3();

    private Vector2 pressPossition;
    private Vector2 releasePossition;
    private int openedFrame = 0;
    public event Action onOpened;
    public event Action onClosed;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        InputListener.Instance.onPressed += OnPress;
        InputListener.Instance.onReleased += OnRelease;
        if (openButton)
            openButton.onReleased += OpenSlidePanel;
        if (closeButton)
            closeButton.onReleased += CloseSlidePanel;
    }

    private void OnDisable()
    {
        InputListener.Instance.onPressed -= OnPress;
        InputListener.Instance.onReleased -= OnRelease;
        if (openButton)
            openButton.onReleased -= OpenSlidePanel;
        if (closeButton)
            closeButton.onReleased -= CloseSlidePanel;
    }

    private void Update()
    {
        if (isMoving)
            UpdatePosition();
        if (background)
            UpdateBackground();
    }

    private void Start()
    {
        FillContent();

        if (background) {
            Color color = background.color;
            color.a = 0f;
            background.color = color;
            background.raycastTarget = false;
        }
    }

    public void OnPress()
    {
        pressPossition = PointerUtils.GetCurrentInputPosition();
    }

    public void OnRelease()
    {
        if (!isOpened) return;
        if (Time.frameCount == openedFrame) return;

        // Close Method
        if (closeMethod == CloseMethod.None) return;
        releasePossition = PointerUtils.GetCurrentInputPosition();
        if (closeMethod == CloseMethod.OnePointClick && releasePossition != pressPossition) return;

        TryToClose();
    }

    private void TryToClose()
    {
        List<RaycastResult> results = new List<RaycastResult>();
        PointerUtils.GetCurrentRaycastResults(results);
        if (IsClickedOutsideMenu(results)) {
            CloseSlidePanel();
        }
    }

    private void FillContent()
    {
        content = GetComponentsInChildren<Transform>(true).ToList();
        if (openButton)
            content.Add(openButton.transform);
        if (closeButton)
            content.Add(closeButton.transform);
    }

    public void OpenSlidePanel()
    {
        ApplyTagetPositionByAlpha(openedScreenPositionAlpha, openedPanelPositionAlpha);

        openedFrame = Time.frameCount;

        if (background) {
            background.raycastTarget = true;
            content.Add(background.transform);
        }

        isOpened = true;
        isMoving = true;
        onOpened?.Invoke();
    }

    public void CloseSlidePanel()
    {
        ApplyTagetPositionByAlpha(closedScreenPositionAlpha, closedPanelPositionAlpha);

        if (background) {
            background.raycastTarget = false;
        }

        isOpened = false;
        isMoving = true;
        onClosed?.Invoke();
    }

    private void UpdatePosition()
    {
        rectTransform.anchoredPosition = math.lerp(rectTransform.anchoredPosition, targetPosition, slideTransitionSpeed * Time.deltaTime);
        if (rectTransform.anchoredPosition == targetPosition)
            isMoving = false;
    }

    private void UpdateBackground()
    {
        Color color = background.color;
        if (isOpened)
            color.a = math.lerp(color.a, openedBackgroundAlpha, alphaTransitionSpeed * Time.deltaTime);
        else
            color.a = math.lerp(color.a, 0f, alphaTransitionSpeed * Time.deltaTime);
        background.color = color;
    }

    public void SetOpenButton(CustomButton button)
    {
        if (openButton) {
            content.Remove(openButton.transform);
        }
        openButton = button;
        content.Add(openButton.transform);
    }

    public void SetCloseButton(CustomButton button)
    {
        if (closeButton) {
            content.Remove(closeButton.transform);
        }
        closeButton = button;
        content.Add(closeButton.transform);
    }

    private bool IsClickedOutsideMenu(List<RaycastResult> results)
    {
        foreach (var hit in results) {
            if (hit.gameObject.transform.IsChildOf(transform)) {
                return false;
            }
        }
        return true;
    }

    private void ApplyTagetPositionByAlpha(Vector2 screenPostionAlpha, Vector2 panelPostionAlpha)
    {
        Vector2 resolution = new Vector2(Screen.width, Screen.height) / canvas.scaleFactor;
        float positionX = resolution.x * screenPostionAlpha.x;
        float positionY = resolution.y * screenPostionAlpha.y;

        Vector2 size = rectTransform.rect.size;
        float sizeCorrectionX = size.x * panelPostionAlpha.x;
        float sizeCorrectionY = size.y * panelPostionAlpha.y;

        targetPosition = new Vector2(positionX, positionY) + new Vector2(sizeCorrectionX, sizeCorrectionY);
    }
}
