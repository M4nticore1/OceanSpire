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

public class SlideAnimatedPanel : AnimatedPanel
{
    [SerializeField] private Canvas canvas;

    [Header("Screen Position")]
    [SerializeField] private Vector2 openedScreenPositionAlpha = new Vector2(0f, 0.5f);
    [SerializeField] private Vector2 closedScreenPositionAlpha = new Vector2(0f, 0.0f);

    [Header("Panel Position")]
    [SerializeField] private Vector2 openedPanelPositionAlpha = new Vector2(0f, 0.5f);
    [SerializeField] private Vector2 closedPanelPositionAlpha = new Vector2(0f, 0.0f);

    [Header("Background")]
    [SerializeField] Image background;
    [SerializeField] float openedBackgroundAlpha = 0.5f;
    [SerializeField] float alphaTransitionSpeed = 10f;

    private Vector2 openedPosition;
    private Vector2 closedPosition;
    private Vector2 targetPosition;

    protected override void Start()
    {
        base.Start();

        if (background != null) {
            var color = background.color;
            color.a = 0f;
            background.color = color;
            background.raycastTarget = false;
        }
    }

    public override void SetAnimationProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        RectTransform.anchoredPosition = math.lerp(closedPosition, openedPosition, progress);
        UpdateBackgroundColor();
    }

    protected override void HandleShown()
    {
        openedPosition = CalculateOpenedPosition();
        targetPosition = openedPosition;

        if (background != null) {
            background.raycastTarget = true;
            content.Add(background.transform);
        }
    }

    protected override void HandleHidden()
    {
        closedPosition = CalculateClosedPosition();
        targetPosition = closedPosition;

        if (background != null) {
            background.raycastTarget = false;
        }
    }

    protected override bool ShouldSetContentInactive()
    {
        if (!HideWhenClosed) return false;
        if (RectTransform.anchoredPosition != targetPosition) return false;
        if (IsUnderAnimation) return false;

        return true;
    }

    private void UpdateBackgroundColor()
    {
        if (background == null) return;

        var color = background.color;
        if (IsShown) {
            color.a = math.lerp(color.a, openedBackgroundAlpha, alphaTransitionSpeed * Time.deltaTime);
        }
        else {
            color.a = math.lerp(color.a, 0f, alphaTransitionSpeed * Time.deltaTime);
        }

        background.color = color;
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
        var scale = canvas.scaleFactor;
        if (scale <= 0f) {
            scale = 1f;
        }

        var resolution = new Vector2(Screen.width, Screen.height) / scale;
        var positionX = resolution.x * screenPostionAlpha.x;
        var positionY = resolution.y * screenPostionAlpha.y;

        var size = RectTransform.rect.size;
        var sizeCorrectionX = size.x * panelPostionAlpha.x;
        var sizeCorrectionY = size.y * panelPostionAlpha.y;

        return new Vector2(positionX, positionY) + new Vector2(sizeCorrectionX, sizeCorrectionY);
    }
}