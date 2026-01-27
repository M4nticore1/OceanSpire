using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlidePanel : MonoBehaviour, IInputListenable
{
    private RectTransform rectTransform;

    [Header("Slide")]
    [SerializeField] private float slideTransitionSpeed = 1f;
    [SerializeField] private Vector2 openedPosition;
    [SerializeField] private Vector2 closedPosition;

    [Header("Background")]
    [SerializeField] Image background;
    [SerializeField] float openedBackgroundAlpha = 1f;
    [SerializeField] float alphaTransitionSpeed = 1f;

    [Header("Buttons")]
    [SerializeField] private CustomSelectable openButton;
    [SerializeField] private CustomSelectable closeButton;

    private bool isOpened = false;
    private bool isMoving = false;
    private List<Transform> content = new List<Transform>();
    private Vector2 targetPosition = new Vector3();

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

    }

    public void OnRelease()
    {
        if (!isOpened) return;
        if (Time.frameCount == openedFrame) return;

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
        openedFrame = Time.frameCount;
        background.raycastTarget = true;
        content.Add(background.transform);
        targetPosition = openedPosition;
        isOpened = true;
        isMoving = true;
        onOpened?.Invoke();
    }

    public void CloseSlidePanel()
    {
        targetPosition = closedPosition;
        background.raycastTarget = false;
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

    public void SetOpenButton(CustomSelectable button)
    {
        if (openButton) {
            content.Remove(openButton.transform);
        }
        openButton = button;
        content.Add(openButton.transform);
    }

    public void SetCloseButton(CustomSelectable button)
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
}
