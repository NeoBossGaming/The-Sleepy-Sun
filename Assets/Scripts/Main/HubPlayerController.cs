using UnityEngine;

/// <summary>
/// Hub-specific player controller.
/// Extends PlayerMovement with interact input for talking to NPCs and entering paths.
/// 
/// Broadcasts OnInteractPressed so nearby interactables (HubPathPortal, HubNPC)
/// can listen without this script needing to know about them directly.
/// </summary>
public class HubPlayerController : PlayerMovement
{
    // Any script implementing IHubInteractable that registers itself
    // will be called when the player presses Interact inside its zone.
    private IHubInteractable currentInteractable;

    protected override void HandleInput()
    {
        if (playerInput.obtainMoveInputActions().interact)
        {
            currentInteractable?.Interact();
        }
    }

    /// <summary>
    /// Called by IHubInteractable objects when the player enters their trigger zone.
    /// </summary>
    public void SetInteractable(IHubInteractable interactable)
    {
        currentInteractable = interactable;
    }

    /// <summary>
    /// Called by IHubInteractable objects when the player leaves their trigger zone.
    /// Only clears if the leaving object is the currently registered one.
    /// </summary>
    public void ClearInteractable(IHubInteractable interactable)
    {
        if (currentInteractable == interactable)
            currentInteractable = null;
    }
}

/// <summary>
/// Interface for anything in the hub the player can interact with.
/// Implement this on HubPathPortal, HubNPC, etc.
/// </summary>
public interface IHubInteractable
{
    void Interact();
}