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

public class SlidePanel : MonoBehaviour, IInputListenable, IOpenable
{
    [SerializeField] private Canvas canvas;
    private RectTransform rectTransform;

    [Header("Slide")]
    [SerializeField] private CloseMethod closeMethod = CloseMethod.Click;
    [SerializeField] private float slideTransitionSpeed = 10f;
    [SerializeField] RectTransform contentRoot;
    [SerializeField] private bool hideWhenClosed = false;

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

    private bool isOpened = false;
    private bool isMoving = false;
    private List<Transform> content = new List<Transform>();

    private Vector2 openedPosition;
    private Vector2 closedPosition;
    private Vector2 targetPosition;

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
    }

    private void OnDisable()
    {
        InputListener.Instance.onPressed -= OnPress;
        InputListener.Instance.onReleased -= OnRelease;
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

        AssignContentRootEnabled();
    }

    private void Update()
    {
        if (isMoving) {
            ProcessMoving();

            if (rectTransform.anchoredPosition == targetPosition) {
                AssignContentRootEnabled();
                SetMoving(false);
            }
        }

        if (background) {
            UpdateBackground();
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
        if (InputListener.Instance.startPressedObject != PointerUtils.GetRaycastUIResult().gameObject) return;

        TryToClose();
    }

    private void TryToClose()
    {
        List<RaycastResult> results = new List<RaycastResult>();
        PointerUtils.GetRaycastUIResults(results);
        if (IsClickedOutsideMenu(results)) {
            Close();
        }
    }

    private void FillContent()
    {
        content = GetComponentsInChildren<Transform>(true).ToList();
    }

    public void Open()
    {
        openedPosition = CalculateOpenedPosition();
        targetPosition = openedPosition;

        openedFrame = Time.frameCount;

        if (background) {
            background.raycastTarget = true;
            content.Add(background.transform);
        }

        if (hideWhenClosed) {
            SetContentRootEnabled(true);
        }

        isOpened = true;
        isMoving = true;
        onOpened?.Invoke();
    }

    public void Close()
    {
        closedPosition = CalculateClosedPosition();
        targetPosition = closedPosition;

        if (background) {
            background.raycastTarget = false;
        }

        isOpened = false;
        isMoving = true;
        onClosed?.Invoke();
    }

    private void SetMoving(bool value)
    {
        isMoving = value;
    }

    private void ProcessMoving()
    {
        rectTransform.anchoredPosition = math.lerp(rectTransform.anchoredPosition, targetPosition, slideTransitionSpeed * Time.deltaTime);

        Vector2 min = math.min(openedPosition, closedPosition);
        Vector2 max = math.max(openedPosition, closedPosition);

        rectTransform.anchoredPosition = math.clamp(rectTransform.anchoredPosition, min, max);
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

    private void AssignContentRootEnabled()
    {
        if (!hideWhenClosed) return;
        if (rectTransform.anchoredPosition != targetPosition) return;

        if (!isMoving) {
            SetContentRootEnabled(false);
        }
        else {
            SetContentRootEnabled(true);
        }
    }

    private void SetContentRootEnabled(bool value)
    {
        contentRoot.gameObject.SetActive(value);
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

    private Vector2 CalculateOpenedPosition()
    {
        return CalculatePositionByAlpha(openedScreenPositionAlpha, openedPanelPositionAlpha);
    }

    private Vector2 CalculateClosedPosition()
    {
        return CalculatePositionByAlpha(closedScreenPositionAlpha, closedPanelPositionAlpha);
    }

    private Vector2 CalculatePositionByAlpha(Vector2 screenPostionAlpha, Vector2 panelPostionAlpha)
    {
        Vector2 resolution = new Vector2(Screen.width, Screen.height) / canvas.scaleFactor;
        float positionX = resolution.x * screenPostionAlpha.x;
        float positionY = resolution.y * screenPostionAlpha.y;

        Vector2 size = rectTransform.rect.size;
        float sizeCorrectionX = size.x * panelPostionAlpha.x;
        float sizeCorrectionY = size.y * panelPostionAlpha.y;

        return new Vector2(positionX, positionY) + new Vector2(sizeCorrectionX, sizeCorrectionY);
    }
}