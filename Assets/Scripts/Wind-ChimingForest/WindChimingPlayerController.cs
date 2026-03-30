using UnityEngine;
using System.Collections;

/// <summary>
/// Player controller for the Wind-Chiming Forest minigame.
/// Extends PlayerMovement.
///
/// HOW IT WORKS:
/// - The player is always parented to their current WindChimingLeaf.
/// - Since leaves drive their own Y via GameManager.CurrentScrollOffset, the player
///   rides upward automatically as a child — no extra scroll logic needed here.
/// - Horizontal movement modifies transform.localPosition (not rb.linearVelocity),
///   because the Rigidbody is set to Kinematic to avoid physics fighting the parent.
/// - Pressing Dash jumps to the nearest safe leaf in the facing direction.
/// - Death is only checked while NOT jumping, enabling close-call moments.
/// </summary>
public class WindChimingPlayerController : PlayerMovement
{
    // ---------------------------------------------------------------------------
    // Note: 'speed' inherited from PlayerMovement is intentionally unused here.
    // Local-position movement needs a much smaller value than velocity-based movement.
    // Use horizontalSpeed instead.
    // ---------------------------------------------------------------------------
    [Header("Horizontal Movement")]
    [SerializeField] private float horizontalSpeed = 3f;

    [Header("Leaf Detection")]
    [SerializeField] private float detectionRange = 2.0f;
    [SerializeField] private string leafTag = "Leaf";

    [Header("Jump Settings")]
    [SerializeField] private float jumpSpeed = 6f;

    [Header("References")]
    [SerializeField] private WindChimingGameManager gameManager;

    private WindChimingLeaf currentLeaf;
    private WindChimingLeaf targetedLeaf;
    private float facingDirection = 1f; // 1 = right, -1 = left
    private bool  isJumping;

    // -------------------------------------------------------------------------

    protected override void Start()
    {
        base.Start();

        // Kinematic so the Rigidbody never fights the parent-driven Y position
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (gameManager == null)
            gameManager = FindFirstObjectByType<WindChimingGameManager>();
    }

    // -------------------------------------------------------------------------
    // Input & movement overrides
    // -------------------------------------------------------------------------

    protected override void HandleInput()
    {
        // All input is blocked mid-jump (isJumping acts as a global gate)
        if (isJumping) return;

        // Track the last horizontal direction for leaf detection
        if (moveValue.x != 0f)
            facingDirection = Mathf.Sign(moveValue.x);

        // Scan for the nearest jumpable leaf each frame
        targetedLeaf = FindNearestSafeLeaf();

        // Dash = jump trigger (WasPressedThisFrame, so only fires once per press)
        if (playerInput.obtainMoveInputActions().dash && targetedLeaf != null)
            StartCoroutine(JumpToLeaf(targetedLeaf));

        // --- Death check ---
        // Not checked while isJumping is true, so a player who dashes off a leaf
        // just before it collapses survives (close-call feel).
        if (currentLeaf != null && !currentLeaf.IsSafe)
            Die();
    }

    protected override void ApplyMovement()
    {
        // Guard: must be grounded and allowed to move
        if (!canMove || isJumping) return;

        // Modify local X — the parent leaf handles world Y via the scroll system
        Vector3 local = transform.localPosition;
        local.x += moveValue.x * horizontalSpeed * Time.fixedDeltaTime;
        transform.localPosition = local;
    }

    // -------------------------------------------------------------------------
    // Leaf detection
    // -------------------------------------------------------------------------

    /// <summary>
    /// Scans an overlap circle in the facing direction and returns the nearest
    /// safe leaf that isn't the one the player is already standing on.
    /// Uses OverlapCircleAll so it never silently misses a closer candidate.
    /// </summary>
    private WindChimingLeaf FindNearestSafeLeaf()
    {
        Vector2 center = (Vector2)transform.position + new Vector2(facingDirection * detectionRange, 0f);
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, detectionRange * 0.5f);

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
    /// The target leaf is itself moving upward, so we chase its live transform
    /// position each frame — no extra math needed.
    /// 
    /// If the target leaf collapses mid-jump, the player still lands on it
    /// and dies immediately after (close call miss). This is intentional.
    /// </summary>
    IEnumerator JumpToLeaf(WindChimingLeaf target)
    {
        isJumping = true;
        SetCanMove(false);

        // Detach from current leaf — player is now in free-flight
        if (currentLeaf != null) currentLeaf.ClearOccupant();
        transform.SetParent(null);

        while (target != null && Vector3.Distance(transform.position, target.transform.position) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target.transform.position, // live position — the leaf is moving up
                jumpSpeed * Time.deltaTime
            );
            yield return null;
        }

        if (target != null)
            LandOnLeaf(target);

        isJumping = false;
        SetCanMove(true);
    }

    // -------------------------------------------------------------------------
    // Spawn & landing helpers (called by GameManager on start / checkpoint reset)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Places the player on the given leaf.
    /// Called by WindChimingGameManager at game start and after checkpoint resets.
    /// </summary>
    public void SpawnOnLeaf(WindChimingLeaf leaf)
    {
        StopAllCoroutines();
        isJumping = false;

        if (currentLeaf != null) currentLeaf.ClearOccupant();
        currentLeaf = null;

        LandOnLeaf(leaf);
        SetCanMove(true);
    }

    /// <summary>
    /// Parents the player to a leaf and zeroes local position so they sit at
    /// the leaf's pivot. Adjust the leaf's pivot in the scene if you want the
    /// player to sit above rather than at the centre.
    /// </summary>
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
        // Double-guard: ignore if already mid-jump (close call protection)
        if (isJumping) return;

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