using UnityEngine;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// Central controller for the Tall Grass stealth mini-game.
///
/// RESPONSIBILITIES:
///   - Activates ShadowBirds when the game starts.
///   - Manages the checkpoint system: stores the last reached checkpoint position.
///   - Handles player capture: respawns the player at the last checkpoint.
///   - Exposes a modular TriggerWin() method that any scene object can call
///     (a goal trigger zone, a collectible item, etc.).
///
/// FLOW CONTROL:
///   Nothing moves until StartGame() is called.
///   Use PauseGame() / ResumeGame() for cutscenes or menus.
/// </summary>
public class TallGrassGameManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------------------------

    [Header("Scene References")]
    [SerializeField] private TallGrassPlayerController playerController;
    [SerializeField] private TallGrassShadowBird[] shadowBirds;
    [SerializeField] private GameManager globalGameManager;

    [Header("Respawn")]
    [Tooltip("Where the player spawns at game start (before any checkpoint is reached).")]
    [SerializeField] private Transform defaultSpawnPoint;
    [Tooltip("Brief pause in seconds before the player reappears after capture.")]
    [SerializeField] private float respawnDelay = 0.8f;

    [Header("Events")]
    public UnityEvent onGameStart;
    public UnityEvent onPlayerCaptured;
    public UnityEvent onWin;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private bool isRunning = false;
    private bool isRespawning = false;

    // Last reached checkpoint (position to respawn at)
    private Vector3 currentRespawnPosition;
    private int lastCheckpointIndex = -1;

    // -------------------------------------------------------------------------
    // Unity Lifecycle (minimal)
    // -------------------------------------------------------------------------

    void Awake()
    {
        // Default respawn is at the defined start point
        currentRespawnPosition = defaultSpawnPoint != null
            ? defaultSpawnPoint.position
            : playerController.transform.position;
    }

    void Start()
    {
        StartGame();
    }

    // -------------------------------------------------------------------------
    // Public Flow Control API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Activates all birds and allows the player to move. Call after intro dialogue.
    /// </summary>
    public void StartGame()
    {
        isRunning = true;
        playerController.SetCanMove(true);
        foreach (var bird in shadowBirds)
        {   
            if (bird != null)
                bird.Activate(playerController);
        }

        onGameStart?.Invoke();
    }

    /// <summary>
    /// Freezes movement and detection. Safe to call during cutscenes.
    /// </summary>
    public void PauseGame()
    {
        isRunning = false;
        playerController.SetCanMove(false);

        foreach (var bird in shadowBirds)
            if (bird != null) bird.Deactivate();
    }

    /// <summary>
    /// Resumes from a pause. Re-activates birds.
    /// </summary>
    public void ResumeGame()
    {
        isRunning = true;
        playerController.SetCanMove(true);

        foreach (var bird in shadowBirds)
            if (bird != null) bird.Activate(playerController);
    }

    // -------------------------------------------------------------------------
    // Public Callbacks (called by TallGrassShadowBird and TallGrassCheckpoint)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called by a TallGrassShadowBird when it detects the player.
    /// Handles respawn logic.
    /// </summary>
    public void OnPlayerCaptured()
    {
        if (!isRunning || isRespawning) return;

        isRespawning = true;
        onPlayerCaptured?.Invoke();
        StartCoroutine(RespawnRoutine());
    }

    /// <summary>
    /// Called by a TallGrassCheckpoint trigger when the player walks through it.
    /// Updates the respawn position if this checkpoint is further along than the last.
    /// </summary>
    public void OnCheckpointReached(int index, Vector3 position)
    {
        if (index <= lastCheckpointIndex) return;

        lastCheckpointIndex = index;
        currentRespawnPosition = position;
    }

    // -------------------------------------------------------------------------
    // Public Win Trigger (modular — call from any scene object)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Triggers the win state. Can be called from:
    ///   - A GoalZone trigger object (OnTriggerEnter2D → gameManager.TriggerWin())
    ///   - A collectible item (on pickup → gameManager.TriggerWin())
    ///   - Any future mechanic without changing this script.
    /// </summary>
    public void TriggerWin()
    {
        if (!isRunning) return;

        isRunning = false;
        playerController.SetCanMove(false);

        foreach (var bird in shadowBirds)
            if (bird != null) bird.Deactivate();

        globalGameManager?.SetGoldenGiner(true); // Note: typo matches GameManager.cs intentionally
        onWin?.Invoke();
    }

    // -------------------------------------------------------------------------
    // Private — Respawn
    // -------------------------------------------------------------------------

    private IEnumerator RespawnRoutine()
    {
        PauseGame();

        yield return new WaitForSeconds(respawnDelay);

        // Move player back to last checkpoint
        playerController.transform.position = currentRespawnPosition;

        // Reset all birds so they can detect again
        foreach (var bird in shadowBirds)
            if (bird != null) bird.ResetCaptureFlag();

        isRespawning = false;
        ResumeGame();
    }
}