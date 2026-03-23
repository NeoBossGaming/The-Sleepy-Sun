using UnityEngine;

/// <summary>
/// Place at the MIDPOINT of the leaf row.
/// Fires the Umbra Interference event once when the player crosses it.
///
/// Per the GDD: "At the halfway point, the wind suddenly dies. All chimes go silent.
/// A dark wave passes over the forest — The Umbra's interference.
/// Several extra leaves begin shaking simultaneously."
///
/// Inspector Setup:
///   - Create an empty GameObject roughly in the middle of the leaf crossing
///   - Add a Collider2D, set Is Trigger = true
///   - Size it tall enough to block the player's path (they must pass through)
///   - Assign WindChimingForestGameManager
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WindChimingForestUmbraTrigger : MonoBehaviour
{
    [SerializeField] private WindChimingForestGameManager gameManager;

    private bool hasTriggered = false;

    void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;

        if (gameManager == null)
            gameManager = FindFirstObjectByType<WindChimingForestGameManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;

        hasTriggered = true;
        gameManager?.TriggerUmbraInterference();
        Debug.Log("[WindChimingForest] Umbra Trigger crossed — interference begins.");
    }
}