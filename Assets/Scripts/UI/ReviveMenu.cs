using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReviveMenu : MonoBehaviour
{
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private TextMeshProUGUI citizenName;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI resmainingTime;

    private Human selectedHuman;

    private void OnEnable()
    {
        Human.onHumanSelected += OnHumanSelected;
        slidePanel.onClosed += OnClosed;
    }

    private void OnDisable()
    {
        Human.onHumanSelected -= OnHumanSelected;
        slidePanel.onClosed += OnClosed;
    }

    private void Open(Human human)
    {
        selectedHuman = human;
        citizenName.SetText(human.firstName + " " + human.lastName);

        slidePanel.Open();
    }

    private void Close()
    {
        slidePanel.Close();
    }

    private void OnClosed()
    {

    }

    private void OnHumanSelected(Human human)
    {
        if (human.currentStateEnum != HumanStateEnum.Citizen) return;
        if (human.Health.isAlive) return;

        Open(human);
    }

    private void OnHumanDeselected(Human human)
    {
        Close();
    }
}