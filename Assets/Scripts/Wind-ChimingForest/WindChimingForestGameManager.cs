using UnityEngine;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// Manages the Wind-Chiming Forest rhythm trial.
///
/// Responsibilities:
///   - Runs the BPM-based beat cycle that triggers leaf shaking
///   - Handles Umbra Interference (called by WindChimingForestUmbraTrigger at midpoint)
///   - Respawns the player when they fall
///   - Fires the win condition when the player reaches the Silver Leaf platform
///
/// Inspector Setup:
///   - Create an empty GameObject "WindChimingForestGameManager"
///   - Drag ALL WindChimingForestLeaf objects into the All Leaves array
///   - Assign Player (WindChimingForestPlayerController)
///   - Create an empty GameObject "RespawnPoint" at the left shore start position; assign it
///   - Assign the scene's GameManager
///   - Wire UnityEvents to audio/visual triggers as needed
/// </summary>
public class WindChimingForestGameManager : MonoBehaviour
{
    [Header("Leaves")]
    [SerializeField] private WindChimingForestLeaf[] allLeaves;

    [Header("Rhythm")]
    [SerializeField] private float bpm = 120f;
    [SerializeField] private int   cycleBeats = 4;   // Beat cycles between each shake event

    [Header("Difficulty")]
    [SerializeField] private int baseDifficulty = 2; // Leaves shaken per cycle at normal difficulty
    private int currentDifficulty;

    [Header("Umbra Interference")]
    [Tooltip("Extra leaves shaken during Umbra Interference on top of baseDifficulty")]
    [SerializeField] private int   umbraExtraDifficulty      = 3;
    [SerializeField] private float umbraInterferenceDuration  = 10f;
    public UnityEvent onUmbraInterference;    // Hook to: play Umbra audio sting, show dark wave VFX
    public UnityEvent onUmbraInterferenceEnd; // Hook to: restore normal music/visuals

    [Header("References")]
    [SerializeField] private WindChimingForestPlayerController player;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private GameManager mainGameManager;

    [Header("Events")]
    public UnityEvent onPlayerFell;  // Hook to: screen flash, "oof" sound
    public UnityEvent onPlayerWin;   // Hook to: victory cutscene, dialogue

    private float beatTimer = 0f;
    private float secondsPerBeat;
    private bool isGameActive       = true;
    private bool hasWon             = false;
    private bool isProcessingDeath  = false;

    void Start()
    {
        currentDifficulty = baseDifficulty;
        secondsPerBeat    = 60f / bpm;

        if (player == null)
            player = FindFirstObjectByType<WindChimingForestPlayerController>();

        if (mainGameManager == null)
            mainGameManager = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        if (!isGameActive) return;

        beatTimer += Time.deltaTime;
        if (beatTimer >= secondsPerBeat * cycleBeats)
        {
            beatTimer = 0f;
            RunShakeCycle();
        }
    }

    /// <summary>
    /// Picks 'currentDifficulty' random leaves and triggers their shake sequence.
    /// The same leaf can be picked multiple times, but TriggerShake() ignores duplicate calls.
    /// </summary>
    private void RunShakeCycle()
    {
        if (allLeaves == null || allLeaves.Length == 0) return;

        for (int i = 0; i < currentDifficulty; i++)
        {
            int index = Random.Range(0, allLeaves.Length);
            allLeaves[index].TriggerShake();
        }
    }

    /// <summary>
    /// Called by WindChimingForestUmbraTrigger when the player crosses the midpoint.
    /// Spikes difficulty for umbraInterferenceDuration seconds, then returns to normal.
    /// </summary>
    public void TriggerUmbraInterference()
    {
        StartCoroutine(UmbraInterferenceRoutine());
    }

    private IEnumerator UmbraInterferenceRoutine()
    {
        currentDifficulty = baseDifficulty + umbraExtraDifficulty;
        onUmbraInterference?.Invoke();
        Debug.Log("[WindChimingForest] Umbra Interference! Difficulty spiked.");

        yield return new WaitForSeconds(umbraInterferenceDuration);

        currentDifficulty = baseDifficulty;
        onUmbraInterferenceEnd?.Invoke();
        Debug.Log("[WindChimingForest] Umbra Interference ended. Difficulty restored.");
    }

    /// <summary>
    /// Called by WindChimingForestPlayerController when the player's current leaf collapses.
    /// </summary>
    public void PlayerFell()
    {
        if (isProcessingDeath || hasWon) return;
        StartCoroutine(RespawnRoutine());
    }

    /// <summary>
    /// Called by WindChimingForestGoalZone when the player reaches the Silver Leaf platform.
    /// </summary>
    public void PlayerReachedGoal()
    {
        if (hasWon) return;
        hasWon      = true;
        isGameActive = false;

        mainGameManager?.SetSilverLeaf(true);
        onPlayerWin?.Invoke();
        Debug.Log("[WindChimingForest] Trial 1 Complete — Silver Leaf collected!");
    }

    private IEnumerator RespawnRoutine()
    {
        isProcessingDeath = true;
        onPlayerFell?.Invoke();

        yield return new WaitForSeconds(1f);

        // Reset player position and clear the current leaf reference
        player.transform.position = respawnPoint.position;
        player.currentLeaf = null;

        isProcessingDeath = false;
    }
}