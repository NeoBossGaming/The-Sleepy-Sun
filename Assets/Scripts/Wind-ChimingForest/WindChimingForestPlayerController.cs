using UnityEngine;
using System.Collections;
 
/// <summary>
/// Player controller for the Wind-Chiming Forest trial.
/// Handles horizontal movement and arc-jumping between WindChimingForestLeaf targets.
///
/// How jumping works:
///   1. Player moves left/right — their direction selects the nearest leaf in that direction.
///   2. Press Jump — an arc leap carries the player to the targeted leaf.
///   3. While jumping, all movement is locked.
///   4. If the current leaf collapses while the player is on it, PlayerFell() is called.
///
/// Inspector Setup:
///   - Attach to player root GameObject (also needs PlayerInput, Rigidbody2D via base class)
///   - Rigidbody2D: Gravity Scale = 0, Freeze Z Rotation, Body Type = Dynamic
///   - SpriteRenderer should be on a CHILD GameObject (not the root!) — drag it into Character Sprite
///     (This lets the child animate vertically for the arc while the root moves linearly)
///   - Set Leaf Tag to whatever tag you've given your WindChimingForestLeaf objects (default: "Leaf")
///   - Tag the player root as "Player"
/// </summary>
public class WindChimingForestPlayerController : PlayerMovement
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpDuration  = 0.6f;
    [SerializeField] private float jumpHeightMax = 1.5f;
    [SerializeField] private float leafYOffset   = 0f;   // Fine-tune vertical landing position on leaf
 
    [Header("Leaf Detection")]
    [SerializeField] private float leafDetectionRange = 2f;
    [SerializeField] private string leafTag = "Leaf";
 
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer characterSprite; // CHILD SpriteRenderer for arc animation
 
    // Publicly accessible so GameManager can reset it on respawn
    public WindChimingForestLeaf currentLeaf = null;
 
    private WindChimingForestLeaf targetLeaf = null;
    private float playerDirection = 1f;  // Last horizontal direction (+1 right, -1 left)
    private bool isJumping = false;
 
    private WindChimingForestGameManager gameManager;
 
    protected override void Start()
    {
        base.Start();
        gameManager = FindObjectOfType<WindChimingForestGameManager>();
    }
 
    protected override void Update()
    {
        base.Update(); // Reads moveValue from PlayerInput
 
        if (isJumping) return;
 
        // Track last horizontal direction for leaf detection
        if (moveValue.x != 0f)
            playerDirection = Mathf.Sign(moveValue.x);
 
        // If the leaf under the player collapses, report death
        if (currentLeaf != null && !currentLeaf.isSafe)
        {
            gameManager?.PlayerFell();
            return;
        }
 
        DetectTargetLeaf();
 
        // Trigger arc jump on Jump input if a valid leaf is targeted
        if (playerInput.obtainMoveInputActions().jump && targetLeaf != null)
            StartCoroutine(ExecuteJump(targetLeaf));
    }
 
    /// <summary>
    /// Overrides base: only apply horizontal movement in this trial.
    /// </summary>
    protected override void ApplyMovement()
    {
        rb.linearVelocity = canMove
            ? new Vector2(moveValue.x * moveSpeed, 0f)
            : Vector2.zero;
    }
 
    /// <summary>
    /// Scans for a WindChimingForestLeaf in the player's current facing direction.
    /// Uses OverlapCircleAll to correctly filter by tag.
    /// </summary>
    private void DetectTargetLeaf()
    {
        Vector2 detectionCenter = new Vector2(
            transform.position.x + (playerDirection * leafDetectionRange),
            transform.position.y
        );
 
        Collider2D[] hits = Physics2D.OverlapCircleAll(detectionCenter, leafDetectionRange / 2f);
 
        targetLeaf = null;
        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag(leafTag)) continue;
 
            WindChimingForestLeaf leaf = hit.GetComponent<WindChimingForestLeaf>();
            if (leaf != null && leaf.isSafe)
            {
                targetLeaf = leaf;
                break;
            }
        }
    }
 
    /// <summary>
    /// Arc jump from current position to the target leaf.
    /// Uses Kinematic override during flight so physics doesn't interfere.
    /// The child SpriteRenderer animates the visual arc; the root moves linearly.
    /// </summary>
    private System.Collections.IEnumerator ExecuteJump(WindChimingForestLeaf target)
    {
        isJumping  = true;
        currentLeaf = null;
        SetMovement(false);
 
        // Switch to Kinematic for manual position control during the leap
        RigidbodyType2D originalBodyType = rb.bodyType;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
 
        Vector3 startPos  = transform.position;
        Vector3 targetPos = new Vector3(
            target.transform.position.x,
            target.transform.position.y + leafYOffset,
            transform.position.z
        );
 
        float elapsed = 0f;
        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / jumpDuration);
 
            // Root moves linearly toward target
            transform.position = Vector3.Lerp(startPos, targetPos, t);
 
            // Child sprite moves along a parabolic arc (visual only)
            if (characterSprite != null)
            {
                float arc = jumpHeightMax * 4f * (t * (1f - t));
                Vector3 lp = characterSprite.transform.localPosition;
                characterSprite.transform.localPosition = new Vector3(lp.x, arc, lp.z);
            }
 
            yield return null;
        }
 
        // Snap and reset
        transform.position = targetPos;
        if (characterSprite != null)
        {
            Vector3 lp = characterSprite.transform.localPosition;
            characterSprite.transform.localPosition = new Vector3(lp.x, 0f, lp.z);
        }
 
        rb.bodyType = originalBodyType;
        currentLeaf = target;
        SetMovement(true);
        isJumping = false;
    }
 
    // Editor gizmo: shows the leaf detection zone
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector2 center = (Vector2)transform.position + new Vector2(playerDirection * leafDetectionRange, 0f);
        Gizmos.DrawWireSphere(center, leafDetectionRange / 2f);
    }
}