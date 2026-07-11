using UnityEngine;

public class BoatProgressDisplayController : ProgressDisplayController
{
    [SerializeField] private Boat boat;

    private IProgressable currentProgressable;

    protected override void Subscribe()
    {
        boat.OnStateEntered += OnBoatStateEntered;
        boat.OnStateExited += OnBoatStateExited;
    }

    protected override void Unsubscribe()
    {
        boat.OnStateEntered -= OnBoatStateEntered;
        boat.OnStateExited -= OnBoatStateExited;
    }

    private void Update()
    {
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        if (currentProgressable == null) return;

        ProgressDisplay.SetProgress(currentProgressable.GetProgress());
    }

    private void OnBoatStateEntered(BoatState state)
    {
        if (state is IProgressable progressable) {
            currentProgressable = progressable;
            ProgressDisplay.Display();
            UpdateProgress();
        }
    }

    private void OnBoatStateExited(BoatState state)
    {
        if (state is IProgressable progressable && progressable == currentProgressable) {
            currentProgressable = null;
            ProgressDisplay.Hide();
        }
    }
}