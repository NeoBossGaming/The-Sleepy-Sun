using UnityEngine;
using System.Collections;

/// <summary>
/// Represents a single lily-pad leaf in the Wind-Chiming Forest.
/// 
/// Y position is driven entirely by WindChimingGameManager.CurrentScrollOffset so that
/// all leaves scroll upward in sync as one unified field — no Rigidbody, no Translate.
/// 
/// Also acts as a JumpAnchor: the player can target and land on this leaf.
/// The GameManager tells this leaf WHEN to shake; this leaf handles HOW.
/// </summary>
public class WindChimingLeaf : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration    = 1.0f; // how long it shakes before collapsing
    [SerializeField] private float shakeAmount      = 0.1f; // max x offset during shake
    [SerializeField] private float collapsedDuration = 1.0f; // how long it stays gone

    // -------------------------------------------------------------------------
    // Public state — read by WindChimingPlayerController each frame
    // -------------------------------------------------------------------------
    public bool IsSafe { get; private set; } = true;

    // -------------------------------------------------------------------------
    // Private scroll tracking
    // -------------------------------------------------------------------------
    private float originalWorldY; // Y position when the game started (scroll offset = 0)
    private float originalWorldX; // X position never changes, kept for clean position sets
    private WindChimingGameManager gameManager;

    private float xShakeOffset; // applied on top of originalWorldX during shake
    private bool  isShaking;

    private Transform occupant; // the player transform currently parented to this leaf

    // -------------------------------------------------------------------------
    // Initialisation & Reset (called by GameManager, not Unity lifecycle)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called by WindChimingGameManager at game start.
    /// Captures the designer-placed world position as the scroll-zero baseline.
    /// </summary>
    public void InitializeLeaf(WindChimingGameManager manager)
    {
        gameManager   = manager;
        originalWorldY = transform.position.y; // pre-placed Y = scroll-zero Y
        originalWorldX = transform.position.x;

        IsSafe       = true;
        xShakeOffset = 0f;
        isShaking    = false;
        occupant     = null;

        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled     = true;
    }

    /// <summary>
    /// Called by WindChimingGameManager on checkpoint reset.
    /// Stops all in-progress shake coroutines and restores safe state.
    /// originalWorldX/Y are unchanged — the designer-placed position is permanent.
    /// </summary>
    public void ResetLeaf(WindChimingGameManager manager)
    {
        StopAllCoroutines();

        gameManager  = manager;
        IsSafe       = true;
        xShakeOffset = 0f;
        isShaking    = false;
        occupant     = null;

        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled     = true;
    }

    // -------------------------------------------------------------------------
    // Scroll sync — runs every frame
    // -------------------------------------------------------------------------

    void Update()
    {
        if (gameManager == null) return;

        // All leaves share the same CurrentScrollOffset so they scroll as one.
        // Formula: world Y = designer-placed Y + how far the world has scrolled.
        float syncedY = originalWorldY + gameManager.CurrentScrollOffset;
        transform.position = new Vector3(originalWorldX + xShakeOffset, syncedY, transform.position.z);
    }

    // -------------------------------------------------------------------------
    // Shake / collapse — driven by GameManager beat cycle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called by WindChimingGameManager. Begins the shake → collapse → respawn sequence.
    /// Ignored if the leaf is already shaking.
    /// </summary>
    public void TriggerShake()
    {
        if (isShaking) return;
        StartCoroutine(ShakeSequence());
    }

    IEnumerator ShakeSequence()
    {
        isShaking = true;
        float elapsed = 0f;

        // Phase 1: Shake — leaf wobbles but is STILL SAFE (player can stand on it)
        while (elapsed < shakeDuration)
        {
            xShakeOffset = Random.Range(-shakeAmount, shakeAmount);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Phase 2: Collapse — leaf disappears, becomes unsafe
        xShakeOffset = 0f;
        IsSafe = false;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled     = false;

        yield return new WaitForSeconds(collapsedDuration);

        // Phase 3: Respawn — leaf returns to normal
        IsSafe    = true;
        isShaking = false;
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled     = true;
    }

    // -------------------------------------------------------------------------
    // Occupant tracking — used by WindChimingPlayerController
    // -------------------------------------------------------------------------

    public void SetOccupant(Transform t) => occupant = t;
    public void ClearOccupant()          => occupant = null;
    public bool IsOccupied               => occupant != null;
}