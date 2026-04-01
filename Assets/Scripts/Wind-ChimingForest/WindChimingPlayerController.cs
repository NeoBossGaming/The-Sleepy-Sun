using UnityEngine;
using System.Collections;

/// <summary>
/// Player controller for the Wind-Chiming Forest minigame.
/// Extends PlayerMovement.
///
/// KEY BEHAVIOURS:
/// - Player is always parented to their current WindChimingLeaf root.
///   Leaf root drives world Y via scroll; player rides for free as a child.
/// - Horizontal movement uses transform.localPosition (not rb velocity),
///   because rb is Kinematic to avoid fighting the parent-driven position.
/// - Dash input triggers a jump to the nearest safe leaf in the facing direction.
/// - Death is only checked while NOT jumping — close-call window is intentional.
/// - Jump speed is polled from WindChimingGameManager each frame (difficulty-scaled).
/// </summary>
public class WindChimingPlayerController : PlayerMovement
{
    [Header("Horizontal Speed")]
    [Tooltip("Use this instead of the inherited 'speed' field. Local-position movement needs a much smaller value.")]
    [SerializeField] private float horizontalSpeed = 3f;

    [Header("Leaf Detection")]
    [SerializeField] private float  detectionRange = 2.0f;
    [SerializeField] private string leafTag        = "Leaf";

    [Header("References")]
    [SerializeField] private WindChimingGameManager gameManager;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------
    private WindChimingLeaf currentLeaf;
    private WindChimingLeaf targetedLeaf;
    private float           facingDirection = 1f; // 1 = right, -1 = left
    private bool            isJumping;

    // -------------------------------------------------------------------------
    // Init
    // -------------------------------------------------------------------------

    protected override void Start()
    {
        base.Start();

        // Kinematic: Rigidbody never fights the parent-driven Y position
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (gameManager == null)
            gameManager = FindFirstObjectByType<WindChimingGameManager>();
    }

    // -------------------------------------------------------------------------
    // Input override
    // -------------------------------------------------------------------------

    protected override void HandleInput()
    {
        // All input blocked while airborne — this is also what enables close calls.
        // If the player dashes before their leaf collapses, isJumping = true before
        // the death check below can fire, so they survive.
        if (isJumping) return;

        // Track facing direction for leaf detection
        if (moveValue.x != 0f)
            facingDirection = Mathf.Sign(moveValue.x);

        // Scan for nearest jumpable leaf every frame
        targetedLeaf = FindNearestSafeLeaf();

        // Dash = jump
        if (playerInput.obtainMoveInputActions().dash && targetedLeaf != null)
            StartCoroutine(JumpToLeaf(targetedLeaf));

        // --- Death check ---
        // isJumping guard above means this block is skipped while airborne,
        // giving the player a close-call window if they jump just in time.
        if (currentLeaf != null && !currentLeaf.IsSafe)
            Die();
    }

    // -------------------------------------------------------------------------
    // Movement override — local-position based
    // -------------------------------------------------------------------------

    protected override void ApplyMovement()
    {
        if (!canMove || isJumping) return;

        // Modify local X only.
        // The parent leaf root handles world Y via scroll — touching it here
        // would de-sync the player from the scrolling system.
        Vector3 local = transform.localPosition;
        local.x += moveValue.x * horizontalSpeed * Time.fixedDeltaTime;
        transform.localPosition = local;
    }

    // -------------------------------------------------------------------------
    // Leaf detection
    // -------------------------------------------------------------------------

    /// <summary>
    /// Casts an overlap circle in the facing direction and returns the nearest
    /// safe leaf that isn't the player's current one.
    /// Uses OverlapCircleAll so no closer candidate is silently skipped.
    /// </summary>
    private WindChimingLeaf FindNearestSafeLeaf()
    {
        Vector2      center  = (Vector2)transform.position + new Vector2(facingDirection * detectionRange, 0f);
        Collider2D[] hits    = Physics2D.OverlapCircleAll(center, detectionRange * 0.5f);

        WindChimingLeaf nearest     = null;
        float           nearestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag(leafTag)) continue;

            WindChimingLeaf leaf = hit.GetComponent<WindChimingLeaf>();
            if (leaf == null || !leaf.IsSafe || leaf == currentLeaf) continue;

            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < nearestDist)
            {
                nearest     = leaf;
                nearestDist = dist;
            }
        }

        return nearest;
    }

    // -------------------------------------------------------------------------
    // Jump coroutine
    // -------------------------------------------------------------------------

    /// <summary>
    /// Slides the player toward a target leaf using MoveTowards.
    /// Jump speed is polled from GameManager each frame so the difficulty-scaled
    /// value is always up to date.
    /// The target is chased by live transform position — the leaf is moving upward
    /// during the jump, and we automatically follow it.
    /// </summary>
    IEnumerator JumpToLeaf(WindChimingLeaf target)
    {
        isJumping = true;
        SetCanMove(false);

        // Detach from current leaf — player is in free flight
        if (currentLeaf != null) currentLeaf.ClearOccupant();
        currentLeaf = null;
        transform.SetParent(null);

        while (target != null && Vector3.Distance(transform.position, target.transform.position) > 0.05f)
        {
            float speed = gameManager != null ? gameManager.CurrentJumpSpeed : 6f;
            transform.position = Vector3.MoveTowards(
                transform.position,
                target.transform.position, // live — leaf is scrolling up during jump
                speed * Time.deltaTime
            );
            yield return null;
        }

        if (target != null)
            LandOnLeaf(target);

        isJumping = false;
        SetCanMove(false);
    }

    // -------------------------------------------------------------------------
    // Spawn & landing  (called by GameManager)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Hard-places the player on a leaf. Called at game start and after resets.
    /// </summary>
    public void SpawnOnLeaf(WindChimingLeaf leaf)
    {
        StopAllCoroutines();
        isJumping = false;

        if (currentLeaf != null) currentLeaf.ClearOccupant();
        currentLeaf = null;

        LandOnLeaf(leaf);
        SetCanMove(false);
    }

    private void LandOnLeaf(WindChimingLeaf leaf)
    {
        currentLeaf = leaf;
        transform.SetParent(leaf.transform);
        transform.localPosition = Vector3.zero;
        leaf.SetOccupant(transform);
    }

    // -------------------------------------------------------------------------
    // Death
    // -------------------------------------------------------------------------

    private void Die()
    {
        if (isJumping) return; // redundant guard — HandleInput already skips, but be safe

        SetCanMove(false);

        if (currentLeaf != null) currentLeaf.ClearOccupant();
        currentLeaf = null;
        transform.SetParent(null);

        gameManager?.OnPlayerDied();
    }

    // -------------------------------------------------------------------------
    // Editor gizmo
    // -------------------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector2 center = (Vector2)transform.position + new Vector2(facingDirection * detectionRange, 0f);
        Gizmos.DrawWireSphere(center, detectionRange * 0.5f);
    }
}