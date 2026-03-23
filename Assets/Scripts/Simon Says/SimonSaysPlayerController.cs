using UnityEngine;

/// <summary>
/// Player controller for the Simon Says minigame.
/// Inherits standard 2D movement from PlayerMovement.
/// Adds statue selection: press Dash (Space) while standing near a statue to select it.
/// </summary>
public class SimonSaysPlayerController : PlayerMovement
{
    // The statue the player is currently standing next to (set by SimonSaysStatue triggers).
    private SimonSaysStatue currentStatueInRange;

    protected override void HandleInput()
    {
        PlayerInput.inputValues input = playerInput.obtainMoveInputActions();

        // Press Dash/Space to select the statue the player is standing on.
        if (input.dash && currentStatueInRange != null)
        {
            currentStatueInRange.TrySelect();
        }
    }

    /// <summary>
    /// Called by SimonSaysStatue.OnTriggerEnter2D when the player enters its zone.
    /// </summary>
    public void SetStatueInRange(SimonSaysStatue statue)
    {
        currentStatueInRange = statue;
    }

    /// <summary>
    /// Called by SimonSaysStatue.OnTriggerExit2D when the player leaves its zone.
    /// Only clears if it's the same statue (handles edge cases with overlapping triggers).
    /// </summary>
    public void ClearStatueInRange(SimonSaysStatue statue)
    {
        if (currentStatueInRange == statue)
            currentStatueInRange = null;
    }
}