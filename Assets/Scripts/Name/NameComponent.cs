using UnityEngine;

public class NameComponent : MonoBehaviour
{
    [SerializeField] private GenderComponent genderComponent;

    private LocalizationItem firstName;
    private LocalizationItem lastName;
    
    public void Init(NameData data)
    {
        SetFirstNameId(data.firstNameId, genderComponent.IsMale);
        SetLastNameId(data.lastNameId, genderComponent.IsMale);
    }

    public string GetName()
    {
        string firstName = LocalizationManager.Instance.GetText(this.firstName);
        string lastName = LocalizationManager.Instance.GetText(this.lastName);
        string name = firstName + " " + lastName;

        return name;
    }

    private void SetFirstNameId(int id, bool isMale)
    {
        firstName = isMale ? HumanNamesList.Instance.GetMaleFirstName(id) : HumanNamesList.Instance.GetFemaleFirstName(id);
    }

    private void SetLastNameId(int id, bool isMale)
    {
        lastName = isMale ? HumanNamesList.Instance.GetMaleLastName(id) : HumanNamesList.Instance.GetFemaleLastName(id);
    }
}