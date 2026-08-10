using UnityEngine;

[CreateAssetMenu(fileName = "AnimationParam", menuName = "Scriptable Objects/AnimationParam")]
public class AnimationParam : ScriptableObject
{
    [SerializeField] private string paramName;
    public string ParamName => paramName;

    public int Hash { get; private set; }

    private void OnEnable()
    {
        UpdateHash();
    }

    private void OnValidate()
    {
        UpdateHash();
    }

    private void UpdateHash()
    {
        if (!string.IsNullOrEmpty(paramName)) {
            Hash = Animator.StringToHash(paramName);
        }
    }

    public static implicit operator int(AnimationParam param)
    {
        return param.Hash;
    }
}
