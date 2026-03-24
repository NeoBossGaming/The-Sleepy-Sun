using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class WindChimingForestGameManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private WindChimingForestPlayerController playerController;
    [SerializeField] private Transform leafRowParent;
    [SerializeField] private GameManager globalGameManager;

    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private float winYThreshold = 50f;

    [Header("Leaf Settings")]
    [SerializeField] private GameObject leafPrefab;
    [SerializeField] private int leafCount = 6;
    [SerializeField] private float leafSpacing = 2f;

    [Header("Shake Cycle")]
    [SerializeField] private float shakeInterval = 3f;
    [SerializeField] private int leavesPerCycle = 2;
    [SerializeField] private float shakeDuration = 0.7f;
    [SerializeField] private float disappearDelay = 0.3f;

    [Header("Checkpoints")]
    [SerializeField] private float[] checkpointYOffsets;

    [Header("Umbra Interference")]
    [SerializeField] private float[] umbraYTriggers;
    [SerializeField] private float umbraScrollSpeedBonus = 0.8f;
    [SerializeField] private int umbraExtraLeavesPerCycle = 1;
    [SerializeField] private float umbraShakeDurationReduction = 0.15f;

    [Header("Events")]
    public UnityEvent onGameStart;
    public UnityEvent onWin;
    public UnityEvent onPlayerDied;
    public UnityEvent onUmbraInterference;

    private float yOffset = 0f;
    private Vector3 leafRowStartPosition;
    private bool isRunning = false;
    private bool isHandlingDeath = false;
    private int lastCheckpointIndex = -1;
    private bool[] umbraTriggered;
    private List<WindChimingForestLeaf> leaves = new List<WindChimingForestLeaf>();
    
    // Store reference to stop stacking
    private Coroutine shakeRoutine;

    void Awake()
    {
        leafRowStartPosition = leafRowParent.position;
        umbraTriggered = new bool[umbraYTriggers != null ? umbraYTriggers.Length : 0];
        StartGame();
    }

    // Switched to Update for smoother visual scrolling with player/leaves
    void Update()
    {
        if (!isRunning) return;

        ScrollWorld();
        CheckCheckpoints();
        CheckUmbraTriggers();
        CheckWinCondition();
        CheckPlayerDeath();
    }

    public void StartGame()
    {
        yOffset = 0f;
        lastCheckpointIndex = -1;
        isHandlingDeath = false;

        SpawnLeaves();
        playerController.SetCanMove(true);
        isRunning = true;

        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeCycleRoutine());
        onGameStart?.Invoke();
    }

    public void PauseGame()
    {
        isRunning = false;
        playerController.SetCanMove(false);
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
    }

    public void ResumeGame()
    {
        isRunning = true;
        playerController.SetCanMove(true);
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeCycleRoutine());
    }

    private void ScrollWorld()
    {
        yOffset += scrollSpeed * Time.deltaTime;
        leafRowParent.position = leafRowStartPosition + Vector3.up * yOffset;
    }

    private void SpawnLeaves()
    {
        foreach (var leaf in leaves)
            if (leaf != null) Destroy(leaf.gameObject);
        leaves.Clear();

        float totalWidth = leafSpacing * (leafCount - 1);
        float startX = -totalWidth / 2f;

        for (int i = 0; i < leafCount; i++)
        {
            float x = startX + i * leafSpacing;
            GameObject go = Instantiate(leafPrefab, leafRowParent);
            go.transform.localPosition = new Vector3(x, 0f, 0f);

            WindChimingForestLeaf leaf = go.GetComponent<WindChimingForestLeaf>();
            leaf.InitPosition(); // Call this to capture local position AFTER setting it
            leaves.Add(leaf);
        }

        int middleIndex = leafCount / 2;
        playerController.SetCurrentLeaf(leaves[middleIndex]);
    }

    private IEnumerator ShakeCycleRoutine()
    {
        while (isRunning)
        {
            yield return new WaitForSeconds(shakeInterval);
            TriggerShakeCycle();
        }
    }

    private void TriggerShakeCycle()
    {
        List<WindChimingForestLeaf> candidates = new List<WindChimingForestLeaf>();
        foreach (var leaf in leaves)
        {
            // Check if NOT already shaking to avoid logic overlaps
            if (leaf != null && leaf.IsActive && !leaf.HasPlayer && !leaf.IsShaking)
                candidates.Add(leaf);
        }

        int count = Mathf.Min(leavesPerCycle, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, candidates.Count);
            candidates[randomIndex].TriggerShake(shakeDuration, disappearDelay);
            candidates.RemoveAt(randomIndex);
        }
    }

    private void CheckCheckpoints()
    {
        if (checkpointYOffsets == null) return;
        for (int i = lastCheckpointIndex + 1; i < checkpointYOffsets.Length; i++)
        {
            if (yOffset >= checkpointYOffsets[i])
                lastCheckpointIndex = i;
        }
    }

    private void CheckPlayerDeath()
    {
        if (isHandlingDeath) return;

        WindChimingForestLeaf current = playerController.CurrentLeaf;
        if (current == null || !current.IsActive)
        {
            isHandlingDeath = true;
            onPlayerDied?.Invoke();
            StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        PauseGame();
        yield return new WaitForSeconds(0.6f);

        float respawnY = lastCheckpointIndex >= 0 ? checkpointYOffsets[lastCheckpointIndex] : 0f;
        yOffset = respawnY;
        leafRowParent.position = leafRowStartPosition + Vector3.up * yOffset;

        foreach (var leaf in leaves)
            if (leaf != null) leaf.Reactivate();

        int middleIndex = leafCount / 2;
        if (middleIndex < leaves.Count && leaves[middleIndex] != null)
            playerController.SetCurrentLeaf(leaves[middleIndex]);

        isHandlingDeath = false;
        ResumeGame();
    }

    private void CheckUmbraTriggers()
    {
        if (umbraYTriggers == null) return;
        for (int i = 0; i < umbraYTriggers.Length; i++)
        {
            if (!umbraTriggered[i] && yOffset >= umbraYTriggers[i])
            {
                umbraTriggered[i] = true;
                StartCoroutine(UmbraInterferenceRoutine());
            }
        }
    }

    private IEnumerator UmbraInterferenceRoutine()
    {
        scrollSpeed += umbraScrollSpeedBonus;
        leavesPerCycle += umbraExtraLeavesPerCycle;
        shakeDuration = Mathf.Max(0.3f, shakeDuration - umbraShakeDurationReduction);
        onUmbraInterference?.Invoke();
        yield return null;
    }

    private void CheckWinCondition()
    {
        if (yOffset < winYThreshold) return;
        isRunning = false;
        playerController.SetCanMove(false);
        globalGameManager?.SetSilverLeaf(true);
        onWin?.Invoke();
    }
}