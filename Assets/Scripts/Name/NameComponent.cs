using System.Collections.Generic;
using UnityEngine;

public class NameComponent : MonoBehaviour, ILocalizable
{
    [SerializeField] private GenderComponent genderComponent;

    public int FirstNameId { get; private set; } = 0;
    public int LastNameId { get; private set; } = 0;

    private LocalizationItem firstName;
    private LocalizationItem lastName;
    
    public void Init()
    {
        Init(NameData.Default());
    }

    public void Init(NameData nameData)
    {
        if (nameData == null) {
            Debug.LogError("data is not valid");
            Init();
            return;
        }

        SetFirstNameId(nameData.FirstNameId, genderComponent.IsMale);
        SetLastNameId(nameData.LastNameId, genderComponent.IsMale);
    }

    public string GetName()
    {
        string firstName = LocalizationManager.Instance.GetText(this.firstName);
        string lastName = LocalizationManager.Instance.GetText(this.lastName);
        string name = firstName + " " + lastName;

        return name;
    }

    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            { "firstName", GetFirstNameText() },
            { "lastName", GetLastNameText() },
        };
    }

    private void SetFirstNameId(int id, bool isMale)
    {
        FirstNameId = id;
        firstName = isMale ? HumanNamesList.Instance.GetMaleFirstName(id) : HumanNamesList.Instance.GetFemaleFirstName(id);
    }

    private void SetLastNameId(int id, bool isMale)
    {
        LastNameId = id;
        lastName = isMale ? HumanNamesList.Instance.GetMaleLastName(id) : HumanNamesList.Instance.GetFemaleLastName(id);
    }

    private string GetFirstNameText()
    {
        return LocalizationManager.Instance.GetText(firstName);
    }

    private string GetLastNameText()
    {
        return LocalizationManager.Instance.GetText(lastName);
    }
}