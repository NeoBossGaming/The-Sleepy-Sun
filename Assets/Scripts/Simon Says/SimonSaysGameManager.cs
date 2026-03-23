using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the entire Simon Says minigame:
///   - Spawns stage prefabs one at a time.
///   - Listens for stage completion and spawns the next one.
///   - Fires the win condition to GameManager when all stages are done.
///
/// Setup:
///   1. Assign stagePrefab (must have SimonSaysStage component).
///   2. Assign initialSpawnPoint (where the very first stage appears).
///   3. Set totalStages to match your level design.
///   4. Assign mainGameManager (the scene's GameManager) for the win condition.
/// </summary>
public class SimonSaysGameManager : MonoBehaviour
{
    [Header("Stage Configuration")]
    [SerializeField] private GameObject stagePrefab;
    [SerializeField] private Transform  initialSpawnPoint;
    [SerializeField] private int        totalStages = 5;

    [Header("Timing")]
    [Tooltip("Delay (seconds) after a stage is cleared before the next one spawns.")]
    [SerializeField] private float delayBeforeNextStageSpawn = 1.5f;

    [Header("Win Condition")]
    [SerializeField] private GameManager mainGameManager;

    // --- State ---
    private int currentStageIndex     = 0;
    private SimonSaysStage currentStage;

    // -------------------------------------------------------

    private void Start()
    {
        SpawnStage(initialSpawnPoint.position);
    }

    // -------------------------------------------------------
    // Stage Lifecycle
    // -------------------------------------------------------

    private void SpawnStage(Vector3 position)
    {
        GameObject obj = Instantiate(stagePrefab, position, Quaternion.identity);
        currentStage = obj.GetComponent<SimonSaysStage>();
        currentStage.Initialize(this);
    }

    /// <summary>
    /// Called by SimonSaysStage when the player completes a stage.
    /// </summary>
    public void OnStageCompleted(SimonSaysStage completedStage)
    {
        currentStageIndex++;
        Debug.Log($"[SimonSaysGameManager] Stage {currentStageIndex} / {totalStages} complete.");

        if (currentStageIndex >= totalStages)
        {
            OnAllStagesCompleted();
            return;
        }

        // Determine where the next stage should spawn
        Vector3 nextSpawnPos = completedStage.nextStageSpawnPoint != null
            ? completedStage.nextStageSpawnPoint.position
            : initialSpawnPoint.position;

        if (completedStage.nextStageSpawnPoint == null)
            Debug.LogWarning("[SimonSaysGameManager] nextStageSpawnPoint not set on completed stage — using initialSpawnPoint as fallback.");

        StartCoroutine(DelayedSpawnNextStage(nextSpawnPos));
    }

    private IEnumerator DelayedSpawnNextStage(Vector3 spawnPos)
    {
        yield return new WaitForSeconds(delayBeforeNextStageSpawn);
        SpawnStage(spawnPos);
    }

    // -------------------------------------------------------
    // Win Condition
    // -------------------------------------------------------

    private void OnAllStagesCompleted()
    {
        Debug.Log("[SimonSaysGameManager] All stages complete — Cave Nectar obtained!");

        if (mainGameManager != null)
            mainGameManager.SetCaveNectar(true);
        else
            Debug.LogWarning("[SimonSaysGameManager] mainGameManager not assigned — win condition not fired.");
    }

    // -------------------------------------------------------
    // Public Accessors (for UI / debug)
    // -------------------------------------------------------

    public int GetCurrentStageIndex() => currentStageIndex;
    public int GetTotalStages()       => totalStages;
}