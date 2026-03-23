using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Controls a single Simon Says stage (one room, one pattern, one door).
///
/// Stage flow:
///   1. Initialize() is called by the GameManager after spawning.
///   2. StartStage() displays the sequence to the player.
///   3. Player selects statues via SimonSaysPlayerController.
///   4. Correct full sequence → door opens → GameManager is notified.
///   5. Wrong input → sequence replays from the beginning.
///
/// Setup (Prefab):
///   - Assign all 5 statues in the statues array (order matters — it's their index).
///   - Assign doorAnimator if available (Animator parameter: "Open" trigger).
///   - Assign nextStageSpawnPoint: a Transform placed beyond the door.
/// </summary>
public class SimonSaysStage : MonoBehaviour
{
    [Header("Stage Configuration")]
    [SerializeField] private int sequenceLength = 5;

    [Header("Statues")]
    [SerializeField] private SimonSaysStatue[] statues;

    [Header("Door")]
    [SerializeField] private Animator doorAnimator;
    // Place this Transform in the scene beyond the door. The next stage spawns here.
    [SerializeField] public Transform nextStageSpawnPoint;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color activeColor = new Color(1f, 0.85f, 0.2f, 1f);  // golden
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color wrongColor   = new Color(0.9f, 0.2f, 0.2f, 1f);

    [Header("Timing")]
    [SerializeField] private float startDelay       = 1.0f; // pause before showing sequence
    [SerializeField] private float displayOnDuration  = 0.5f; // how long each statue lights up
    [SerializeField] private float displayOffDuration = 0.3f; // gap between each statue

    // --- State ---
    private List<int> sequence = new List<int>();
    private int playerStep     = 0;
    private bool isDisplaying  = false;
    private bool isCompleted   = false;
    private bool hasStarted    = false;

    private SimonSaysGameManager gameManager;

    // -------------------------------------------------------
    // Setup
    // -------------------------------------------------------

    /// <summary>
    /// Called by SimonSaysGameManager immediately after spawning this prefab.
    /// Generates the sequence and initializes all statues.
    /// </summary>
    public void Initialize(SimonSaysGameManager manager)
    {
        gameManager = manager;

        for (int i = 0; i < statues.Length; i++)
        {
            statues[i].Initialize(this, i);
            statues[i].SetColor(normalColor);
        }

        GenerateSequence();
    }

    private void GenerateSequence()
    {
        sequence.Clear();
        for (int i = 0; i < sequenceLength; i++)
            sequence.Add(Random.Range(0, statues.Length));
    }

    // -------------------------------------------------------
    // Stage Flow
    // -------------------------------------------------------

    /// <summary>
    /// Displays the sequence to the player. Safe to call multiple times — only runs once.
    /// Called by SimonSaysGameManager after a short delay.
    /// </summary>
    public void StartStage()
    {
        if (hasStarted || isCompleted) return;
        hasStarted = true;
        StartCoroutine(DisplaySequenceRoutine());
    }

    private IEnumerator DisplaySequenceRoutine()
    {
        isDisplaying = true;
        playerStep = 0;
        ResetAllColors();

        yield return new WaitForSeconds(startDelay);

        foreach (int index in sequence)
        {
            yield return StartCoroutine(
                statues[index].FlashColor(activeColor, displayOnDuration, normalColor)
            );
            yield return new WaitForSeconds(displayOffDuration);
        }

        isDisplaying = false;
        // Player can now select statues
    }

    // -------------------------------------------------------
    // Player Input Handling
    // -------------------------------------------------------

    /// <summary>
    /// Called by SimonSaysStatue when the player selects it.
    /// Validates against the current sequence step.
    /// </summary>
    public void PlayerSelectedStatue(int index)
    {
        // Ignore input during display or after completion
        if (!hasStarted || isDisplaying || isCompleted) return;

        if (index == sequence[playerStep])
        {
            // Correct
            StartCoroutine(statues[index].FlashColor(correctColor, 0.2f, normalColor));
            playerStep++;

            if (playerStep >= sequence.Count)
                StartCoroutine(StageCompletedRoutine());
        }
        else
        {
            // Wrong
            StartCoroutine(WrongInputRoutine());
        }
    }

    // -------------------------------------------------------
    // Outcome Routines
    // -------------------------------------------------------

    private IEnumerator StageCompletedRoutine()
    {
        isCompleted = true;
        isDisplaying = true; // block any late input

        // Blink all statues green twice as success feedback
        for (int i = 0; i < 2; i++)
        {
            SetAllColors(correctColor);
            yield return new WaitForSeconds(0.25f);
            ResetAllColors();
            yield return new WaitForSeconds(0.2f);
        }

        OpenDoor();

        yield return new WaitForSeconds(0.8f);

        gameManager.OnStageCompleted(this);
    }

    private IEnumerator WrongInputRoutine()
    {
        isDisplaying = true;
        playerStep = 0;

        // Flash all red once as failure feedback
        SetAllColors(wrongColor);
        yield return new WaitForSeconds(0.5f);
        ResetAllColors();
        yield return new WaitForSeconds(0.4f);

        // Replay the sequence so the player can try again
        isDisplaying = false;
        StartCoroutine(DisplaySequenceRoutine());
    }

    // -------------------------------------------------------
    // Door
    // -------------------------------------------------------

    private void OpenDoor()
    {
        if (doorAnimator != null)
        {
            // Animator needs a trigger parameter named "Open"
            doorAnimator.SetTrigger("Open");
        }
        else
        {
            // PLACEHOLDER — assign a doorAnimator in the Inspector,
            // or implement your own door logic here.
            Debug.Log($"[SimonSaysStage] Door opened on stage (no Animator assigned yet).");
        }
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    private void ResetAllColors() => SetAllColors(normalColor);

    private void SetAllColors(Color color)
    {
        foreach (var statue in statues)
            statue.SetColor(color);
    }
}