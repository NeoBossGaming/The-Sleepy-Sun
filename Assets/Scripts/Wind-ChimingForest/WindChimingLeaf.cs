using UnityEngine;
using System.Collections;

/// <summary>
/// Represents a single lily-pad leaf in the Wind-Chiming Forest.
///
/// STRUCTURE EXPECTED IN SCENE:
///   WindChimingLeaf (root) — has Collider2D, this script
///     └── Visual (child)   — has SpriteRenderer
///
/// The ROOT never moves during a shake. Only the Visual child's localPosition
/// oscillates on X. This means a player parented to the root is NOT thrown around
/// visually while standing on a shaking leaf — clean separation of concerns.
///
/// Y scrolling: root position is driven by WindChimingGameManager.CurrentScrollOffset.
/// </summary>
public class WindChimingLeaf : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Child GameObject that holds the SpriteRenderer. This is what visually shakes.")]
    [SerializeField] private Transform      visual;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D     leafCollider;

    // -------------------------------------------------------------------------
    // Public state — polled by WindChimingPlayerController every frame
    // -------------------------------------------------------------------------
    public bool IsSafe { get; private set; } = true;

    // -------------------------------------------------------------------------
    // Scroll tracking
    // -------------------------------------------------------------------------
    private float originalWorldY; // designer-placed Y at scroll offset 0
    private float originalWorldX;
    private WindChimingGameManager gameManager;

    private bool      isShaking;
    private Transform occupant; // player transform currently parented here

    // -------------------------------------------------------------------------
    // Init / Reset  (called by GameManager, not Unity lifecycle)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Captures the designer-placed world position as the scroll-zero baseline.
    /// Called once by WindChimingGameManager.InitializeGame().
    /// </summary>
    public void InitializeLeaf(WindChimingGameManager manager)
    {
        gameManager    = manager;
        originalWorldY = transform.position.y;
        originalWorldX = transform.position.x;

        IsSafe    = true;
        isShaking = false;
        occupant  = null;

        if (visual != null) visual.localPosition = Vector3.zero;
        spriteRenderer.enabled = true;
        leafCollider.enabled   = true;
    }

    /// <summary>
    /// Stops all in-progress shaking and restores a clean safe state.
    /// Called by WindChimingGameManager on every checkpoint reset.
    /// </summary>
    public void ResetLeaf(WindChimingGameManager manager)
    {
        StopAllCoroutines();

        gameManager = manager;
        IsSafe      = true;
        isShaking   = false;
        occupant    = null;

        if (visual != null) visual.localPosition = Vector3.zero;
        spriteRenderer.enabled = true;
        leafCollider.enabled   = true;
    }

    // -------------------------------------------------------------------------
    // Scroll sync — runs every frame
    // -------------------------------------------------------------------------

    void Update()
    {
        if (gameManager == null) return;

        // Root only moves on Y to match the global scroll.
        // Visual handles its own local X offset during shakes.
        transform.position = new Vector3(
            originalWorldX,
            originalWorldY + gameManager.CurrentScrollOffset,
            transform.position.z
        );
    }

    // -------------------------------------------------------------------------
    // Shake / collapse — parameters supplied by GameManager for difficulty scaling
    // -------------------------------------------------------------------------

    /// <summary>
    /// Begins shake → collapse → respawn.
    /// shakeDuration and shakeAmount come from GameManager so difficulty
    /// scaling lives entirely in one place.
    /// No-ops if already mid-shake.
    /// </summary>
    public void TriggerShake(float shakeDuration, float shakeAmount, float collapsedDuration = 1.0f)
    {
        if (isShaking) return;
        StartCoroutine(ShakeSequence(shakeDuration, shakeAmount, collapsedDuration));
    }

    IEnumerator ShakeSequence(float shakeDuration, float shakeAmount, float collapsedDuration)
    {
        isShaking = true;
        float elapsed = 0f;

        // Phase 1: Visual shakes — root and player remain perfectly still
        while (elapsed < shakeDuration)
        {
            if (visual != null)
                visual.localPosition = new Vector3(Random.Range(-shakeAmount, shakeAmount), 0f, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Phase 2: Collapse
        if (visual != null) visual.localPosition = Vector3.zero;
        IsSafe                 = false;
        spriteRenderer.enabled = false;
        leafCollider.enabled   = false;

        yield return new WaitForSeconds(collapsedDuration);

        // Phase 3: Respawn
        IsSafe                 = true;
        isShaking              = false;
        spriteRenderer.enabled = true;
        leafCollider.enabled   = true;
    }

    // -------------------------------------------------------------------------
    // Occupant tracking — used by WindChimingPlayerController
    // -------------------------------------------------------------------------

    public void SetOccupant(Transform t) => occupant = t;
    public void ClearOccupant()          => occupant = null;
    public bool IsOccupied               => occupant != null;
}