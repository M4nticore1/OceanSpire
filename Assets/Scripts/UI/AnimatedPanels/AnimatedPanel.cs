using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class AnimatedPanel : MonoBehaviour, IOpenable
{
    private RectTransform rectTransform;
    public RectTransform RectTransform => rectTransform != null ? rectTransform : GetComponent<RectTransform>();

    [Header("Animation")]
    [SerializeField] private CloseMethod closeMethod = CloseMethod.Click;

    [SerializeField] private float animationSpeed = 10f;
    public float AnimationSpeed => animationSpeed;

    [SerializeField] private RectTransform contentRoot;
    public RectTransform ContentRoot => contentRoot;

    [SerializeField] private bool hideWhenClosed = false;
    public bool HideWhenClosed => hideWhenClosed;

    [SerializeField] private bool isShown = false;
    public bool IsShown => isShown;

    public bool IsUnderAnimation => IsShown ? animationProgress < 1f : animationProgress > 0f;

    private float animationProgress = 0f;

    private Vector2 pressPossition;
    private Vector2 releasePossition;
    private int openedFrame = 0;

    protected List<Transform> content = new List<Transform>();

    private InputListener inputListener => InputListener.Instance;

    public event Action OnShown;
    public event Action OnHidden;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    protected virtual void OnEnable()
    {
        if (inputListener != null) {
            InputListener.Instance.OnPressed += OnPress;
            InputListener.Instance.OnReleased += OnRelease;
        }
    }

    protected virtual void OnDisable()
    {
        if (inputListener != null) {
            InputListener.Instance.OnPressed -= OnPress;
            InputListener.Instance.OnReleased -= OnRelease;
        }
    }

    protected virtual void Start()
    {
        FillContent();
        StartCoroutine(UpdateContentRootEnabledEndOfFrame());
    }

    private void Update()
    {
        float targetProgress = isShown ? 1f : 0f;

        if (!Mathf.Approximately(animationProgress, targetProgress)) {
            animationProgress = Mathf.Lerp(animationProgress, targetProgress, animationSpeed * Time.deltaTime);

            if (Mathf.Abs(animationProgress - targetProgress) < 0.001f) {
                animationProgress = targetProgress;
            }

            SetAnimationProgress(animationProgress);
            UpdateContentRootEnabled();
        }
    }

    public abstract void SetAnimationProgress(float progress);

    protected abstract void HandleShown();

    protected abstract void HandleHidden();

    protected abstract bool ShouldSetContentInactive();

    public void Show()
    {
        isShown = true;
        openedFrame = Time.frameCount;

        HandleShown();
        OnShown?.Invoke();
    }

    public void Hide()
    {
        isShown = false;

        if (hideWhenClosed) {
            SetContentRootEnabled(true);
        }

        HandleHidden();
        OnHidden?.Invoke();
    }

    private void TryClose()
    {
        var results = new List<RaycastResult>();
        PointerUtils.GetRaycastUIResults(results);

        if (IsClickedOutsideMenu(results)) {
            Hide();
        }
    }

    private void FillContent()
    {
        content = GetComponentsInChildren<Transform>(true).ToList();
    }

    private void UpdateContentRootEnabled()
    {
        SetContentRootEnabled(ShouldSetContentInactive());
    }

    private void SetContentRootEnabled(bool value)
    {
        //contentRoot.gameObject.SetActive(value);
    }

    private void OnPress()
    {
        pressPossition = PointerUtils.GetCurrentInputPosition();
    }

    private void OnRelease()
    {
        if (!IsShown) return;
        if (Time.frameCount == openedFrame) return;
        if (closeMethod == CloseMethod.None) return;

        releasePossition = PointerUtils.GetCurrentInputPosition();

        if (closeMethod == CloseMethod.OnePointClick && releasePossition != pressPossition) return;
        if (InputListener.Instance.startPressedObject != PointerUtils.GetRaycastUIResult().gameObject) return;

        TryClose();
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

    private IEnumerator UpdateContentRootEnabledEndOfFrame()
    {
        yield return null;

        UpdateContentRootEnabled();
    }
}