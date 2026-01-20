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
    [SerializeField] private RectTransform slider;
    [SerializeField] private float slideTransitionSpeed = 1f;
    [SerializeField] private Vector2 closedPosition;

    [Header("Background")]
    [SerializeField] Image background;
    [SerializeField] float openedBackgroundAlpha = 1f;
    [SerializeField] float alphaTransitionSpeed = 1f;

    [Header("Buttons")]
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;

    private bool isOpened = false;
    private List<Transform> content = new List<Transform>();
    private Vector2 targetPosition = new Vector3();
    private Vector2 currentPosition = new Vector3();

    private int openedFrame = -1;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        FillContent();
        CloseSlidePanel();
        currentPosition = targetPosition;

        if (background) {
            Color color = background.color;
            color.a = 0f;
            background.color = color;
        }

        if (openButton)
            openButton.onClick.AddListener(OpenSlidePanel);
        if (closeButton)
            closeButton.onClick.AddListener(CloseSlidePanel);
    }

    private void Update()
    {
        UpdatePosition();
        if (background)
            UpdateBackground();
    }

    public void OnPress()
    {

    }

    public void OnRelease()
    {
        if (Time.frameCount == openedFrame)
            return;

        List<RaycastResult> results = new List<RaycastResult>();
        InputUtils.GetCurrentRaycastResults(results);
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
        targetPosition = new Vector2(0, slider.rect.height);
        background.raycastTarget = true;
        content.Add(background.transform);
        isOpened = true;
    }

    public void CloseSlidePanel()
    {
        targetPosition = closedPosition;
        background.raycastTarget = false;
        isOpened = false;
    }

    private void UpdatePosition()
    {
        currentPosition = math.lerp(currentPosition, targetPosition, slideTransitionSpeed * Time.deltaTime);
        rectTransform.anchoredPosition = currentPosition;
    }

    private void UpdateBackground()
    {
        Color color = background.color;
        float alpha = color.a;
        if (isOpened) {
            alpha = math.lerp(alpha, openedBackgroundAlpha, alphaTransitionSpeed * Time.deltaTime);
        }
        else {
            alpha = math.lerp(alpha, 0f, alphaTransitionSpeed * Time.deltaTime);
        }
        color.a = alpha;
        background.color = color;
    }

    public void SetOpenButton(Button button)
    {
        if (openButton) {
            //openButton.onClick.RemoveAllListeners();
            content.Remove(openButton.transform);
        }
        openButton = button;
        //openButton.onClick.AddListener(OpenSlidePanel);
        content.Add(openButton.transform);
    }

    public void SetCloseButton(Button button)
    {
        if (closeButton) {
            //closeButton.onClick.RemoveAllListeners();
            content.Remove(closeButton.transform);
        }
        closeButton = button;
        //closeButton.onClick.AddListener(CloseSlidePanel);
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
