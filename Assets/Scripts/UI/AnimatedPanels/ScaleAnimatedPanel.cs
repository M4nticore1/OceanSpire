using Unity.Mathematics;
using UnityEngine;

public class ScaleAnimatedPanel : AnimatedPanel
{
    [SerializeField] private Vector3 closedScale = Vector3.zero;
    [SerializeField] private Vector3 openedScale = Vector3.one;

    public override void SetAnimationProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        RectTransform.localScale = math.lerp(closedScale, openedScale, progress);
    }

    protected override void HandleShown()
    {
        
    }

    protected override void HandleHidden()
    {
        
    }

    protected override bool ShouldSetContentInactive()
    {
        return false;
    }
}