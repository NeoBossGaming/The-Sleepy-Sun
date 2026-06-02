using UnityEngine;
using TMPro;

/// <summary>
/// Placed on each path entrance in the Village Hub.
/// 
/// When the player walks into the trigger zone, a prompt appears ("Press [Jump] to enter").
/// When the player presses Interact inside the zone:
///   - If unlocked: tells HubGameManager to load the target scene.
///   - If locked: shows a locked message instead.
/// 
/// Scene Setup:
/// - Add a Collider2D (set to Is Trigger) to the portal GameObject.
/// - Assign the target scene name in the Inspector.
/// - Assign the HubGameManager reference.
/// - Optionally assign promptUI and lockedMessageUI for visual feedback.
/// </summary>
public class HubPathPortal : MonoBehaviour, IHubInteractable
{
    [Header("Portal Settings")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private bool isLocked = false;

    [Header("References")]
    [SerializeField] private HubGameManager hubGameManager;

    [Header("UI Feedback (Optional)")]
    [SerializeField] private GameObject promptUI;         // "Press [Interact] to enter"
    [SerializeField] private GameObject lockedMessageUI; // "You need all ingredients first"

    private void Start()
    {
        HideAllUI();
    }

    /// <summary>
    /// Called by HubGameManager to lock/unlock this portal.
    /// </summary>
    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }

    /// <summary>
    /// Called by HubPlayerController when the player presses Interact inside the zone.
    /// </summary>
    public void Interact()
    {
        if (isLocked)
        {
            ShowLockedMessage();
        }
        else
        {
            hubGameManager.LoadScene(targetSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        HubPlayerController player = other.GetComponent<HubPlayerController>();
        if (player != null) player.SetInteractable(this);
        Debug.Log($"Player entered {gameObject.name} trigger. Portal is {(isLocked ? "locked" : "unlocked")}.");
        Interact();
        ShowPrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        HubPlayerController player = other.GetComponent<HubPlayerController>();
        if (player != null) player.ClearInteractable(this);

        HideAllUI();
    }

    private void ShowPrompt()
    {
        HideAllUI();
        if (promptUI != null) promptUI.SetActive(true);
    }

    private void ShowLockedMessage()
    {
        HideAllUI();
        if (lockedMessageUI != null) lockedMessageUI.SetActive(true);
    }

    private void HideAllUI()
    {
        if (promptUI != null) promptUI.SetActive(false);
        if (lockedMessageUI != null) lockedMessageUI.SetActive(false);
    }
}