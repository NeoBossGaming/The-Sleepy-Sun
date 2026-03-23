using UnityEngine;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// Manages the Tall Grass Path trial.
///
/// Responsibilities:
///   - Respawns the player when caught by a Shadow Bird
///   - Fires win condition when the player collects the Golden Ginger
///
/// Inspector Setup:
///   - Create an empty GameObject "TallGrassGameManager" in the scene
///   - Assign Player (TallGrassPlayerController) and Spawn Point (Transform at path start)
///   - Assign the scene's GameManager if present; otherwise it's found automatically
///   - Wire onPlayerCaught / onPlayerWin events to sound/UI triggers in the Inspector
/// </summary>
public class TallGrassGameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TallGrassPlayerController player;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameManager mainGameManager;

    [Header("Settings")]
    [SerializeField] private float respawnDelay = 1f;

    [Header("Events")]
    public UnityEvent onPlayerCaught; // Hook up: flash screen, play sfx, etc.
    public UnityEvent onPlayerWin;    // Hook up: cutscene trigger, dialogue, etc.

    private bool isProcessingDeath = false;
    private bool hasWon = false;

    void Start()
    {
        if (player == null)
            player = FindObjectOfType<TallGrassPlayerController>();

        if (mainGameManager == null)
            mainGameManager = FindObjectOfType<GameManager>();
    }

    /// <summary>
    /// Called by TallGrassFlyingBird when the player is spotted without cover.
    /// </summary>
    public void PlayerCaught()
    {
        if (isProcessingDeath || hasWon) return;
        StartCoroutine(RespawnRoutine());
    }

    /// <summary>
    /// Called by TallGrassGoalZone when the player reaches the Golden Ginger.
    /// </summary>
    public void PlayerReachedGoal()
    {
        if (hasWon) return;
        hasWon = true;

        // NOTE: SetGoldenGiner is a typo in GameManager.cs — fix to SetGoldenGinger when possible.
        mainGameManager?.SetGoldenGiner(true);
        onPlayerWin?.Invoke();
        Debug.Log("[TallGrass] Trial 4 Complete — Golden Ginger collected!");
    }

    private IEnumerator RespawnRoutine()
    {
        isProcessingDeath = true;
        onPlayerCaught?.Invoke();

        player.SetMovement(false);
        yield return new WaitForSeconds(respawnDelay);

        player.transform.position = spawnPoint.position;
        player.SetMovement(true);

        isProcessingDeath = false;
    }
}