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
    [SerializeField] private int baseDifficulty = 0; // Starts at 0 multiplier

    [Header("Difficulty Scaling")]
    [SerializeField] private float baseShakeDuration = 2.0f;
    [SerializeField] private float shakeDurationDecreasePerLevel = 0.2f; // Reduces shake time so they drop faster
    [SerializeField] private float minShakeDuration = 0.5f; // Absolute fastest it can drop

    [SerializeField] private float baseCollapseDuration = 1.0f;
    [SerializeField] private float collapseDurationIncreasePerLevel = 0.2f; // Stays dropped longer

    [SerializeField] private float baseShakeAmount = 0.1f;
    [SerializeField] private float shakeAmountIncreasePerLevel = 0.05f; // Shakes more violently

    [SerializeField] private float baseJumpSpeed = 6f;
    [SerializeField] private float jumpSpeedIncreasePerLevel = 1.5f; // Player jumps faster

    [Header("Scene References")]
    [SerializeField] private WindChimingLeaf[] allLeaves;
    [SerializeField] private WindChimingLeaf playerSpawnLeaf;

    [Header("Checkpoints (Scroll Offset Values)")]
    [SerializeField] private float[] checkpointScrollOffsets;

    [Header("Umbra Interference")]
    [SerializeField] private float umbraInterferenceOffset;
    [SerializeField] private int   umbraDifficultyBonus = 2; // Increases the difficulty multiplier during event
    [SerializeField] private float umbraDuration = 10f;

    [Header("Win Condition")]
    [SerializeField] private float finishScrollOffset;

    public float CurrentScrollOffset { get; private set; }
    
    // Public property so the player can read their current jump speed
    public float CurrentJumpSpeed => baseJumpSpeed + (currentDifficulty * jumpSpeedIncreasePerLevel);

    private WindChimingPlayerController player;
    private int   currentDifficulty;
    private float secondsPerBeat;
    private float beatTimer;
    private bool  gameRunning;
    private bool  umbraTriggered;
    private bool  finishTriggered;

    void Start()
    {
        player = FindFirstObjectByType<WindChimingPlayerController>();
        InitializeGame();
    }

    public void InitializeGame()
    {
        CurrentScrollOffset = 0f;
        currentDifficulty   = baseDifficulty;
        secondsPerBeat      = 60f / bpm;
        beatTimer           = 0f;
        umbraTriggered      = false;
        finishTriggered     = false;
        gameRunning         = false;

        foreach (var leaf in allLeaves)
            leaf.InitializeLeaf(this);

        player?.SpawnOnLeaf(playerSpawnLeaf);

        gameRunning = true;
    }

    void Update()
    {
        if (!gameRunning) return;

        CurrentScrollOffset += riseSpeed * Time.deltaTime;

        beatTimer += Time.deltaTime;
        if (beatTimer >= secondsPerBeat * 4f)
        {
            beatTimer = 0f;
            StartCoroutine(RunBeatCycle());
        }

        if (!umbraTriggered && CurrentScrollOffset >= umbraInterferenceOffset)
        {
            umbraTriggered = true;
            StartCoroutine(UmbraInterference());
        }

        if (!finishTriggered && CurrentScrollOffset >= finishScrollOffset)
        {
            finishTriggered = true;
            OnFinish();
        }
    }

    IEnumerator RunBeatCycle()
    {
        // 1. Calculate the current difficulty variables based on the current level
        float currentShakeDur = Mathf.Max(minShakeDuration, baseShakeDuration - (currentDifficulty * shakeDurationDecreasePerLevel));
        float currentShakeAmt = baseShakeAmount + (currentDifficulty * shakeAmountIncreasePerLevel);
        float currentCollapseDur = baseCollapseDuration + (currentDifficulty * collapseDurationIncreasePerLevel);

        // 2. Pick exactly ONE leaf to be the safe leaf
        int safeIndex = Random.Range(0, allLeaves.Length);
        WindChimingLeaf safeLeaf = allLeaves[safeIndex];

        // 3. Tell every OTHER leaf to shake and drop
        foreach (var leaf in allLeaves)
        {
            if (leaf != safeLeaf)
            {
                leaf.TriggerShake(currentShakeDur, currentShakeAmt, currentCollapseDur);
            }
        }
        yield return null;
    }

    IEnumerator UmbraInterference()
    {
        currentDifficulty += umbraDifficultyBonus;
        yield return new WaitForSeconds(umbraDuration);
        currentDifficulty -= umbraDifficultyBonus;
    }

    void OnFinish()
    {
        gameRunning = false;
        FindFirstObjectByType<GameManager>()?.SetSilverLeaf(true);
    }

    public void OnPlayerDied()
    {
        if (!gameRunning) return;
        gameRunning = false;
        StartCoroutine(ResetToCheckpoint());
    }

    IEnumerator ResetToCheckpoint()
    {
        float targetOffset = 0f;
        foreach (float cp in checkpointScrollOffsets)
        {
            if (cp <= CurrentScrollOffset)
                targetOffset = cp;
        }

        CurrentScrollOffset = targetOffset;

        foreach (var leaf in allLeaves)
            leaf.ResetLeaf(this);

        player?.SpawnOnLeaf(playerSpawnLeaf);

        currentDifficulty  = baseDifficulty;
        umbraTriggered     = targetOffset >= umbraInterferenceOffset;
        if (umbraTriggered) currentDifficulty += umbraDifficultyBonus;

        beatTimer = 0f;

        yield return null; 
        gameRunning = true;
    }
}