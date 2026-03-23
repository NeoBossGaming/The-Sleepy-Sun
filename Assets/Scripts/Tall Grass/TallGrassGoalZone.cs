using UnityEngine;

/// <summary>
/// Place this on a trigger collider at the far end of the Tall Grass Path.
/// When the player enters it, the Golden Ginger is collected and the trial ends.
///
/// Inspector Setup:
///   - Create a GameObject at the path's end (where the Golden Ginger prop is)
///   - Add a Collider2D, set Is Trigger = true
///   - Add this script and assign TallGrassGameManager
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TallGrassGoalZone : MonoBehaviour
{
    [SerializeField] private TallGrassGameManager gameManager;

    void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;

        if (gameManager == null)
            gameManager = FindObjectOfType<TallGrassGameManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            gameManager?.PlayerReachedGoal();
    }
}