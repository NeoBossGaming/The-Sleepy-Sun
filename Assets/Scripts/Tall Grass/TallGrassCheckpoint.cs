using UnityEngine;

/// <summary>
/// A checkpoint trigger zone placed in the scene.
/// When the player walks through it, this script notifies the TallGrassGameManager
/// to update the respawn point.
///
/// SETUP: Attach to a GameObject with a Trigger Collider2D.
///        Assign a checkpointIndex that is unique and ordered from 0 upward.
/// </summary>
public class TallGrassCheckpoint : MonoBehaviour
{
    [SerializeField] private TallGrassGameManager gameManager;
    [SerializeField] private int checkpointIndex;

    // Optional: prevent the same checkpoint from being registered twice
    private bool hasBeenReached = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenReached) return;
        if (!other.CompareTag("Player")) return;

        hasBeenReached = true;
        gameManager.OnCheckpointReached(checkpointIndex, transform.position);
    }
}