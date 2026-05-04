using UnityEngine;

public class GenderComponent : MonoBehaviour
{
    [SerializeField] private bool isMale = true;
    public bool IsMale => isMale;

    public void SetGender(bool isMale)
    {
        this.isMale = isMale;
    }
}