using UnityEngine;
using System.Collections;

/// <summary>
/// Represents a single statue in a Simon Says stage.
/// 
/// Responsibilities:
/// - Flash its color when told by the stage (during display or feedback).
/// - Detect when the player is standing in its trigger zone.
/// - Notify the parent stage when the player selects it.
/// 
/// Setup: Assign a SpriteRenderer. Add a 2D trigger Collider for the interaction zone.
/// </summary>
public class SimonSaysStatue : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private SimonSaysStage parentStage;
    private int statueIndex;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Called by SimonSaysStage after spawning to register this statue's index.
    /// Must be called before the stage starts.
    /// </summary>
    public void Initialize(SimonSaysStage stage, int index)
    {
        parentStage = stage;
        statueIndex = index;
    }

    /// <summary>
    /// Called by SimonSaysPlayerController when the player presses the select key.
    /// Forwards the selection to the parent stage.
    /// </summary>
    public void TrySelect()
    {
        parentStage.PlayerSelectedStatue(statueIndex);
    }

    // --- Visual ---

    public void SetColor(Color color)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }

    /// <summary>
    /// Flashes to a color for a duration, then returns to a specified color.
    /// Used by the stage for display and feedback sequences.
    /// </summary>
    public IEnumerator FlashColor(Color flashColor, float duration, Color returnColor)
    {
        SetColor(flashColor);
        yield return new WaitForSeconds(duration);
        SetColor(returnColor);
    }

    // --- Trigger Detection ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SimonSaysPlayerController player = other.GetComponent<SimonSaysPlayerController>();
            player?.SetStatueInRange(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SimonSaysPlayerController player = other.GetComponent<SimonSaysPlayerController>();
            player?.ClearStatueInRange(this);
        }
    }
}