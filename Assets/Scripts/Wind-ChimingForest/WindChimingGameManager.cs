using UnityEngine;
using System.Collections;

/// <summary>
/// Central manager for the Wind-Chiming Forest minigame.
/// 
/// Owns: scroll offset, beat cycle, difficulty, umbra interference, checkpoints, and win/lose events.
/// Leaves read CurrentScrollOffset every frame to sync their Y position.
/// Player is told where to spawn by this manager.
/// </summary>
public class WindChimingGameManager : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private float riseSpeed = 2f;

    [Header("Beat Settings")]
    [SerializeField] private float bpm = 120f;
    [SerializeField] private int baseDifficulty = 2;

    [Header("Scene References")]
    [SerializeField] private WindChimingLeaf[] allLeaves;
    [SerializeField] private WindChimingLeaf playerSpawnLeaf;

    // ---------------------------------------------------------------------------
    // Checkpoints
    // Set these in the Inspector as scroll offset values (world units scrolled).
    // Example: 0, 5, 10 means checkpoints at start, 5 units scrolled, 10 units.
    // ---------------------------------------------------------------------------
    [Header("Checkpoints (Scroll Offset Values)")]
    [SerializeField] private float[] checkpointScrollOffsets;

    [Header("Umbra Interference")]
    [SerializeField] private float umbraInterferenceOffset;   // scroll offset that triggers the event
    [SerializeField] private int   umbraDifficultyBonus = 2;  // extra leaves that shake during event
    [SerializeField] private float umbraDuration = 10f;       // how long the event lasts (seconds)

    [Header("Win Condition")]
    [SerializeField] private float finishScrollOffset; // reaching this offset ends the minigame

    // ---------------------------------------------------------------------------
    // Public read-only scroll value — leaves poll this every Update to sync Y.
    // ---------------------------------------------------------------------------
    public float CurrentScrollOffset { get; private set; }

    private WindChimingPlayerController player;
    private int   currentDifficulty;
    private float secondsPerBeat;
    private float beatTimer;
    private bool  gameRunning;
    private bool  umbraTriggered;
    private bool  finishTriggered;

    // -------------------------------------------------------------------------

    void Start()
    {
        player = FindFirstObjectByType<WindChimingPlayerController>();
        InitializeGame();
    }

    /// <summary>
    /// Full game initialisation. Called at Start and can be called again externally
    /// if you ever want a hard restart from scroll offset 0.
    /// </summary>
    public void InitializeGame()
    {
        CurrentScrollOffset = 0f;
        currentDifficulty   = baseDifficulty;
        secondsPerBeat      = 60f / bpm;
        beatTimer           = 0f;
        umbraTriggered      = false;
        finishTriggered     = false;
        gameRunning         = false; // keep false until everything is ready

        foreach (var leaf in allLeaves)
            leaf.InitializeLeaf(this);

        player?.SpawnOnLeaf(playerSpawnLeaf);

        gameRunning = true;
    }

    void Update()
    {
        if (!gameRunning) return;

        // --- Scroll the world upward ---
        CurrentScrollOffset += riseSpeed * Time.deltaTime;

        // --- Beat cycle: every 4 beats, trigger leaf shakes ---
        beatTimer += Time.deltaTime;
        if (beatTimer >= secondsPerBeat * 4f)
        {
            beatTimer = 0f;
            StartCoroutine(RunBeatCycle());
        }

        // --- Umbra interference triggers at a designer-set scroll offset ---
        if (!umbraTriggered && CurrentScrollOffset >= umbraInterferenceOffset)
        {
            umbraTriggered = true;
            StartCoroutine(UmbraInterference());
        }

        // --- Win condition ---
        if (!finishTriggered && CurrentScrollOffset >= finishScrollOffset)
        {
            finishTriggered = true;
            OnFinish();
        }
    }

    // -------------------------------------------------------------------------
    // Beat cycle
    // -------------------------------------------------------------------------

    IEnumerator RunBeatCycle()
    {
        // Pick 'currentDifficulty' random leaves and tell each to shake.
        // Duplicates are allowed — the same leaf can be picked twice, but
        // WindChimingLeaf.TriggerShake() guards against double-shaking.
        for (int i = 0; i < currentDifficulty; i++)
        {
            int index = Random.Range(0, allLeaves.Length);
            allLeaves[index].TriggerShake();
        }
        yield return null;
    }

    // -------------------------------------------------------------------------
    // Umbra interference
    // -------------------------------------------------------------------------

    IEnumerator UmbraInterference()
    {
        // Temporarily increase difficulty, then restore it
        currentDifficulty += umbraDifficultyBonus;
        yield return new WaitForSeconds(umbraDuration);
        currentDifficulty -= umbraDifficultyBonus;
    }

    // -------------------------------------------------------------------------
    // Win / Lose
    // -------------------------------------------------------------------------

    void OnFinish()
    {
        gameRunning = false;
        // PLACEHOLDER — replace this with a proper event / scene transition later
        FindFirstObjectByType<GameManager>()?.SetSilverLeaf(true);
    }

    /// <summary>
    /// Called by WindChimingPlayerController when the player lands on an unsafe leaf.
    /// </summary>
    public void OnPlayerDied()
    {
        if (!gameRunning) return;
        gameRunning = false;
        StartCoroutine(ResetToCheckpoint());
    }

    IEnumerator ResetToCheckpoint()
    {
        // Find the last checkpoint the player passed (highest offset <= current)
        float targetOffset = 0f;
        foreach (float cp in checkpointScrollOffsets)
        {
            if (cp <= CurrentScrollOffset)
                targetOffset = cp;
        }

        // Snap scroll back to checkpoint
        CurrentScrollOffset = targetOffset;

        // Reset all leaves to clean state
        foreach (var leaf in allLeaves)
            leaf.ResetLeaf(this);

        // Reset player to spawn leaf
        player?.SpawnOnLeaf(playerSpawnLeaf);

        // Recalculate difficulty — umbra may already be active at this checkpoint
        currentDifficulty  = baseDifficulty;
        umbraTriggered     = targetOffset >= umbraInterferenceOffset;
        if (umbraTriggered) currentDifficulty += umbraDifficultyBonus;

        beatTimer = 0f;

        yield return null; // One frame pause so all leaf positions update before unblocking
        gameRunning = true;
    }
}