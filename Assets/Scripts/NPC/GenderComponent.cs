using UnityEngine;

public class GenderComponent : MonoBehaviour
{
    [SerializeField] private bool isMale = true;
    public bool IsMale => isMale;

    [SerializeField] private Sprite maleSprite;
    [SerializeField] private Sprite femaleSprite;

    public void SetGender(bool isMale)
    {
        this.isMale = isMale;
    }

    public Sprite GetGenderSprite()
    {
        return isMale ? maleSprite : femaleSprite;
    }
}