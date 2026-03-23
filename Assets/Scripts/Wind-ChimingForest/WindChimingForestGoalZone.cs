using UnityEngine;

/// <summary>
/// Place on a trigger collider at the Silver Leaf platform (far right end of the leaf row).
/// When the player enters, the trial ends.
///
/// Inspector Setup:
///   - Create a GameObject on the right shore platform
///   - Add Collider2D, set Is Trigger = true, size to cover the platform area
///   - Assign WindChimingForestGameManager
///   - Add the Silver Leaf visual prop as a child or sibling
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WindChimingForestGoalZone : MonoBehaviour
{
    [SerializeField] private WindChimingForestGameManager gameManager;

    void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;

        if (gameManager == null)
            gameManager = FindFirstObjectByType<WindChimingForestGameManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            gameManager?.PlayerReachedGoal();
    }
}